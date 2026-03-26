resource "azurerm_windows_web_app" "app" {
  name                = var.web_app_name
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  service_plan_id     = data.azurerm_service_plan.plan.id

  https_only                                     = true
  client_affinity_enabled                        = true
  ftp_publish_basic_authentication_enabled       = false
  webdeploy_publish_basic_authentication_enabled = false

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on                         = false
    ftps_state                        = "FtpsOnly"
    http2_enabled                     = false
    minimum_tls_version               = "1.2"
    scm_minimum_tls_version           = "1.2"
    use_32_bit_worker                 = true
    websockets_enabled                = false
    ip_restriction_default_action     = "Allow"
    scm_ip_restriction_default_action = "Allow"

    application_stack {
      current_stack  = "dotnet"
      dotnet_version = "v10.0"
    }
  }

  app_settings = {
    "KeyVault__Uri"                                    = "https://${var.key_vault_name}.vault.azure.net/"
    "ServiceBus__FullyQualifiedNamespace"              = "${azurerm_servicebus_namespace.sb.name}.servicebus.windows.net"
    "ServiceBus__RegistrationCompletedQueueName"       = var.registration_completed_queue_name
    "ServiceBus__ReminderQueueName"                    = var.reminder_queue_name
    "APPINSIGHTS_INSTRUMENTATIONKEY"                   = data.azurerm_application_insights.ai.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING"            = data.azurerm_application_insights.ai.connection_string
    "ApplicationInsightsAgent_EXTENSION_VERSION"       = "~2"
    "XDT_MicrosoftApplicationInsights_Mode"            = "recommended"
    "APPINSIGHTS_PROFILERFEATURE_VERSION"              = "1.0.0"
    "DiagnosticServices_EXTENSION_VERSION"             = "~3"
    "APPINSIGHTS_SNAPSHOTFEATURE_VERSION"              = "1.0.0"
    "SnapshotDebugger_EXTENSION_VERSION"               = "disabled"
    "InstrumentationEngine_EXTENSION_VERSION"          = "disabled"
    "XDT_MicrosoftApplicationInsights_BaseExtensions"  = "disabled"
    "XDT_MicrosoftApplicationInsights_PreemptSdk"      = "disabled"
    "XDT_MicrosoftApplicationInsights_Java"            = "1"
    "XDT_MicrosoftApplicationInsights_NodeJS"          = "1"
  }

  sticky_settings {
    app_setting_names = [
      "APPINSIGHTS_INSTRUMENTATIONKEY",
      "APPLICATIONINSIGHTS_CONNECTION_STRING",
      "APPINSIGHTS_PROFILERFEATURE_VERSION",
      "APPINSIGHTS_SNAPSHOTFEATURE_VERSION",
      "ApplicationInsightsAgent_EXTENSION_VERSION",
      "XDT_MicrosoftApplicationInsights_BaseExtensions",
      "DiagnosticServices_EXTENSION_VERSION",
      "InstrumentationEngine_EXTENSION_VERSION",
      "SnapshotDebugger_EXTENSION_VERSION",
      "XDT_MicrosoftApplicationInsights_Mode",
      "XDT_MicrosoftApplicationInsights_PreemptSdk",
      "APPLICATIONINSIGHTS_CONFIGURATION_CONTENT",
      "XDT_MicrosoftApplicationInsightsJava",
      "XDT_MicrosoftApplicationInsights_NodeJS",
    ]
  }

  tags = merge(var.tags, {
    "hidden-link: /app-insights-resource-id" = data.azurerm_application_insights.ai.id
  })

  lifecycle {
    ignore_changes = [
      tags["hidden-link: /app-insights-resource-id"]
    ]
  }
}

resource "azurerm_servicebus_namespace" "sb" {
  name                = var.service_bus_namespace_name
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  sku                 = "Standard"

  tags = var.tags
}

resource "azurerm_servicebus_queue" "registration_completed" {
  name         = var.registration_completed_queue_name
  namespace_id = azurerm_servicebus_namespace.sb.id

  max_delivery_count = 10
}

resource "azurerm_servicebus_queue" "reminder" {
  name         = var.reminder_queue_name
  namespace_id = azurerm_servicebus_namespace.sb.id

  max_delivery_count = 10
}

