variable "environment" {
  description = "Environment name (e.g. prod, staging)"
  type        = string
}

variable "name_suffix" {
  description = "Suffix appended to resource names (\"\" for prod, \"-staging\" for staging)"
  type        = string
}
