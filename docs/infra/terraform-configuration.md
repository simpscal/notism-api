# Terraform configuration for Notism AWS infrastructure

This document describes the Terraform setup used to provision the Notism API infrastructure on AWS. It matches the architecture in [aws-architecture.md](aws-architecture.md) and uses the same **notism** resource naming as the CLI-based deployment.

---

## Overview

The Terraform code lives in **`terraform/`** and creates:

- **Networking**: VPC, Internet Gateway, one public subnet (with IPv6), route tables
- **Security**: EC2 security group
- **Compute**: EC2 instance (notism-api) with Elastic IP, Docker + Compose plugin
- **IAM**: EC2 role and instance profile (S3 + ECR)
- **Container registry**: ECR repository (notism-api)
- **Storage**: 4 S3 buckets (private-notism-storage, public-notism-storage, notism-web, notism-web-prod)
- **CDN**: 2 CloudFront distributions with Origin Access Controls for frontend hosting

PostgreSQL is hosted on Supabase (external to Terraform). No RDS resources are provisioned.

All resources are tagged with `Project = notism` and `ManagedBy = terraform` via the provider default tags.

---

## File structure

| File | Purpose |
|------|---------|
| `versions.tf` | Terraform and provider version constraints (AWS ~> 5.0) |
| `provider.tf` | AWS provider configuration and default tags |
| `variables.tf` | Input variables (region, CIDRs, key_name, environment) |
| `vpc.tf` | VPC, IGW, public subnet (IPv4 + IPv6), route table and associations |
| `security_groups.tf` | notism-ec2-sg |
| `iam.tf` | NotismEC2Role, NotismEC2Profile, S3 and ECR policy attachments |
| `ec2.tf` | AMI data source, user data, EC2 instance (notism-api) |
| `eip.tf` | Elastic IP (notism-api-eip) and association to EC2 |
| `ecr.tf` | ECR repository for the API image |
| `s3.tf` | S3 buckets (storage + frontend hosting), encryption, CORS, policies, public access blocks |
| `cloudfront.tf` | CloudFront distributions, Origin Access Controls, managed cache policy data source |
| `outputs.tf` | Output values (VPC ID, EC2 ID, ECR URL, API URL, S3 ARNs, CloudFront domains, connection string hint) |
| `example.tfvars` | Example variable file (no secrets) |

---

## Variables

| Variable | Type | Default | Required | Description |
|----------|------|---------|----------|-------------|
| `aws_region` | `string` | `"us-east-1"` | No | AWS region for all resources. |
| `vpc_cidr` | `string` | `"10.0.0.0/16"` | No | CIDR block for the VPC. |
| `public_subnet_cidr` | `string` | `"10.0.1.0/24"` | No | CIDR for the public subnet (EC2). |
| `key_name` | `string` | `null` | No | Name of an existing EC2 key pair for SSH. |
| `environment` | `string` | `"prod"` | No | Environment tag applied via default_tags. |

### Connecting a key pair to the EC2 instance

The key pair is set **only at instance launch**. You cannot attach or change it on an existing instance.

- **New instance:** Create a key pair in AWS (EC2 → Key pairs → Create key pair; name e.g. `notism-deploy`, RSA, PEM). Set `key_name = "notism-deploy"` in your tfvars, then run `terraform apply`. The new instance will have that key in `~/.ssh/authorized_keys`.
- **Existing instance launched without a key:** You must recreate the instance:
  ```bash
  cd terraform
  terraform taint aws_instance.api
  terraform apply -var-file=terraform.tfvars
  ```
  Then put the private key (the `.pem` you downloaded) into GitHub secret `EC2_SSH_PRIVATE_KEY`.

### Example variable file

Create `terraform/terraform.tfvars` (do not commit):

```hcl
aws_region  = "us-east-1"
environment = "prod"
key_name    = "notism-api"
ami_id      = "ami-01a29f4fad8f1cb0e"
```

---

## Resources and naming

