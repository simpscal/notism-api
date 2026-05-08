# ------------------------------------------------------------
# VPC
# ------------------------------------------------------------

output "vpc_id" {
  description = "ID of the notism VPC"
  value       = aws_vpc.main.id
}

# ------------------------------------------------------------
# EC2
# ------------------------------------------------------------

output "ec2_instance_id" {
  description = "ID of the notism-api EC2 instance"
  value       = aws_instance.api.id
}

output "ec2_public_ip" {
  description = "Public IP of the API (Elastic IP). 'EIP_RELEASED' when not allocated."
  value       = "EIP_RELEASED"
}

# ------------------------------------------------------------
# ECR
# ------------------------------------------------------------

output "ecr_repository_url" {
  description = "ECR repository URL for notism-api image"
  value       = aws_ecr_repository.api.repository_url
}

# ------------------------------------------------------------
# PostgreSQL
# ------------------------------------------------------------

output "connection_string_hint" {
  description = "Hint for building the PostgreSQL connection string (Supabase — set via CONNECTION_STRING GitHub Secret)"
  value       = "Host=<supabase-host>;Database=postgres;Username=postgres;Password=<from CONNECTION_STRING secret>;Port=5432;SSL Mode=Require;Trust Server Certificate=true"
  sensitive   = true
}

# ------------------------------------------------------------
# API
# ------------------------------------------------------------

output "api_url" {
  description = "API base URL (HTTP). 'EIP_RELEASED' when EIP not allocated."
  value       = "http://EIP_RELEASED:5000"
}

# ------------------------------------------------------------
# S3
# ------------------------------------------------------------

output "s3_private_storage_arn" {
  description = "ARN of the private storage S3 bucket"
  value       = aws_s3_bucket.private_storage.arn
}

output "s3_public_storage_arn" {
  description = "ARN of the public storage S3 bucket"
  value       = aws_s3_bucket.public_storage.arn
}

output "s3_web_arn" {
  description = "ARN of the dev frontend S3 bucket"
  value       = aws_s3_bucket.web.arn
}

output "s3_web_prod_arn" {
  description = "ARN of the prod frontend S3 bucket"
  value       = aws_s3_bucket.web_prod.arn
}

# ------------------------------------------------------------
# CloudFront
# ------------------------------------------------------------

output "cloudfront_web_domain_name" {
  description = "CloudFront domain name for dev frontend"
  value       = aws_cloudfront_distribution.web.domain_name
}

output "cloudfront_web_distribution_id" {
  description = "CloudFront distribution ID for dev frontend"
  value       = aws_cloudfront_distribution.web.id
}

output "cloudfront_web_prod_domain_name" {
  description = "CloudFront domain name for prod frontend"
  value       = aws_cloudfront_distribution.web_prod.domain_name
}

output "cloudfront_web_prod_distribution_id" {
  description = "CloudFront distribution ID for prod frontend"
  value       = aws_cloudfront_distribution.web_prod.id
}
