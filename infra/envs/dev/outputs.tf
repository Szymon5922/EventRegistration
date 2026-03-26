output "web_app_id" {
  value = azurerm_windows_web_app.app.id
}

output "web_app_default_hostname" {
  value = azurerm_windows_web_app.app.default_hostname
}

output "service_bus_namespace_id" {
  value = azurerm_servicebus_namespace.sb.id
}

output "key_vault_id" {
  value = data.azurerm_key_vault.kv.id
}

output "function_app_name" {
  value = azurerm_windows_function_app.mail_functions.name
}

output "registration_completed_queue_name" {
  value = azurerm_servicebus_queue.registration_completed.name
}

output "reminder_queue_name" {
  value = azurerm_servicebus_queue.reminder.name
}

output "acs_sender_address" {
  value = "${azurerm_email_communication_service_domain_sender_username.mail_sender.name}@${azurerm_email_communication_service_domain.mail_domain.from_sender_domain}"
}
