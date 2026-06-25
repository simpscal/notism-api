# ------------------------------------------------------------------------------
# Prod adoption — moved {} blocks
#
# Maps every pre-refactor flat top-level resource address to its new module
# address. This is the mechanism that makes `terraform plan` in prod show
# 0 add / 0 change / 0 destroy after `terraform init -migrate-state`.
#
# DO NOT run apply from this refactor. The plan-must-be-0-change gate and the
# init -migrate-state are MANUAL pre-release steps (see terraform/README.md).
#
# github-oidc note: those resources were originally adopted into the flat root
# via `import {}` blocks (see import.tf). If they already live in state under the
# flat addresses, the moved blocks below relocate them; if not yet in state, the
# import blocks in import.tf bring them in at the new module addresses. Both are
# no-ops once state matches.
# ------------------------------------------------------------------------------

# ----- network (vpc.tf, security_groups.tf) -----------------------------------

moved {
  from = aws_vpc.main
  to   = module.network.aws_vpc.main
}

moved {
  from = aws_internet_gateway.main
  to   = module.network.aws_internet_gateway.main
}

moved {
  from = aws_subnet.public
  to   = module.network.aws_subnet.public
}

moved {
  from = aws_route_table.public
  to   = module.network.aws_route_table.public
}

moved {
  from = aws_route_table_association.public
  to   = module.network.aws_route_table_association.public
}

moved {
  from = aws_security_group.ec2
  to   = module.network.aws_security_group.ec2
}

# ----- registry (ecr.tf) ------------------------------------------------------

moved {
  from = aws_ecr_repository.api
  to   = module.registry.aws_ecr_repository.api
}

# ----- storage (s3.tf, incl. web_prod bucket policy) --------------------------

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

moved {
  from = aws_s3_bucket_cors_configuration.private_storage
  to   = module.storage.aws_s3_bucket_cors_configuration.private_storage
}

moved {
  from = aws_s3_bucket_cors_configuration.public_storage
  to   = module.storage.aws_s3_bucket_cors_configuration.public_storage
}

moved {
  from = aws_s3_bucket_notification.private_storage
  to   = module.storage.aws_s3_bucket_notification.private_storage
}

moved {
  from = aws_s3_bucket_policy.private_storage
  to   = module.storage.aws_s3_bucket_policy.private_storage
}

moved {
  from = aws_s3_bucket_policy.public_storage
  to   = module.storage.aws_s3_bucket_policy.public_storage
}

moved {
  from = aws_s3_bucket_policy.web_prod
  to   = module.storage.aws_s3_bucket_policy.web_prod
}

# ----- cdn (cloudfront.tf) ----------------------------------------------------

moved {
  from = aws_cloudfront_origin_access_control.web_prod
  to   = module.cdn.aws_cloudfront_origin_access_control.web_prod
}

moved {
  from = aws_cloudfront_distribution.web_prod
  to   = module.cdn.aws_cloudfront_distribution.web_prod
}

# ----- image-processing (lambda.tf) -------------------------------------------

moved {
  from = aws_lambda_function.image_resizing
  to   = module.image_processing.aws_lambda_function.image_resizing
}

moved {
  from = aws_lambda_permission.s3_invoke_image_resizing
  to   = module.image_processing.aws_lambda_permission.s3_invoke_image_resizing
}

moved {
  from = aws_iam_role.lambda_image_resizing
  to   = module.image_processing.aws_iam_role.lambda_image_resizing
}

moved {
  from = aws_iam_role_policy.lambda_image_resizing_s3
  to   = module.image_processing.aws_iam_role_policy.lambda_image_resizing_s3
}

moved {
  from = aws_iam_role_policy_attachment.lambda_image_resizing_basic_execution
  to   = module.image_processing.aws_iam_role_policy_attachment.lambda_image_resizing_basic_execution
}

# ----- compute (ec2.tf, eip.tf) -----------------------------------------------

moved {
  from = aws_instance.api
  to   = module.compute.aws_instance.api
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

# ----- github-oidc (github_oidc.tf) -------------------------------------------
#
# These resources were adopted into the flat root via `import {}` blocks, so
# their flat-state presence at refactor time is not guaranteed. A `moved` here
# would collide with the import block targeting the same new address. Adoption is
# therefore handled exclusively by import.tf (config-driven imports at the new
# module addresses). Left as import references rather than moved blocks:
#
#   import: see import.tf -> module.github_oidc.aws_iam_openid_connect_provider.github_actions
#   import: see import.tf -> module.github_oidc.aws_iam_role.api_deploy
#   import: see import.tf -> module.github_oidc.aws_iam_role.web_deploy
#   import: see import.tf -> module.github_oidc.aws_iam_role_policy_attachment.api_deploy_cloudfront
#   import: see import.tf -> module.github_oidc.aws_iam_role_policy_attachment.api_deploy_ecr
#   import: see import.tf -> module.github_oidc.aws_iam_role_policy_attachment.web_deploy_cloudfront
#   import: see import.tf -> module.github_oidc.aws_iam_role_policy_attachment.web_deploy_s3
#
# api_deploy_ec2_start has no live id to import (it was the one intended create
# in the flat root). If it was already applied, run:
#   # import: terraform import module.github_oidc.aws_iam_role_policy.api_deploy_ec2_start notism-api-deploy-role:notism-api-deploy-ec2-start
