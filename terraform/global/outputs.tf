output "github_actions_oidc_provider_arn" {
  description = "ARN of the shared GitHub Actions OIDC provider"
  value       = aws_iam_openid_connect_provider.github_actions.arn
}

output "api_deploy_role_arn" {
  description = "ARN of the notism-api GitHub Actions deploy role"
  value       = aws_iam_role.api_deploy.arn
}

output "web_deploy_role_arn" {
  description = "ARN of the notism-web GitHub Actions deploy role"
  value       = aws_iam_role.web_deploy.arn
}
