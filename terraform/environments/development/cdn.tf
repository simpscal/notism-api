module "cdn" {
  source = "../../modules/cdn"

  aws_region                           = var.aws_region
  web_prod_bucket_regional_domain_name = module.storage.web_prod_bucket_regional_domain_name
}
