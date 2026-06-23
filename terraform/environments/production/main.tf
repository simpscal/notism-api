data "aws_caller_identity" "current" {}

# ------------------------------------------------------------------------------
# Modules
# ------------------------------------------------------------------------------

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

# ------------------------------------------------------------------------------
# Glue — cross-cutting resources that reference BOTH storage and compute.
#
# These wire the S3 ↔ CloudFront ↔ Lambda relationships at the root to break the
# module dependency cycle. They keep their original top-level addresses, so no
# moved {} blocks are required for them.
# ------------------------------------------------------------------------------

# ------------------------------------------------------------------------------
# CORS Configuration (storage buckets only)
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_cors_configuration" "private_storage" {
  bucket = module.storage.private_storage_id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["PUT", "GET", "DELETE", "HEAD"]
    allowed_origins = [
      "http://localhost:4200",
      "https://${module.compute.cloudfront_web_prod_domain_name}",
      "http://${module.compute.cloudfront_web_prod_domain_name}",
    ]
    expose_headers  = ["ETag", "x-amz-server-side-encryption", "x-amz-request-id", "x-amz-id-2"]
    max_age_seconds = 3000
  }
}

resource "aws_s3_bucket_cors_configuration" "public_storage" {
  bucket = module.storage.public_storage_id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["PUT", "GET", "DELETE", "HEAD"]
    allowed_origins = ["http://localhost:4200"]
    expose_headers  = ["ETag", "x-amz-server-side-encryption", "x-amz-request-id", "x-amz-id-2"]
    max_age_seconds = 3000
  }
}

# ------------------------------------------------------------------------------
# S3 Event Notifications
#
# private-notism-storage triggers the image-resizing pipeline:
#   - ObjectCreated under avatar/ or food/ → notism-image-resizing
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_notification" "private_storage" {
  bucket = module.storage.private_storage_id

  lambda_function {
    lambda_function_arn = module.compute.image_resizing_arn
    events              = ["s3:ObjectCreated:*"]
    filter_prefix       = "avatar/"
  }

  lambda_function {
    lambda_function_arn = module.compute.image_resizing_arn
    events              = ["s3:ObjectCreated:*"]
    filter_prefix       = "food/"
  }

  depends_on = [
    aws_lambda_permission.s3_invoke_image_resizing,
  ]
}

# ------------------------------------------------------------------------------
# Lambda resource-based policy — allow S3 to invoke the resize function
# ------------------------------------------------------------------------------

resource "aws_lambda_permission" "s3_invoke_image_resizing" {
  statement_id   = "AllowS3InvokeImageResizing"
  action         = "lambda:InvokeFunction"
  function_name  = module.compute.image_resizing_function_name
  principal      = "s3.amazonaws.com"
  source_arn     = module.storage.private_storage_arn
  source_account = data.aws_caller_identity.current.account_id
}

# ------------------------------------------------------------------------------
# Bucket Policies
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_policy" "private_storage" {
  bucket = module.storage.private_storage_id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "AllowPublicAccessFromOrigins"
        Effect    = "Allow"
        Principal = "*"
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject",
        ]
        Resource = "${module.storage.private_storage_arn}/*"
        Condition = {
          StringLike = {
            "aws:Referer" = [
              "http://localhost:4200/*",
              "https://${module.compute.cloudfront_web_prod_domain_name}/*",
              "http://${module.compute.cloudfront_web_prod_domain_name}/*",
            ]
          }
        }
      }
    ]
  })
}

resource "aws_s3_bucket_policy" "public_storage" {
  bucket = module.storage.public_storage_id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "PublicReadGetObject"
        Effect    = "Allow"
        Principal = "*"
        Action    = "s3:GetObject"
        Resource  = "${module.storage.public_storage_arn}/*"
      }
    ]
  })
}

resource "aws_s3_bucket_policy" "web_prod" {
  bucket = module.storage.web_prod_id

  policy = jsonencode({
    Version = "2008-10-17"
    Id      = "PolicyForCloudFrontPrivateContent"
    Statement = [
      {
        Sid    = "AllowCloudFrontServicePrincipal"
        Effect = "Allow"
        Principal = {
          Service = "cloudfront.amazonaws.com"
        }
        Action   = "s3:GetObject"
        Resource = "${module.storage.web_prod_arn}/*"
        Condition = {
          ArnLike = {
            "AWS:SourceArn" = module.compute.cloudfront_web_prod_arn
          }
        }
      }
    ]
  })
}
