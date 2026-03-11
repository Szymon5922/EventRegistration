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
      "APPLICATIONINSIGHTS_CONNECTION_STRING ",
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

resource "azurerm_servicebus_queue" "email_queue" {
  name         = var.service_bus_queue_name
  namespace_id = azurerm_servicebus_namespace.sb.id

  max_delivery_count = 10
}

resource "azurerm_role_assignment" "webapp_servicebus_sender" {
  scope                = azurerm_servicebus_namespace.sb.id
  role_definition_name = "Azure Service Bus Data Sender"
  principal_id         = azurerm_windows_web_app.app.identity[0].principal_id
}