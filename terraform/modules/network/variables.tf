variable "environment" {
  description = "Environment name (e.g. prod, staging)"
  type        = string
}

variable "name_suffix" {
  description = "Suffix appended to resource names (\"\" for prod, \"-staging\" for staging)"
  type        = string
}

variable "vpc_cidr" {
  description = "CIDR for the VPC"
  type        = string
}

variable "public_subnet_cidr" {
  description = "CIDR for the public subnet (EC2)"
  type        = string
}
