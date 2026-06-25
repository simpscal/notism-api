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
  prod/                    # composes modules with prod inputs; live infra adopted by hand at cutover (state mv + import)
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

### 2. prod adoption (manual state reconciliation + 0-change plan gate)

There are no committed `moved {}` / `import {}` blocks. Prod state is reconciled
by hand at cutover (matching the project's manual-cutover convention) using the
exhaustive command list in [Prod cutover — state reconciliation](#prod-cutover--state-reconciliation)
below. The ordered gate is:

```bash
cd terraform/environments/prod
# Build the lambda zip first if not present (see lambda-src/image-resizing/README.md):
#   -> terraform/lambda-packages/notism-image-resizing.zip

# 1. Migrate the existing flat/local state into the new S3 backend:
terraform init -migrate-state

# 2. Run EVERY `terraform state mv` + `terraform import` command from the
#    "Prod cutover — state reconciliation" section below (in that order).

# 3. GATE — the plan MUST be 0 add / 0 change / 0 destroy:
terraform plan -out tfplan
#    -> If the plan is not 0/0/0, STOP and reconcile (a mis-mapped address or an
#       input default diverged from live state) before applying.

# 4. Apply:
terraform apply tfplan
```

### 3. staging (fresh create)

```bash
cd terraform/environments/staging
terraform init
terraform apply
```

## The 0-change prod plan gate

`environments/prod` keeps prod resource names identical (`name_suffix = ""`), so
after the flat state is migrated and every resource is moved/imported to its new
module address (see below), `terraform plan` in prod must show **0 add / 0 change
/ 0 destroy**. A non-zero plan means an address mapping or an input default
diverged from live state — fix it before applying.

## Prod cutover — state reconciliation

Run these **by hand at cutover**, from `terraform/environments/prod/`, AFTER
`terraform init -migrate-state` and BEFORE the 0-change `terraform plan` gate.
These commands are documentation only — they are NOT committed as `moved`/`import`
blocks and CI never runs them.

### 3a. Relocate flat addresses to module addresses (`terraform state mv`)

One per resource — old flat address → new module address (39 total):

```bash
# network (vpc.tf, security_groups.tf)
terraform state mv 'aws_vpc.main'                       'module.network.aws_vpc.main'
terraform state mv 'aws_internet_gateway.main'          'module.network.aws_internet_gateway.main'
terraform state mv 'aws_subnet.public'                  'module.network.aws_subnet.public'
terraform state mv 'aws_route_table.public'             'module.network.aws_route_table.public'
terraform state mv 'aws_route_table_association.public'  'module.network.aws_route_table_association.public'
terraform state mv 'aws_security_group.ec2'             'module.network.aws_security_group.ec2'

# registry (ecr.tf)
terraform state mv 'aws_ecr_repository.api'             'module.registry.aws_ecr_repository.api'

# storage (s3.tf, incl. web_prod bucket policy)
terraform state mv 'aws_s3_bucket.private_storage'      'module.storage.aws_s3_bucket.private_storage'
terraform state mv 'aws_s3_bucket.public_storage'       'module.storage.aws_s3_bucket.public_storage'
terraform state mv 'aws_s3_bucket.web_prod'             'module.storage.aws_s3_bucket.web_prod'
terraform state mv 'aws_s3_bucket_server_side_encryption_configuration.private_storage' 'module.storage.aws_s3_bucket_server_side_encryption_configuration.private_storage'
terraform state mv 'aws_s3_bucket_server_side_encryption_configuration.public_storage'  'module.storage.aws_s3_bucket_server_side_encryption_configuration.public_storage'
terraform state mv 'aws_s3_bucket_server_side_encryption_configuration.web_prod'        'module.storage.aws_s3_bucket_server_side_encryption_configuration.web_prod'
terraform state mv 'aws_s3_bucket_ownership_controls.private_storage'  'module.storage.aws_s3_bucket_ownership_controls.private_storage'
terraform state mv 'aws_s3_bucket_ownership_controls.public_storage'   'module.storage.aws_s3_bucket_ownership_controls.public_storage'
terraform state mv 'aws_s3_bucket_ownership_controls.web_prod'         'module.storage.aws_s3_bucket_ownership_controls.web_prod'
terraform state mv 'aws_s3_bucket_public_access_block.private_storage' 'module.storage.aws_s3_bucket_public_access_block.private_storage'
terraform state mv 'aws_s3_bucket_public_access_block.public_storage'  'module.storage.aws_s3_bucket_public_access_block.public_storage'
terraform state mv 'aws_s3_bucket_public_access_block.web_prod'        'module.storage.aws_s3_bucket_public_access_block.web_prod'
terraform state mv 'aws_s3_bucket_cors_configuration.private_storage'  'module.storage.aws_s3_bucket_cors_configuration.private_storage'
terraform state mv 'aws_s3_bucket_cors_configuration.public_storage'   'module.storage.aws_s3_bucket_cors_configuration.public_storage'
terraform state mv 'aws_s3_bucket_notification.private_storage'        'module.storage.aws_s3_bucket_notification.private_storage'
terraform state mv 'aws_s3_bucket_policy.private_storage'              'module.storage.aws_s3_bucket_policy.private_storage'
terraform state mv 'aws_s3_bucket_policy.public_storage'               'module.storage.aws_s3_bucket_policy.public_storage'
terraform state mv 'aws_s3_bucket_policy.web_prod'                     'module.storage.aws_s3_bucket_policy.web_prod'

# cdn (cloudfront.tf)
terraform state mv 'aws_cloudfront_origin_access_control.web_prod'  'module.cdn.aws_cloudfront_origin_access_control.web_prod'
terraform state mv 'aws_cloudfront_distribution.web_prod'           'module.cdn.aws_cloudfront_distribution.web_prod'

# image-processing (lambda.tf)
terraform state mv 'aws_lambda_function.image_resizing'             'module.image_processing.aws_lambda_function.image_resizing'
terraform state mv 'aws_lambda_permission.s3_invoke_image_resizing' 'module.image_processing.aws_lambda_permission.s3_invoke_image_resizing'
terraform state mv 'aws_iam_role.lambda_image_resizing'            'module.image_processing.aws_iam_role.lambda_image_resizing'
terraform state mv 'aws_iam_role_policy.lambda_image_resizing_s3'  'module.image_processing.aws_iam_role_policy.lambda_image_resizing_s3'
terraform state mv 'aws_iam_role_policy_attachment.lambda_image_resizing_basic_execution' 'module.image_processing.aws_iam_role_policy_attachment.lambda_image_resizing_basic_execution'

# compute (ec2.tf, eip.tf)
terraform state mv 'aws_instance.api'                  'module.compute.aws_instance.api'
terraform state mv 'aws_eip.api'                       'module.compute.aws_eip.api'
terraform state mv 'aws_eip_association.api'           'module.compute.aws_eip_association.api'
terraform state mv 'aws_iam_role.ec2'                  'module.compute.aws_iam_role.ec2'
terraform state mv 'aws_iam_role_policy.ec2_s3'        'module.compute.aws_iam_role_policy.ec2_s3'
terraform state mv 'aws_iam_role_policy_attachment.ec2_ecr' 'module.compute.aws_iam_role_policy_attachment.ec2_ecr'
terraform state mv 'aws_iam_instance_profile.ec2'      'module.compute.aws_iam_instance_profile.ec2'
```

> If `aws_iam_role_policy.api_deploy_ec2_start` was already applied into the flat
> state, also move it (it was the one intended create in the flat root):
>
> ```bash
> terraform state mv 'aws_iam_role_policy.api_deploy_ec2_start' 'module.github_oidc.aws_iam_role_policy.api_deploy_ec2_start'
> ```

### 3b. Adopt the github-oidc resources (`terraform import`)

The account-shared github-oidc resources were originally adopted via `import {}`
blocks. Import them at their new module addresses (7 total). **Run an import only
if the resource is NOT already in state** (e.g. it was never applied, or was not
moved in 3a). The literal account id `249550149516` is the AWS account; it is an
import locator only — the module derives the account from `data.aws_caller_identity`.

```bash
terraform import 'module.github_oidc.aws_iam_openid_connect_provider.github_actions' 'arn:aws:iam::249550149516:oidc-provider/token.actions.githubusercontent.com'
terraform import 'module.github_oidc.aws_iam_role.api_deploy' 'notism-api-deploy-role'
terraform import 'module.github_oidc.aws_iam_role.web_deploy' 'notism-web-deploy'
terraform import 'module.github_oidc.aws_iam_role_policy_attachment.api_deploy_cloudfront' 'notism-api-deploy-role/arn:aws:iam::aws:policy/CloudFrontFullAccess'
terraform import 'module.github_oidc.aws_iam_role_policy_attachment.api_deploy_ecr' 'notism-api-deploy-role/arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryPowerUser'
terraform import 'module.github_oidc.aws_iam_role_policy_attachment.web_deploy_cloudfront' 'notism-web-deploy/arn:aws:iam::aws:policy/CloudFrontFullAccess'
terraform import 'module.github_oidc.aws_iam_role_policy_attachment.web_deploy_s3' 'notism-web-deploy/arn:aws:iam::aws:policy/AmazonS3FullAccess'
```

> `module.github_oidc.aws_iam_role_policy.api_deploy_ec2_start` has no separate
> live id to import — adopt it via the `state mv` in 3a if it was already applied,
> otherwise let the 0-change gate plan create it (it is the single intended create).

After 3a + 3b, return to step 2's `terraform plan` gate — it must be 0/0/0.
