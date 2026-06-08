# CI/CD Deploy Flow with GitHub Actions OIDC

## Overview

This document describes how the API is deployed to AWS EC2 from GitHub Actions, and how the pipeline authenticates to AWS using **OpenID Connect (OIDC)** — no long-lived access keys. It also covers the self-healing **auto-start** step that boots a stopped EC2 instance before deploying, and the IAM resources that make both possible. All AWS IAM resources described here are managed in Terraform under `terraform/github_oidc.tf`.

## Architecture

```
GitHub Actions (push to main)
  → request OIDC token (token.actions.githubusercontent.com)
  → assume notism-api-deploy-role via AssumeRoleWithWebIdentity
  → ensure EC2 running (start-instances + wait instance-running)
  → build & push image to ECR
  → SSH to EC2 (Elastic IP) → pull image → restart container
```

## Components

### 1. GitHub Actions OIDC Provider

A single account-level OIDC provider lets GitHub Actions workflows assume IAM roles without stored credentials.

- **URL**: `token.actions.githubusercontent.com`
- **Audience** (`client_id_list`): `sts.amazonaws.com`
- **Terraform**: `aws_iam_openid_connect_provider.github_actions` in `terraform/github_oidc.tf`
- **Shared by**: both the API and web deploy roles (below).

### 2. Deploy Roles

Each repo assumes its own role, scoped by the OIDC token's `sub` claim so only that repo's workflows can assume it.

| Role | Assumed by | `sub` condition | Managed policies |
|------|-----------|-----------------|------------------|
| **notism-api-deploy-role** | `simpscal/notism-api` | `repo:simpscal/notism-api:*` | `CloudFrontFullAccess`, `AmazonEC2ContainerRegistryPowerUser`, `AmazonECS_FullAccess` |
| **notism-web-deploy** | `simpscal/notism-web` | `repo:simpscal/notism-web:*` | `CloudFrontFullAccess`, `AmazonS3FullAccess` |

Both roles trust the shared OIDC provider via `sts:AssumeRoleWithWebIdentity` with `aud = sts.amazonaws.com`.

### 3. EC2 Auto-Start Permission

The `deploy-ec2` job starts the API instance if it is stopped, so a stopped instance never blocks CI indefinitely. This requires an inline policy on **notism-api-deploy-role**:

- **Terraform**: `aws_iam_role_policy.api_deploy_ec2_start` (`notism-api-deploy-ec2-start`)
- **`ec2:StartInstances`** — scoped to the API instance ARN (`aws_instance.api`).
- **`ec2:DescribeInstances`, `ec2:DescribeInstanceStatus`** — on `Resource = "*"`. These describe actions **do not support resource-level scoping**, so they must use `*`.

### 4. Workflow Steps (`.github/workflows/deploy.yml`)

1. **Ensure EC2 is running** — `aws ec2 start-instances` (no-op if already running) then `aws ec2 wait instance-running`. Uses the `EC2_INSTANCE_ID` secret.
2. **Configure AWS credentials** — `aws-actions/configure-aws-credentials@v5` with `role-to-assume: ${{ secrets.AWS_ROLE_TO_ASSUME }}` (the API deploy role ARN).
3. **Build & push image** — composite action `.github/actions/build-push-ecr/action.yml` pushes to ECR.
4. **Deploy** — SSH to the EC2 Elastic IP (`EC2_HOST`), pull the new image, restart the container.

> **Node.js 24 runtime:** `deploy.yml` and the `build-push-ecr` composite action set `FORCE_JAVASCRIPT_ACTIONS_TO_NODE24: true` to opt JavaScript actions onto the Node.js 24 runtime ahead of the GitHub Node.js 20 deprecation. Composite actions do not inherit the top-level `env`, so the flag is set on the composite steps directly.

## Required GitHub Actions Secrets

| Secret | Value / purpose |
|--------|-----------------|
| `AWS_ROLE_TO_ASSUME` | ARN of `notism-api-deploy-role` |
| `EC2_INSTANCE_ID` | `i-0637aed64215bf80f` (auto-start target) |
| `EC2_HOST` | Elastic IP `3.115.163.17` (stable, Terraform-managed) |
| `EC2_USER`, `EC2_SSH_PRIVATE_KEY` | SSH access to the instance |
| `ECR_REPOSITORY` | Target ECR repository |

## Terraform: Managing the IAM / OIDC Resources

The OIDC provider and both deploy roles were originally created by hand in the AWS console. They are now defined in `terraform/github_oidc.tf` and adopted into state via **config-driven `import` blocks** (Terraform ≥ 1.5).

### Applying changes

State is **local** (`terraform/terraform.tfstate`, gitignored — it holds sensitive outputs), so apply from a workstation with AWS credentials:

```bash
cd terraform
terraform plan    # review: imports reconcile existing resources, only intended changes shown
terraform apply
```

On first apply the import blocks reconcile the existing provider, roles, and managed-policy attachments into state (no recreate); subsequent `terraform plan` runs report **no changes**.

### Notes

- **Import-block ids must be literal strings** (no variable/data interpolation). The OIDC provider import id therefore inlines the account id; runtime policy/trust ARNs still derive the account from `aws_caller_identity`.
- Managed-policy attachments are replicated **exactly** as they exist in AWS — changing them alters live deploy permissions.
- The `import` blocks are one-time adoption helpers; they are harmless to keep but may be removed once the resources are confirmed in state.
