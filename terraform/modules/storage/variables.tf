variable "environment" {
  description = "Environment name (e.g. prod, staging)"
  type        = string
}

variable "name_suffix" {
  description = "Suffix appended to resource names (\"\" for prod, \"-staging\" for staging)"
  type        = string
}

variable "image_resizing_lambda_arn" {
  description = "ARN of the image-resizing Lambda invoked on private bucket ObjectCreated events"
  type        = string
}

variable "image_resizing_lambda_permission_id" {
  description = "Dependency handle for the Lambda resource-based permission that allows S3 to invoke the function"
  type        = string
}

variable "cloudfront_domain_name" {
  description = "CloudFront distribution domain name used in CORS / referer rules"
  type        = string
}

variable "cloudfront_distribution_arn" {
  description = "CloudFront distribution ARN granted read access to the web bucket"
  type        = string
}
