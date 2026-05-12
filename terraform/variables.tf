variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-northeast-1"
}

variable "vpc_cidr" {
  description = "CIDR for the VPC"
  type        = string
  default     = "10.0.0.0/16"
}

variable "public_subnet_cidr" {
  description = "CIDR for the public subnet (EC2)"
  type        = string
  default     = "10.0.1.0/24"
}

variable "key_name" {
  description = "Name of an existing EC2 key pair for SSH (optional)"
  type        = string
  default     = null
}

variable "environment" {
  description = "Environment tag (e.g. prod, dev)"
  type        = string
  default     = "prod"
}

variable "lambda_sharp_layer_arn" {
  description = "ARN (including version) of the sharp Lambda layer used by all image-resizing functions. Must be in the same region as the Lambda functions."
  type        = string
  default     = "arn:aws:lambda:us-east-1:249550149516:layer:sharp:1"
}

variable "enable_s3_lambda_notifications" {
  description = "Set to true only after the Lambda functions have been migrated to var.aws_region. S3 event notifications require the target Lambda to reside in the same region as the bucket; leaving this false defers the apply until the migration is complete."
  type        = bool
  default     = false
}
