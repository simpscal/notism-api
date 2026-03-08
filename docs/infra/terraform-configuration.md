# Terraform configuration for Notism AWS infrastructure

This document describes the Terraform setup used to provision the Notism API infrastructure on AWS. It matches the architecture in [aws-architecture.md](aws-architecture.md) and uses the same **notism** resource naming as the CLI-based deployment.

---

## Overview

The Terraform code lives in **`terraform/`** and creates:

- **Networking**: VPC, Internet Gateway, one public subnet, two private subnets (2 AZs), route tables
- **Security**: EC2 and RDS security groups
- **Database**: RDS PostgreSQL (notism-db)
- **Compute**: EC2 instance (notism-api) with Elastic IP
- **IAM**: EC2 role and instance profile (S3 + ECR)
- **Container registry**: ECR repository (notism-api)

All resources are tagged with `Project = notism` and `ManagedBy = terraform` via the provider default tags.

---

## File structure

| File | Purpose |
|------|---------|
| `versions.tf` | Terraform and provider version constraints (AWS ~> 5.0) |
| `provider.tf` | AWS provider configuration and default tags |
| `variables.tf` | Input variables (region, CIDRs, db_password, key_name, environment) |
| `vpc.tf` | VPC, IGW, subnets, route tables and associations |
| `security_groups.tf` | notism-ec2-sg and notism-rds-sg |
| `rds.tf` | DB subnet group and RDS PostgreSQL instance |
| `iam.tf` | NotismEC2Role, NotismEC2Profile, S3 and ECR policy attachments |
| `ec2.tf` | AMI data source, user data, EC2 instance (notism-api) |
| `eip.tf` | Elastic IP (notism-api-eip) and association to EC2 |
| `ecr.tf` | ECR repository for the API image |
| `outputs.tf` | Output values (VPC ID, EC2 IP, RDS endpoint, ECR URL, API URL, etc.) |
| `example.tfvars` | Example variable file (no secrets) |
| `README.md` | Quick start and usage |

---

## Variables

### Required

| Variable | Type | Description |
|----------|------|-------------|
| `db_password` | `string` (sensitive) | Master password for RDS PostgreSQL. Provide via `-var`, `-var-file`, or `TF_VAR_db_password`. Never commit. |

### Optional (with defaults)

| Variable | Type | Default | Description |
|----------|------|---------|-------------|
| `aws_region` | `string` | `"us-east-1"` | AWS region for all resources. |
| `vpc_cidr` | `string` | `"10.0.0.0/16"` | CIDR block for the VPC. |
| `public_subnet_cidr` | `string` | `"10.0.1.0/24"` | CIDR for the public subnet (EC2). |
| `private_subnet_cidrs` | `list(string)` | `["10.0.2.0/24", "10.0.3.0/24"]` | CIDRs for private subnets (RDS). Must have at least 2 for RDS. |
| `key_name` | `string` | `null` | Name of an existing EC2 key pair for SSH. Omit if you do not need SSH (e.g. use Session Manager). |
| `environment` | `string` | `"prod"` | Environment tag applied via default_tags. |

### Connecting a key pair to the EC2 instance

The key pair is set **only at instance launch**. You cannot attach or change it on an existing instance.

- **New instance:** Create a key pair in AWS (EC2 → Key pairs → Create key pair; name e.g. `notism-deploy`, RSA, PEM). Set `key_name = "notism-deploy"` in your tfvars, then run `terraform apply`. The new instance will have that key in `~/.ssh/authorized_keys`.
- **Existing instance that was launched without a key:** You must recreate the instance so Terraform launches it with `key_name` set. After creating the key pair and adding `key_name` to tfvars, run:
  ```bash
  cd terraform
  terraform taint aws_instance.api
  terraform apply -var-file=terraform.tfvars
  ```
  Then put the **private** key (the `.pem` you downloaded) into GitHub secret `EC2_SSH_PRIVATE_KEY`.

### Example variable file

Create `terraform/terraform.tfvars` (do not commit if it contains secrets):

```hcl
aws_region  = "us-east-1"
environment = "prod"
db_password = "your-secure-password"
key_name    = "notism-deploy"   # name of key pair created in EC2 (same region)
```

Or use the example file:

```bash
cp terraform/example.tfvars terraform/terraform.tfvars
# Edit and set db_password, then:
terraform apply -var-file=terraform.tfvars
```

---

## Resources and naming

