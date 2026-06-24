module "compute" {
  source = "../../modules/compute"

  aws_region          = var.aws_region
  vpc_id              = module.vpc.vpc_id
  public_subnet_id    = module.vpc.public_subnet_id
  key_name            = var.key_name
  private_storage_arn = module.storage.private_storage_arn
  public_storage_arn  = module.storage.public_storage_arn
}
