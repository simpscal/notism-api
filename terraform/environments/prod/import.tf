# ------------------------------------------------------------------------------
# Config-driven imports (Terraform >= 1.5) for the account-shared github-oidc
# resources that already exist in AWS. These reconcile the live state into
# Terraform with no create.
#
# Terraform 1.5 import block ids must be literal strings (no variable or
# data-source interpolation), so the account id is inlined here. This is only an
# import locator for an existing resource, not infrastructure config — the
# module's policy/trust ARNs derive the account from data.aws_caller_identity.
#
# If the moved {} blocks in moved.tf already relocated these from the flat root
# state, these import blocks no-op. Authored as code; NOT run by this refactor.
# ------------------------------------------------------------------------------

import {
  to = module.github_oidc.aws_iam_openid_connect_provider.github_actions
  id = "arn:aws:iam::249550149516:oidc-provider/token.actions.githubusercontent.com"
}

import {
  to = module.github_oidc.aws_iam_role.api_deploy
  id = "notism-api-deploy-role"
}

import {
  to = module.github_oidc.aws_iam_role.web_deploy
  id = "notism-web-deploy"
}

import {
  to = module.github_oidc.aws_iam_role_policy_attachment.api_deploy_cloudfront
  id = "notism-api-deploy-role/arn:aws:iam::aws:policy/CloudFrontFullAccess"
}

import {
  to = module.github_oidc.aws_iam_role_policy_attachment.api_deploy_ecr
  id = "notism-api-deploy-role/arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser"
}

import {
  to = module.github_oidc.aws_iam_role_policy_attachment.web_deploy_cloudfront
  id = "notism-web-deploy/arn:aws:iam::aws:policy/CloudFrontFullAccess"
}

import {
  to = module.github_oidc.aws_iam_role_policy_attachment.web_deploy_s3
  id = "notism-web-deploy/arn:aws:iam::aws:policy/AmazonS3FullAccess"
}
