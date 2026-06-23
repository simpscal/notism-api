# ------------------------------------------------------------------------------
# private_storage
# ------------------------------------------------------------------------------

output "private_storage_id" {
  description = "ID (bucket name) of the private storage bucket"
  value       = aws_s3_bucket.private_storage.id
}

output "private_storage_arn" {
  description = "ARN of the private storage bucket"
  value       = aws_s3_bucket.private_storage.arn
}

output "private_storage_bucket_regional_domain_name" {
  description = "Regional domain name of the private storage bucket"
  value       = aws_s3_bucket.private_storage.bucket_regional_domain_name
}

# ------------------------------------------------------------------------------
# public_storage
# ------------------------------------------------------------------------------

output "public_storage_id" {
  description = "ID (bucket name) of the public storage bucket"
  value       = aws_s3_bucket.public_storage.id
}

output "public_storage_arn" {
  description = "ARN of the public storage bucket"
  value       = aws_s3_bucket.public_storage.arn
}

output "public_storage_bucket_regional_domain_name" {
  description = "Regional domain name of the public storage bucket"
  value       = aws_s3_bucket.public_storage.bucket_regional_domain_name
}

# ------------------------------------------------------------------------------
# web_prod
# ------------------------------------------------------------------------------

output "web_prod_id" {
  description = "ID (bucket name) of the prod frontend bucket"
  value       = aws_s3_bucket.web_prod.id
}

output "web_prod_arn" {
  description = "ARN of the prod frontend bucket"
  value       = aws_s3_bucket.web_prod.arn
}

output "web_prod_bucket_regional_domain_name" {
  description = "Regional domain name of the prod frontend bucket"
  value       = aws_s3_bucket.web_prod.bucket_regional_domain_name
}
