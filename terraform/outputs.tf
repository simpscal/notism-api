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
  description = "Public IP of the API (Elastic IP)"
  value       = aws_eip.api.public_ip
}

# ------------------------------------------------------------
# RDS (only when use_rds = true)
# ------------------------------------------------------------

output "rds_endpoint" {
  description = "RDS PostgreSQL endpoint"
  value       = var.use_rds ? aws_db_instance.main[0].endpoint : null
}

output "rds_address" {
  description = "RDS hostname (for connection string)"
  value       = var.use_rds ? aws_db_instance.main[0].address : null
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
  description = "Hint for building the PostgreSQL connection string (password is sensitive)"
  value       = var.use_rds ? "Host=${aws_db_instance.main[0].address};Database=notism_db;Username=notismadmin;Password=<from var.db_password>;Port=5432" : "Host=notism-db;Database=notism_db;Username=notismadmin;Password=<from DB_PASSWORD secret>;Port=5432"
  sensitive   = true
}

# ------------------------------------------------------------
# API
# ------------------------------------------------------------

output "api_url" {
  description = "API base URL (HTTP)"
  value       = "http://${aws_eip.api.public_ip}:5000"
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
