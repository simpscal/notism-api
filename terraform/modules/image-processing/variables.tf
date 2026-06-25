variable "environment" {
  description = "Environment name (e.g. prod, staging)"
  type        = string
}

variable "name_suffix" {
  description = "Suffix appended to resource names (\"\" for prod, \"-staging\" for staging)"
  type        = string
}

variable "aws_region" {
  description = "AWS region (passed to the function as REGION and used for the sharp layer ARN)"
  type        = string
}

variable "lambda_package_path" {
  description = "Path (relative to the calling root) to the deployment zip. Code is deployed by CI/CD; Terraform manages configuration only."
  type        = string
  default     = "./lambda-packages/notism-image-resizing.zip"
}

variable "destination_bucket" {
  description = "Bucket the resize variants are written to (the public storage bucket name)"
  type        = string
}

variable "private_storage_arn" {
  description = "ARN of the private storage S3 bucket (source events + IAM scope)"
  type        = string
}

variable "public_storage_arn" {
  description = "ARN of the public storage S3 bucket (IAM scope)"
  type        = string
}
