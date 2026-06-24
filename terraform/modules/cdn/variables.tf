variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-northeast-1"
}

variable "web_prod_bucket_regional_domain_name" {
  description = "Regional domain name of the prod frontend bucket (CloudFront origin)"
  type        = string
}
