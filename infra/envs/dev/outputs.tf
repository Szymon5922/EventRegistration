output "web_app_id" {
  value = azurerm_windows_web_app.app.id
}

output "web_app_default_hostname" {
  value = azurerm_windows_web_app.app.default_hostname
}

output "service_bus_namespace_id" {
  value = azurerm_servicebus_namespace.sb.id
}

output "service_bus_queue_id" {
  value = azurerm_servicebus_queue.email_queue.id
}

output "key_vault_id" {
  value = data.azurerm_key_vault.kv.id
}