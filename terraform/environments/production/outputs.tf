# ------------------------------------------------------------
# VPC
# ------------------------------------------------------------

output "vpc_id" {
  description = "ID of the notism VPC"
  value       = module.vpc.vpc_id
}

# ------------------------------------------------------------
# EC2
# ------------------------------------------------------------

output "ec2_instance_id" {
  description = "ID of the notism-api EC2 instance"
  value       = module.compute.instance_id
}

output "ec2_public_ip" {
  description = "Public IP of the API (Elastic IP)."
  value       = module.compute.eip_public_ip
}

# ------------------------------------------------------------
# ECR
# ------------------------------------------------------------

output "ecr_repository_url" {
  description = "ECR repository URL for notism-api image"
  value       = module.compute.ecr_repository_url
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
  description = "API base URL (HTTP)."
  value       = "http://${module.compute.eip_public_ip}:5000"
}

# ------------------------------------------------------------
# S3
# ------------------------------------------------------------

output "s3_private_storage_arn" {
  description = "ARN of the private storage S3 bucket"
  value       = module.storage.private_storage_arn
}

output "s3_public_storage_arn" {
  description = "ARN of the public storage S3 bucket"
  value       = module.storage.public_storage_arn
}

output "s3_web_prod_arn" {
  description = "ARN of the prod frontend S3 bucket"
  value       = module.storage.web_prod_arn
}

# ------------------------------------------------------------
# CloudFront
# ------------------------------------------------------------

output "cloudfront_web_prod_domain_name" {
  description = "CloudFront domain name for prod frontend"
  value       = module.cdn.cloudfront_web_prod_domain_name
}

output "cloudfront_web_prod_distribution_id" {
  description = "CloudFront distribution ID for prod frontend"
  value       = module.cdn.cloudfront_web_prod_distribution_id
}
