subscription_id           = "9d677ffc-c757-498a-acf4-004e4eb2a826"
location                  = "polandcentral"
resource_group_name       = "rg-registration-dev-plc-001"
web_app_name              = "app-registration-wev-dev-001"
service_plan_name         = "asp-registration-plc"
application_insights_name = "ai-registration-wev-dev-001"
key_vault_name            = "kv-registration-dev-plc"
service_bus_namespace_name = "sb-registration-dev-plc-001"
service_bus_queue_name     = "email-queue"

tags = {
  environment = "dev"
  project     = "registration"
  managedBy   = "terraform"
}