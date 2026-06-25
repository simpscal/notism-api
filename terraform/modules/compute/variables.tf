variable "environment" {
  description = "Environment name (e.g. prod, staging). Used for the root-device Environment tag."
  type        = string
}

variable "name_suffix" {
  description = "Suffix appended to resource names (\"\" for prod, \"-staging\" for staging)"
  type        = string
}

variable "instance_type" {
  description = "EC2 instance type"
  type        = string
  default     = "t4g.micro"
}

variable "key_name" {
  description = "Name of an existing EC2 key pair for SSH (optional)"
  type        = string
  default     = null
}

variable "subnet_id" {
  description = "ID of the public subnet to launch the instance in"
  type        = string
}

variable "security_group_id" {
  description = "ID of the EC2 security group"
  type        = string
}

variable "private_storage_arn" {
  description = "ARN of the private storage S3 bucket (IAM scope)"
  type        = string
}

variable "public_storage_arn" {
  description = "ARN of the public storage S3 bucket (IAM scope)"
  type        = string
}
