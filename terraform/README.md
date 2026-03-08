# Notism AWS infrastructure (Terraform)

This directory defines the Notism API infrastructure on AWS: VPC, subnets, security groups, RDS PostgreSQL, IAM role, EC2 instance, Elastic IP, and ECR repository. All resources use the **notism** prefix to match the CLI-based setup.

**Full documentation:** [docs/terraform-configuration.md](../docs/terraform-configuration.md) — file structure, variables, resources, outputs, and security.

## Prerequisites

- [Terraform](https://www.terraform.io/downloads) >= 1.0
- AWS credentials configured (e.g. `aws configure` or env vars)
- A value for the RDS master password

## Usage

1. **Initialize Terraform**

   ```bash
   cd terraform
   terraform init
   ```

2. **Plan (optional)**

   ```bash
   terraform plan -var="db_password=YOUR_SECURE_PASSWORD"
   ```

   Or use a `terraform.tfvars` file (add it to `.gitignore`; do not commit passwords):

   ```hcl
   db_password = "YOUR_SECURE_PASSWORD"
   aws_region  = "us-east-1"
   # key_name   = "your-ec2-key-pair"  # optional, for SSH
   ```

3. **Apply**

   ```bash
   terraform apply -var="db_password=YOUR_SECURE_PASSWORD"
   ```

   Or with tfvars:

   ```bash
   terraform apply -var-file=terraform.tfvars
   ```

4. **Outputs**

   After apply, Terraform prints outputs such as `ec2_public_ip`, `rds_address`, `ecr_repository_url`, and `api_url`. Use these for GitHub Actions secrets (e.g. `EC2_HOST` = `ec2_public_ip`) and connection strings.

## Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `aws_region` | AWS region | `us-east-1` |
| `vpc_cidr` | VPC CIDR block | `10.0.0.0/16` |
| `public_subnet_cidr` | Public subnet CIDR | `10.0.1.0/24` |
| `private_subnet_cidrs` | Private subnet CIDRs (min 2 for RDS) | `["10.0.2.0/24", "10.0.3.0/24"]` |
| `db_password` | RDS master password | (required, sensitive) |
| `key_name` | EC2 key pair name for SSH | `null` (optional) |
| `environment` | Environment tag | `prod` |

## Resource overview

- **VPC**: notism-vpc, one public subnet, two private subnets (2 AZs for RDS)
- **Route tables**: notism-public-rt (0.0.0.0/0 → IGW), notism-private-rt (no IGW)
- **Security groups**: notism-ec2-sg (22, 80, 443, 5000), notism-rds-sg (5432 from EC2 only)
- **RDS**: notism-db (PostgreSQL 16.13, db.t4g.micro, notism_db)
- **IAM**: NotismEC2Role, NotismEC2Profile (S3 + ECR)
- **EC2**: notism-api (t4g.micro, Amazon Linux 2023 ARM, Docker in user data)
- **Elastic IP**: notism-api-eip
- **ECR**: notism-api repository

## Destroying

```bash
terraform destroy -var="db_password=YOUR_PASSWORD"
```

RDS has `skip_final_snapshot = true` by default; adjust in `rds.tf` if you need a final snapshot.
