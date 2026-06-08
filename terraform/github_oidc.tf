# ------------------------------------------------------------------------------
# GitHub Actions OIDC Provider
#
# Shared by the notism-api and notism-web deploy roles so GitHub Actions can
# assume those roles via web identity federation (no long-lived AWS keys).
# Created originally by hand in the console; imported into Terraform below.
# ------------------------------------------------------------------------------

resource "aws_iam_openid_connect_provider" "github_actions" {
  url             = "https://token.actions.githubusercontent.com"
  client_id_list  = ["sts.amazonaws.com"]
  thumbprint_list = ["2b18947a6a9fc7764fd8b5fb18a863b0c6dac24f"]

  tags = {
    Name = "github-actions-oidc"
  }
}

# ------------------------------------------------------------------------------
# notism-api deploy role
#
# Assumed by GitHub Actions for the simpscal/notism-api repo to deploy the API
# (ECS/ECR/CloudFront) and — via the inline policy below — start the API EC2
# instance for the deploy auto-start step.
# ------------------------------------------------------------------------------

resource "aws_iam_role" "api_deploy" {
  name        = "notism-api-deploy-role"
  description = "GitHub Actions OIDC role for Notism API/Worker deploy (ECR, EC2, ECS, CloudFront)"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Federated = aws_iam_openid_connect_provider.github_actions.arn
        }
        Action = "sts:AssumeRoleWithWebIdentity"
        Condition = {
          StringEquals = {
            "token.actions.githubusercontent.com:aud" = "sts.amazonaws.com"
          }
          StringLike = {
            "token.actions.githubusercontent.com:sub" = "repo:simpscal/notism-api:*"
          }
        }
      }
    ]
  })

  tags = {
    Name = "notism-api-deploy-role"
  }
}

resource "aws_iam_role_policy_attachment" "api_deploy_cloudfront" {
  role       = aws_iam_role.api_deploy.name
  policy_arn = "arn:aws:iam::aws:policy/CloudFrontFullAccess"
}

resource "aws_iam_role_policy_attachment" "api_deploy_ecr" {
  role       = aws_iam_role.api_deploy.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser"
}

resource "aws_iam_role_policy_attachment" "api_deploy_ecs" {
  role       = aws_iam_role.api_deploy.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonECS_FullAccess"
}

# NEW (the only intended apply-time change): grant the deploy role permission to
# start the API EC2 instance for the deploy auto-start step.
#   - ec2:StartInstances is scoped to the api instance ARN.
#   - ec2:DescribeInstances / ec2:DescribeInstanceStatus do NOT support
#     resource-level scoping and must use "*".
resource "aws_iam_role_policy" "api_deploy_ec2_start" {
  name = "notism-api-deploy-ec2-start"
  role = aws_iam_role.api_deploy.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect   = "Allow"
        Action   = "ec2:StartInstances"
        Resource = "arn:aws:ec2:${var.aws_region}:${data.aws_caller_identity.current.account_id}:instance/${aws_instance.api.id}"
      },
      {
        Effect = "Allow"
        Action = [
          "ec2:DescribeInstances",
          "ec2:DescribeInstanceStatus",
        ]
        Resource = "*"
      }
    ]
  })
}

# ------------------------------------------------------------------------------
# notism-web deploy role
#
# Assumed by GitHub Actions for the simpscal/notism-web repo to deploy the
# frontend (S3 + CloudFront).
# ------------------------------------------------------------------------------

resource "aws_iam_role" "web_deploy" {
  name        = "notism-web-deploy"
  description = "Web deployment"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Federated = aws_iam_openid_connect_provider.github_actions.arn
        }
        Action = "sts:AssumeRoleWithWebIdentity"
        Condition = {
          StringEquals = {
            "token.actions.githubusercontent.com:aud" = "sts.amazonaws.com"
          }
          StringLike = {
            "token.actions.githubusercontent.com:sub" = "repo:simpscal/notism-web:*"
          }
        }
      }
    ]
  })

  tags = {
    Name = "notism-web-deploy"
  }
}

resource "aws_iam_role_policy_attachment" "web_deploy_cloudfront" {
  role       = aws_iam_role.web_deploy.name
  policy_arn = "arn:aws:iam::aws:policy/CloudFrontFullAccess"
}

resource "aws_iam_role_policy_attachment" "web_deploy_s3" {
  role       = aws_iam_role.web_deploy.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonS3FullAccess"
}

# ------------------------------------------------------------------------------
# Config-driven imports (Terraform >= 1.5) for the resources that already exist
# in the AWS account. These reconcile the live state into Terraform with no
# create. The new inline policy (api_deploy_ec2_start) has NO import block — it
# is the one intended create.
# ------------------------------------------------------------------------------

# NOTE: Terraform 1.5 import block ids must be literal strings (no variable or
# data-source interpolation), so the account id is inlined here. This is only an
# import locator for an existing resource, not infrastructure config — the
# policy/trust ARNs above still derive the account from aws_caller_identity.
import {
  to = aws_iam_openid_connect_provider.github_actions
  id = "arn:aws:iam::249550149516:oidc-provider/token.actions.githubusercontent.com"
}

import {
  to = aws_iam_role.api_deploy
  id = "notism-api-deploy-role"
}

import {
  to = aws_iam_role.web_deploy
  id = "notism-web-deploy"
}

import {
  to = aws_iam_role_policy_attachment.api_deploy_cloudfront
  id = "notism-api-deploy-role/arn:aws:iam::aws:policy/CloudFrontFullAccess"
}

import {
  to = aws_iam_role_policy_attachment.api_deploy_ecr
  id = "notism-api-deploy-role/arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser"
}

import {
  to = aws_iam_role_policy_attachment.api_deploy_ecs
  id = "notism-api-deploy-role/arn:aws:iam::aws:policy/AmazonECS_FullAccess"
}

import {
  to = aws_iam_role_policy_attachment.web_deploy_cloudfront
  id = "notism-web-deploy/arn:aws:iam::aws:policy/CloudFrontFullAccess"
}

import {
  to = aws_iam_role_policy_attachment.web_deploy_s3
  id = "notism-web-deploy/arn:aws:iam::aws:policy/AmazonS3FullAccess"
}