| Terraform resource | AWS name / identifier | Notes |
|--------------------|------------------------|-------|
| `aws_vpc.main` | notism-vpc | Single VPC for the stack. IPv6 CIDR assigned. |
| `aws_internet_gateway.main` | notism-igw | Attached to VPC. |
| `aws_subnet.public` | notism-public-subnet | Public subnet (first AZ). EC2 lives here. IPv4 + IPv6. |
| `aws_route_table.public` | notism-public-rt | Routes `0.0.0.0/0` and `::/0` → IGW. |
| `aws_security_group.ec2` | notism-ec2-sg | Inbound: 22, 80, 443. Outbound: all. |
| `aws_iam_role.ec2` | NotismEC2Role | Trust: ec2.amazonaws.com. |
| `aws_iam_instance_profile.ec2` | NotismEC2Profile | Attached to EC2. |
| `aws_instance.api` | notism-api (Name tag) | t4g.micro, Amazon Linux 2023 ARM. |
| `aws_eip.api` | notism-api-eip (Name tag) | Elastic IP for the API. |
| `aws_ecr_repository.api` | notism-api | ECR repository for the API image. |
| `aws_s3_bucket.private_storage` | private-notism-storage | Private file storage for the API. |
| `aws_s3_bucket.public_storage` | public-notism-storage | Public file storage (public-read). |
| `aws_s3_bucket.web` | notism-web | Dev frontend static hosting. |
| `aws_s3_bucket.web_prod` | notism-web-prod | Prod frontend static hosting. |
| `aws_cloudfront_distribution.web` | E1GJTPGEDUM3ZO | Dev frontend CDN (origin: notism-web). |
| `aws_cloudfront_distribution.web_prod` | E3B3TRUPIAA1TW | Prod frontend CDN (origin: notism-web-prod). |
| `aws_cloudfront_origin_access_control.web` | oac-notism-web | OAC for dev CloudFront → S3. |
| `aws_cloudfront_origin_access_control.web_prod` | oac-notism-web-prod | OAC for prod CloudFront → S3. |

---

## Outputs

After `terraform apply`, these outputs are available (e.g. `terraform output ec2_instance_id`):

| Output | Description |
|--------|-------------|
| `vpc_id` | ID of the notism VPC. |
| `ec2_instance_id` | EC2 instance ID (notism-api). |
| `ec2_public_ip` | Public IP of the API (Elastic IP). Use for GitHub secret `EC2_HOST`. |
| `ecr_repository_url` | ECR repository URL for the notism-api image. |
| `connection_string_hint` | (sensitive) Template for the Supabase connection string. |
| `api_url` | HTTP URL for the API. |
| `s3_private_storage_arn` | ARN of the private storage S3 bucket. |
| `s3_public_storage_arn` | ARN of the public storage S3 bucket. |
| `s3_web_arn` | ARN of the dev frontend S3 bucket. |
| `s3_web_prod_arn` | ARN of the prod frontend S3 bucket. |
| `cloudfront_web_domain_name` | CloudFront domain name for dev frontend. |
| `cloudfront_web_distribution_id` | CloudFront distribution ID for dev frontend. |
| `cloudfront_web_prod_domain_name` | CloudFront domain name for prod frontend. |
| `cloudfront_web_prod_distribution_id` | CloudFront distribution ID for prod frontend. |

---

## Usage

### Prerequisites

- [Terraform](https://www.terraform.io/downloads) >= 1.0
- AWS credentials (e.g. `aws configure` or environment variables)

### Commands

```bash
cd terraform
terraform init
terraform plan -var="key_name=notism-api"
terraform apply -var="key_name=notism-api"
```

With a variable file:

```bash
terraform apply -var-file=terraform.tfvars
```

To destroy:

```bash
terraform destroy -var="key_name=notism-api"
```

### State

- State is stored locally by default (`terraform.tfstate` in `terraform/`).
- Do not commit state files — they may contain sensitive metadata.
- For team or CI use, configure a [remote backend](https://www.terraform.io/docs/language/settings/backends/index.html) (e.g. S3 + DynamoDB).

---

## Security and sensitive data

- **State**: May contain IDs and metadata; avoid committing `*.tfstate` and `*.tfstate.*`. Use `.gitignore` (already in `terraform/.gitignore`).
- **tfvars**: Keep `terraform.tfvars` out of version control; use `example.tfvars` as a template only.
- **Database credentials**: Not managed by Terraform. The Supabase connection string is stored exclusively in the `CONNECTION_STRING` GitHub secret.

---

## Related docs

- [aws-architecture.md](aws-architecture.md) — Architecture diagram and components.