resource "azurerm_role_assignment" "webapp_servicebus_sender" {
  scope                = azurerm_servicebus_namespace.sb.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = azurerm_windows_web_app.app.identity[0].principal_id
}

resource "azurerm_role_assignment" "function_servicebus_receiver" {
  scope                = azurerm_servicebus_namespace.sb.id
  role_definition_name = "Azure Service Bus Data Receiver"
  principal_id         = azurerm_windows_function_app.mail_functions.identity[0].principal_id
}

resource "azurerm_storage_account" "function_storage" {
  name                     = var.function_storage_account_name
  resource_group_name      = data.azurerm_resource_group.rg.name
  location                 = data.azurerm_resource_group.rg.location
  account_tier             = "Standard"
  account_replication_type = "LRS"

  tags = var.tags
}

resource "azurerm_service_plan" "function_plan" {
  name                = var.function_service_plan_name
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  os_type             = "Windows"
  sku_name            = "Y1"

  tags = var.tags
}

resource "azurerm_windows_function_app" "mail_functions" {
  name                = var.function_app_name
  location            = data.azurerm_resource_group.rg.location
  resource_group_name = data.azurerm_resource_group.rg.name
  service_plan_id     = azurerm_service_plan.function_plan.id

  storage_account_name       = azurerm_storage_account.function_storage.name
  storage_account_access_key = azurerm_storage_account.function_storage.primary_access_key

  https_only = true

  identity {
    type = "SystemAssigned"
  }

  site_config {
    always_on = false

    application_stack {
      dotnet_version = "v10.0"
    }
  }

  app_settings = {
    "FUNCTIONS_WORKER_RUNTIME"              = "dotnet-isolated"
    "ServiceBus__fullyQualifiedNamespace"   = "${azurerm_servicebus_namespace.sb.name}.servicebus.windows.net"
    "RegistrationCompletedQueue"            = var.registration_completed_queue_name
    "ReminderQueue"                         = var.reminder_queue_name
    "SqlConnectionString"                   = "@Microsoft.KeyVault(SecretUri=https://${var.key_vault_name}.vault.azure.net/secrets/${var.sql_connection_string_secret_name}/)"
    "App__BaseUrl"                          = "https://${azurerm_windows_web_app.app.default_hostname}"
    "AcsEmailFrom"                          = "${azurerm_email_communication_service_domain_sender_username.mail_sender.name}@${azurerm_email_communication_service_domain.mail_domain.from_sender_domain}"
    "AcsEmailConnectionString"              = "@Microsoft.KeyVault(SecretUri=${azurerm_key_vault_secret.acs_email_connection_string.versionless_id})"
    "APPINSIGHTS_INSTRUMENTATIONKEY"        = data.azurerm_application_insights.ai.instrumentation_key
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = data.azurerm_application_insights.ai.connection_string
  }

  tags = var.tags
}

resource "azurerm_email_communication_service" "mail_service" {
  name                = var.acs_email_service_name
  resource_group_name = data.azurerm_resource_group.rg.name
  data_location       = var.acs_email_data_location

  tags = var.tags
}

resource "azurerm_email_communication_service_domain" "mail_domain" {
  name             = var.acs_email_domain_name
  email_service_id = azurerm_email_communication_service.mail_service.id
  domain_management = "AzureManaged"

  tags = var.tags
}

resource "azurerm_email_communication_service_domain_sender_username" "mail_sender" {
  name                    = var.acs_email_sender_username
  email_service_domain_id = azurerm_email_communication_service_domain.mail_domain.id
}

resource "azurerm_key_vault_secret" "acs_email_connection_string" {
  name         = "acs-email-connection-string"
  value        = var.acs_email_connection_string
  key_vault_id = data.azurerm_key_vault.kv.id
}

resource "azurerm_role_assignment" "function_key_vault_secrets_user" {
  scope                = data.azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_windows_function_app.mail_functions.identity[0].principal_id
}

resource "azurerm_role_assignment" "webapp_key_vault_secrets_user" {
  scope                = data.azurerm_key_vault.kv.id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_windows_web_app.app.identity[0].principal_id
}
