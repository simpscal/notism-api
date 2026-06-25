# ------------------------------------------------------------------------------
# Prod env root
#
# Composes the reusable modules with prod inputs. name_suffix = "" keeps every
# resource name identical to the pre-refactor flat root, so this root ADOPTS the
# live prod infrastructure with a 0-change plan (see moved.tf + README.md).
#
# Module wiring order (storage -> cdn -> image-processing -> compute):
#   - Bucket names/ARNs are derived from name_suffix in locals so the
#     image-processing module can reference them without creating a
#     storage<->image-processing dependency cycle (storage's bucket notification
#     consumes the lambda arn; image-processing consumes bucket names only).
#   - github-oidc is an account-shared singleton and is instantiated HERE ONLY,
#     never in the staging root.
# ------------------------------------------------------------------------------

locals {
  private_storage_bucket = "private-notism-storage${var.name_suffix}"
  public_storage_bucket  = "public-notism-storage${var.name_suffix}"
  web_bucket             = "notism-web-prod${var.name_suffix}"

  private_storage_arn = "arn:aws:s3:::${local.private_storage_bucket}"
  public_storage_arn  = "arn:aws:s3:::${local.public_storage_bucket}"
}

module "network" {
  source = "../../modules/network"

  environment        = var.environment
  name_suffix        = var.name_suffix
  vpc_cidr           = var.vpc_cidr
  public_subnet_cidr = var.public_subnet_cidr
}

module "registry" {
  source = "../../modules/registry"

  environment = var.environment
  name_suffix = var.name_suffix
}

module "image_processing" {
  source = "../../modules/image-processing"

  environment         = var.environment
  name_suffix         = var.name_suffix
  aws_region          = var.aws_region
  lambda_package_path = "${path.module}/../../lambda-packages/notism-image-resizing.zip"
  destination_bucket  = local.public_storage_bucket
  private_storage_arn = local.private_storage_arn
  public_storage_arn  = local.public_storage_arn
}

module "cdn" {
  source = "../../modules/cdn"

  environment = var.environment
  name_suffix = var.name_suffix
  aws_region  = var.aws_region

  # Literal web bucket name (derived from name_suffix) so cdn has no dependency
  # on the storage module — keeping the storage -> cdn edge one-directional.
  web_bucket_name = local.web_bucket
}

module "storage" {
  source = "../../modules/storage"

  environment                         = var.environment
  name_suffix                         = var.name_suffix
  image_resizing_lambda_arn           = module.image_processing.function_arn
  image_resizing_lambda_permission_id = module.image_processing.invoke_permission_id
  cloudfront_domain_name              = module.cdn.domain_name
  cloudfront_distribution_arn         = module.cdn.distribution_arn
}

module "compute" {
  source = "../../modules/compute"

  environment         = var.environment
  name_suffix         = var.name_suffix
  key_name            = var.key_name
  subnet_id           = module.network.public_subnet_id
  security_group_id   = module.network.ec2_security_group_id
  private_storage_arn = module.storage.private_storage_arn
  public_storage_arn  = module.storage.public_storage_arn
}

module "github_oidc" {
  source = "../../modules/github-oidc"

  api_instance_arn = module.compute.instance_arn
}
