output "private_storage_bucket" {
  description = "Name of the private storage S3 bucket"
  value       = aws_s3_bucket.private_storage.bucket
}

output "public_storage_bucket" {
  description = "Name of the public storage S3 bucket"
  value       = aws_s3_bucket.public_storage.bucket
}

output "web_bucket" {
  description = "Name of the web frontend S3 bucket"
  value       = aws_s3_bucket.web_prod.bucket
}

output "private_storage_arn" {
  description = "ARN of the private storage S3 bucket"
  value       = aws_s3_bucket.private_storage.arn
}

output "public_storage_arn" {
  description = "ARN of the public storage S3 bucket"
  value       = aws_s3_bucket.public_storage.arn
}

output "web_bucket_arn" {
  description = "ARN of the web frontend S3 bucket"
  value       = aws_s3_bucket.web_prod.arn
}

output "web_bucket_id" {
  description = "ID of the web frontend S3 bucket"
  value       = aws_s3_bucket.web_prod.id
}

output "web_bucket_regional_domain_name" {
  description = "Regional domain name of the web frontend S3 bucket (CloudFront origin)"
  value       = aws_s3_bucket.web_prod.bucket_regional_domain_name
}
