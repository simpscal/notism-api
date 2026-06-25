# Notism Terraform

Reusable modules + per-env roots for a single AWS account (`249550149516`),
supporting **prod** and **staging** with per-env isolated S3 remote state.

## Layout

```
bootstrap/                 # local-state root: provisions the remote-state backend (S3 + DynamoDB). Apply once.
modules/
  network/                 # vpc, subnet, igw, route table, ec2 security group
  compute/                 # ec2 instance, eip, ec2 IAM role/profile/policy
  storage/                 # s3 buckets (private/public/web), encryption, policies, cors, notification
  cdn/                     # cloudfront distribution + origin access control
  registry/                # ecr repository
  image-processing/        # image-resizing lambda + IAM (DESTINATION_BUCKET wired from storage)
  github-oidc/             # OIDC provider + deploy roles (account-shared singleton)
environments/
  prod/                    # composes modules with prod inputs; ADOPTS live infra (moved.tf + import.tf)
  staging/                 # composes modules with staging inputs; fresh create
lambda-src/                # lambda handler source (zip built out of band into lambda-packages/)
```

## Suffix contract

Every module takes `environment` and `name_suffix`. Resource names are
`"<base>${var.name_suffix}"`.

- **prod** passes `name_suffix = ""` → names are UNCHANGED from the pre-refactor
  flat root (`private-notism-storage`, `notism-api`, `notism-ec2-role`,
  `notism-image-resizing`, `notism-ec2-sg`, ...). This is what makes prod
  adoptable with a 0-change plan.
- **staging** passes `name_suffix = "-staging"` →
  `private-notism-storage-staging`, `notism-api-staging`, etc.

`github-oidc` is an account-shared singleton (one OIDC provider + deploy roles
per account) → instantiated in the **prod root only**. Staging reuses the same
account-level roles and does NOT instantiate it.

## Per-env remote state

Single backend bucket `notism-tfstate` + lock table `notism-tflock`, isolated by key:

| Env     | State key                        |
| ------- | -------------------------------- |
| prod    | `env/prod/terraform.tfstate`     |
| staging | `env/staging/terraform.tfstate`  |

Staging VPC uses CIDR `10.1.0.0/16` (subnet `10.1.1.0/24`); prod keeps
`10.0.0.0/16` (subnet `10.0.1.0/24`) to avoid any future peering clash.

## Apply order (MANUAL — CI runs no terraform)

Terraform is never applied by CI. All of the following are manual pre-release /
cutover steps run by a human with AWS credentials.

### 1. bootstrap (once, before any env root)

```bash
cd terraform/bootstrap
terraform init
terraform apply        # creates notism-tfstate + notism-tflock
```

### 2. prod adoption (0-change plan gate)

```bash
cd terraform/environments/prod
# Build the lambda zip first if not present (see lambda-src/image-resizing/README.md):
#   -> terraform/lambda-packages/notism-image-resizing.zip

# Migrate the existing flat/local state into the new S3 backend:
terraform init -migrate-state

# moved.tf relocates flat addresses into module addresses; import.tf adopts the
# github-oidc resources. The plan MUST be 0 add / 0 change / 0 destroy:
terraform plan -out tfplan
#   -> GATE: if the plan is not 0/0/0, STOP and reconcile before applying.
terraform apply tfplan
```

### 3. staging (fresh create)

```bash
cd terraform/environments/staging
terraform init
terraform apply
```

## The 0-change prod plan gate

`environments/prod` keeps prod resource names identical (`name_suffix = ""`) and
maps every former flat resource address to its new module address via
`moved.tf`. The `github-oidc` resources are adopted via `import.tf`. After
`init -migrate-state`, `terraform plan` in prod must show **0 add / 0 change /
0 destroy**. A non-zero plan means an address mapping or an input default
diverged from live state — fix it before applying.
