variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-northeast-1"
}

variable "vpc_id" {
  description = "ID of the VPC the compute resources live in"
  type        = string
}

variable "public_subnet_id" {
  description = "ID of the public subnet for the EC2 instance"
  type        = string
}

variable "key_name" {
  description = "Name of an existing EC2 key pair for SSH (optional)"
  type        = string
  default     = null
}

variable "private_storage_arn" {
  description = "ARN of the private storage S3 bucket (for EC2 + Lambda IAM policies)"
  type        = string
}

variable "public_storage_arn" {
  description = "ARN of the public storage S3 bucket (for EC2 + Lambda IAM policies)"
  type        = string
}

variable "web_prod_bucket_regional_domain_name" {
  description = "Regional domain name of the prod frontend bucket (CloudFront origin)"
  type        = string
}
