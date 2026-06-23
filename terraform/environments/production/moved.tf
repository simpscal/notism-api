# ------------------------------------------------------------------------------
# moved {} blocks — relocate every existing flat-root resource address to its new
# nested module address so a future `terraform plan` shows MOVES, not
# destroy/create. No resource is recreated.
#
# Glue resources (aws_s3_bucket_policy.*, aws_s3_bucket_cors_configuration.*,
# aws_s3_bucket_notification.private_storage, aws_lambda_permission.s3_invoke_image_resizing)
# keep their original top-level address in this root, so they need NO moved block.
#
# Global resources (OIDC provider, api_deploy/web_deploy roles + policies) migrate
# to a SEPARATE state via import + state rm at cutover — NOT via moved blocks.
# ------------------------------------------------------------------------------

# ------------------------------------------------------------------------------
# module.vpc
# ------------------------------------------------------------------------------

moved {
  from = aws_vpc.main
  to   = module.vpc.aws_vpc.main
}

moved {
  from = aws_internet_gateway.main
  to   = module.vpc.aws_internet_gateway.main
}

moved {
  from = aws_subnet.public
  to   = module.vpc.aws_subnet.public
}

moved {
  from = aws_route_table.public
  to   = module.vpc.aws_route_table.public
}

moved {
  from = aws_route_table_association.public
  to   = module.vpc.aws_route_table_association.public
}

# ------------------------------------------------------------------------------
# module.compute
# ------------------------------------------------------------------------------

moved {
  from = aws_instance.api
  to   = module.compute.aws_instance.api
}

moved {
  from = aws_iam_role.ec2
  to   = module.compute.aws_iam_role.ec2
}

moved {
  from = aws_iam_role_policy.ec2_s3
  to   = module.compute.aws_iam_role_policy.ec2_s3
}

moved {
  from = aws_iam_role_policy_attachment.ec2_ecr
  to   = module.compute.aws_iam_role_policy_attachment.ec2_ecr
}

moved {
  from = aws_iam_instance_profile.ec2
  to   = module.compute.aws_iam_instance_profile.ec2
}

moved {
  from = aws_eip.api
  to   = module.compute.aws_eip.api
}

moved {
  from = aws_eip_association.api
  to   = module.compute.aws_eip_association.api
}

moved {
  from = aws_security_group.ec2
  to   = module.compute.aws_security_group.ec2
}

moved {
  from = aws_ecr_repository.api
  to   = module.compute.aws_ecr_repository.api
}

moved {
  from = aws_lambda_function.image_resizing
  to   = module.compute.aws_lambda_function.image_resizing
}

moved {
  from = aws_iam_role.lambda_image_resizing
  to   = module.compute.aws_iam_role.lambda_image_resizing
}

moved {
  from = aws_iam_role_policy.lambda_image_resizing_s3
  to   = module.compute.aws_iam_role_policy.lambda_image_resizing_s3
}

moved {
  from = aws_iam_role_policy_attachment.lambda_image_resizing_basic_execution
  to   = module.compute.aws_iam_role_policy_attachment.lambda_image_resizing_basic_execution
}

moved {
  from = aws_cloudfront_distribution.web_prod
  to   = module.compute.aws_cloudfront_distribution.web_prod
}

moved {
  from = aws_cloudfront_origin_access_control.web_prod
  to   = module.compute.aws_cloudfront_origin_access_control.web_prod
}

# ------------------------------------------------------------------------------
# module.storage
# ------------------------------------------------------------------------------

moved {
  from = aws_s3_bucket.private_storage
  to   = module.storage.aws_s3_bucket.private_storage
}

moved {
  from = aws_s3_bucket.public_storage
  to   = module.storage.aws_s3_bucket.public_storage
}

moved {
  from = aws_s3_bucket.web_prod
  to   = module.storage.aws_s3_bucket.web_prod
}

moved {
  from = aws_s3_bucket_server_side_encryption_configuration.private_storage
  to   = module.storage.aws_s3_bucket_server_side_encryption_configuration.private_storage
}

moved {
  from = aws_s3_bucket_server_side_encryption_configuration.public_storage
  to   = module.storage.aws_s3_bucket_server_side_encryption_configuration.public_storage
}

moved {
  from = aws_s3_bucket_server_side_encryption_configuration.web_prod
  to   = module.storage.aws_s3_bucket_server_side_encryption_configuration.web_prod
}

moved {
  from = aws_s3_bucket_ownership_controls.private_storage
  to   = module.storage.aws_s3_bucket_ownership_controls.private_storage
}

moved {
  from = aws_s3_bucket_ownership_controls.public_storage
  to   = module.storage.aws_s3_bucket_ownership_controls.public_storage
}

moved {
  from = aws_s3_bucket_ownership_controls.web_prod
  to   = module.storage.aws_s3_bucket_ownership_controls.web_prod
}

moved {
  from = aws_s3_bucket_public_access_block.private_storage
  to   = module.storage.aws_s3_bucket_public_access_block.private_storage
}

moved {
  from = aws_s3_bucket_public_access_block.public_storage
  to   = module.storage.aws_s3_bucket_public_access_block.public_storage
}

moved {
  from = aws_s3_bucket_public_access_block.web_prod
  to   = module.storage.aws_s3_bucket_public_access_block.web_prod
}
