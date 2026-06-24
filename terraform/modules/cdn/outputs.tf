output "cloudfront_web_prod_domain_name" {
  description = "CloudFront domain name for prod frontend"
  value       = aws_cloudfront_distribution.web_prod.domain_name
}

output "cloudfront_web_prod_arn" {
  description = "CloudFront distribution ARN for prod frontend"
  value       = aws_cloudfront_distribution.web_prod.arn
}

output "cloudfront_web_prod_distribution_id" {
  description = "CloudFront distribution ID for prod frontend"
  value       = aws_cloudfront_distribution.web_prod.id
}
