# ------------------------------------------------------------------------------
# Staging env root
#
# Composes the reusable modules with staging inputs. name_suffix = "-staging"
# gives every resource a distinct name; the VPC uses a separate CIDR (10.1.0.0/16)
# from prod (10.0.0.0/16) to avoid any future peering clash. Staging is a fresh
# create — no moved/import adoption.
#
# github-oidc is an account-shared singleton instantiated only in the prod root;
# staging does NOT instantiate it and reuses the same account-level deploy roles.
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