| Terraform resource | AWS name / identifier | Notes |
|--------------------|------------------------|-------|
| `aws_vpc.main` | notism-vpc | Single VPC for the stack. |
| `aws_internet_gateway.main` | notism-igw | Attached to VPC. |
| `aws_subnet.public` | notism-public-subnet | One per AZ (first AZ). EC2 lives here. |
| `aws_subnet.private[0]` | notism-private-subnet-a | First private subnet (RDS). |
| `aws_subnet.private[1]` | notism-private-subnet-b | Second private subnet (RDS, 2nd AZ). |
| `aws_route_table.public` | notism-public-rt | Default route 0.0.0.0/0 → IGW. |
| `aws_route_table.private` | notism-private-rt | No internet route. |
| `aws_security_group.ec2` | notism-ec2-sg | Ports 22, 80, 443, 5000 from 0.0.0.0/0. |
| `aws_security_group.rds` | notism-rds-sg | Port 5432 from notism-ec2-sg only. |
| `aws_db_subnet_group.main` | notism-db-subnet | Both private subnets. |
| `aws_db_instance.main` | notism-db | PostgreSQL 16.13, db.t4g.micro, notism_db. |
| `aws_iam_role.ec2` | NotismEC2Role | Trust: ec2.amazonaws.com. |
| `aws_iam_instance_profile.ec2` | NotismEC2Profile | Attached to EC2. |
| `aws_instance.api` | notism-api (Name tag) | t4g.micro, Amazon Linux 2023 ARM. |
| `aws_eip.api` | notism-api-eip (Name tag) | Elastic IP for the API. |
| `aws_ecr_repository.api` | notism-api | ECR repository for the API image. |

---

## Outputs

After `terraform apply`, these outputs are available (e.g. `terraform output ec2_public_ip`):

| Output | Description |
|--------|-------------|
| `vpc_id` | ID of the notism VPC. |
| `ec2_instance_id` | EC2 instance ID (notism-api). |
| `ec2_public_ip` | Public IP of the API (Elastic IP). Use for GitHub secret `EC2_HOST`. |
| `rds_endpoint` | Full RDS endpoint (host:port). |
| `rds_address` | RDS hostname only (for connection string). |
| `ecr_repository_url` | ECR repository URL for the notism-api image. |
| `connection_string_hint` | (sensitive) Template for the PostgreSQL connection string. |
| `api_url` | HTTP URL for the API (e.g. `http://<eip>:5000`). |

---

## Usage

### Prerequisites

- [Terraform](https://www.terraform.io/downloads) >= 1.0
- AWS credentials (e.g. `aws configure` or `AWS_ACCESS_KEY_ID` / `AWS_SECRET_ACCESS_KEY`)
- A value for the RDS master password

### Commands

```bash
cd terraform
terraform init
terraform plan -var="db_password=YOUR_SECURE_PASSWORD"
terraform apply -var="db_password=YOUR_SECURE_PASSWORD"
```

With a variable file:

```bash
terraform apply -var-file=terraform.tfvars
```

To destroy:

```bash
terraform destroy -var="db_password=YOUR_PASSWORD"
```

RDS is created with `skip_final_snapshot = true`; change this in `rds.tf` if you need a final backup before destroy.

### State

- State is stored locally by default (e.g. `terraform.tfstate` in `terraform/`).
- Do not commit state files if they might contain sensitive data.
- For team or CI use, configure a [remote backend](https://www.terraform.io/docs/language/settings/backends/index.html) (e.g. S3 + DynamoDB).

---

## Security and sensitive data

- **`db_password`**: Marked sensitive in Terraform; use `-var`, `-var-file`, or `TF_VAR_db_password`. Do not put in committed files.
- **State**: May contain IDs and metadata; avoid committing `*.tfstate` and `*.tfstate.*`. Use `.gitignore` (already in `terraform/.gitignore`).
- **tfvars**: Keep `terraform.tfvars` out of version control if it holds secrets; use `example.tfvars` as a template only.

---

## Relation to other deployment methods

- **Same architecture**: This Terraform setup produces the same layout as the [AWS CLI script](deploy-aws-cli.md) (VPC, subnets, RDS, EC2, EIP, ECR, IAM). You can adopt Terraform for net-new infrastructure or gradually replace CLI-created resources by importing them.
- **App deployment**: Terraform does not deploy the application or run the GitHub Actions workflow. After infrastructure is up, use the [deploy workflow](.github/workflows/deploy.yml) or the steps in [deploy-aws-cli.md](deploy-aws-cli.md) to build, push, and run the API on EC2.
- **Secrets**: Set GitHub Actions secrets (e.g. `EC2_HOST`, `CONNECTION_STRING`) from Terraform outputs and your chosen secret store; Terraform does not manage GitHub secrets.

---

## Related docs

- [terraform/README.md](../terraform/README.md) — Quick start in the Terraform directory.
- [aws-architecture.md](aws-architecture.md) — Architecture diagram and components.
- [deploy-aws-cli.md](deploy-aws-cli.md) — CLI deployment and GitHub Actions.
