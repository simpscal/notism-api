output "vpc_id" {
  description = "ID of the staging VPC"
  value       = module.network.vpc_id
}

output "ec2_instance_id" {
  description = "ID of the staging EC2 instance"
  value       = module.compute.instance_id
}

output "ec2_public_ip" {
  description = "Public IP of the staging API (Elastic IP)."
  value       = module.compute.public_ip
}

output "ecr_repository_url" {
  description = "ECR repository URL for the staging notism-api image"
  value       = module.registry.repository_url
}

output "api_url" {
  description = "Staging API base URL (HTTP)."
  value       = "http://${module.compute.public_ip}:5000"
}

output "s3_private_storage_arn" {
  description = "ARN of the staging private storage S3 bucket"
  value       = module.storage.private_storage_arn
}

output "s3_public_storage_arn" {
  description = "ARN of the staging public storage S3 bucket"
  value       = module.storage.public_storage_arn
}

output "s3_web_arn" {
  description = "ARN of the staging frontend S3 bucket"
  value       = module.storage.web_bucket_arn
}

output "cloudfront_web_domain_name" {
  description = "CloudFront domain name for the staging frontend"
  value       = module.cdn.domain_name
}

output "cloudfront_web_distribution_id" {
  description = "CloudFront distribution ID for the staging frontend"
  value       = module.cdn.distribution_id
}
