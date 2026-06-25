variable "environment" {
  description = "Environment name (e.g. prod, staging)"
  type        = string
}

variable "name_suffix" {
  description = "Suffix appended to resource names (\"\" for prod, \"-staging\" for staging)"
  type        = string
}

variable "aws_region" {
  description = "AWS region (used in the OAC name and origin domain)"
  type        = string
}

variable "web_bucket_name" {
  description = "Name of the web frontend S3 bucket (origin). The regional domain and ARN are derived from it to avoid a storage<->cdn dependency cycle."
  type        = string
}
