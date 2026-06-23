# Development environment — SCAFFOLD ONLY. Not initialised, not applied.
#
# Mirrors the production root's module wiring so the layout validates, but is not
# wired for a real apply yet. Before applying development, the storage module's
# hard-coded global bucket names ("private-notism-storage", "public-notism-storage",
# "notism-web-prod") and the compute module's fixed resource names must be made
# per-environment (e.g. name prefix from var.environment) — otherwise a
# development apply collides with the production resources. Tracked as follow-up.

data "aws_caller_identity" "current" {}

module "vpc" {
  source = "../../modules/vpc"

  vpc_cidr           = var.vpc_cidr
  public_subnet_cidr = var.public_subnet_cidr
}

module "storage" {
  source = "../../modules/storage"
}

module "compute" {
  source = "../../modules/compute"

  aws_region                           = var.aws_region
  vpc_id                               = module.vpc.vpc_id
  public_subnet_id                     = module.vpc.public_subnet_id
  key_name                             = var.key_name
  private_storage_arn                  = module.storage.private_storage_arn
  public_storage_arn                   = module.storage.public_storage_arn
  web_prod_bucket_regional_domain_name = module.storage.web_prod_bucket_regional_domain_name
}
