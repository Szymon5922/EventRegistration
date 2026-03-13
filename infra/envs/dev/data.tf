data "azurerm_resource_group" "rg" {
  name = var.resource_group_name
}

data "azurerm_service_plan" "plan" {
  name                = var.service_plan_name
  resource_group_name = data.azurerm_resource_group.rg.name
}

data "azurerm_application_insights" "ai" {
  name                = var.application_insights_name
  resource_group_name = data.azurerm_resource_group.rg.name
}

data "azurerm_key_vault" "kv" {
  name                = var.key_vault_name
  resource_group_name = data.azurerm_resource_group.rg.name
}