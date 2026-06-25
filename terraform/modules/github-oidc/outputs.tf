output "oidc_provider_arn" {
  description = "ARN of the GitHub Actions OIDC provider"
  value       = aws_iam_openid_connect_provider.github_actions.arn
}

output "api_deploy_role_arn" {
  description = "ARN of the notism-api deploy role"
  value       = aws_iam_role.api_deploy.arn
}

output "web_deploy_role_arn" {
  description = "ARN of the notism-web deploy role"
  value       = aws_iam_role.web_deploy.arn
}
