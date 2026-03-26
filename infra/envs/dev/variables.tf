variable "subscription_id" {
  type = string
}

variable "location" {
  type = string
}

variable "resource_group_name" {
  type = string
}

variable "web_app_name" {
  type = string
}

variable "service_plan_name" {
  type = string
}

variable "application_insights_name" {
  type = string
}

variable "key_vault_name" {
  type = string
}

variable "service_bus_namespace_name" {
  type = string
}

variable "registration_completed_queue_name" {
  type = string
}

variable "reminder_queue_name" {
  type = string
}

variable "tags" {
  type    = map(string)
  default = {}
}

variable "function_app_name" {
  type = string
}

variable "function_storage_account_name" {
  type = string
}

variable "acs_email_service_name" {
  type = string
}

variable "acs_email_domain_name" {
  type = string
}

variable "acs_email_sender_username" {
  type = string
}

variable "acs_email_connection_string" {
  type      = string
  sensitive = true
}

variable "sql_connection_string_secret_name" {
  type    = string
  default = "SqlConnectionString"
}
