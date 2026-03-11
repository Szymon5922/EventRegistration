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

variable "service_bus_queue_name" {
  type = string
}

variable "tags" {
  type    = map(string)
  default = {}
}