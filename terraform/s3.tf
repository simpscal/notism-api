# ------------------------------------------------------------------------------
# S3 Buckets
# ------------------------------------------------------------------------------

resource "aws_s3_bucket" "private_storage" {
  bucket = "private-notism-storage"
}

resource "aws_s3_bucket" "public_storage" {
  bucket = "public-notism-storage"
}

resource "aws_s3_bucket" "web_prod" {
  bucket = "notism-web-prod"
}

# ------------------------------------------------------------------------------
# Server-Side Encryption
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_server_side_encryption_configuration" "private_storage" {
  bucket = aws_s3_bucket.private_storage.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
    bucket_key_enabled = true
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "public_storage" {
  bucket = aws_s3_bucket.public_storage.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
    bucket_key_enabled = true
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "web_prod" {
  bucket = aws_s3_bucket.web_prod.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
    bucket_key_enabled = true
  }
}

# ------------------------------------------------------------------------------
# Ownership Controls
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_ownership_controls" "private_storage" {
  bucket = aws_s3_bucket.private_storage.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_ownership_controls" "public_storage" {
  bucket = aws_s3_bucket.public_storage.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

resource "aws_s3_bucket_ownership_controls" "web_prod" {
  bucket = aws_s3_bucket.web_prod.id

  rule {
    object_ownership = "BucketOwnerEnforced"
  }
}

# ------------------------------------------------------------------------------
# Public Access Block
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_public_access_block" "private_storage" {
  bucket = aws_s3_bucket.private_storage.id

  block_public_acls       = false
  ignore_public_acls      = false
  block_public_policy     = false
  restrict_public_buckets = false
}

resource "aws_s3_bucket_public_access_block" "public_storage" {
  bucket = aws_s3_bucket.public_storage.id

  block_public_acls       = false
  ignore_public_acls      = false
  block_public_policy     = false
  restrict_public_buckets = false
}

resource "aws_s3_bucket_public_access_block" "web_prod" {
  bucket = aws_s3_bucket.web_prod.id

  block_public_acls       = true
  ignore_public_acls      = true
  block_public_policy     = true
  restrict_public_buckets = true
}

# ------------------------------------------------------------------------------
# CORS Configuration (storage buckets only)
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_cors_configuration" "private_storage" {
  bucket = aws_s3_bucket.private_storage.id

  cors_rule {
    allowed_headers = ["*"]
    allowed_methods = ["PUT", "GET", "DELETE", "HEAD"]
    allowed_origins = [
      "http://localhost:4200",
      "https://${aws_cloudfront_distribution.web_prod.domain_name}",
      "http://${aws_cloudfront_distribution.web_prod.domain_name}",
    ]
    expose_headers  = ["ETag", "x-amz-server-side-encryption", "x-amz-request-id", "x-amz-id-2"]
    max_age_seconds = 3000
  }
}

resource "aws_s3_bucket_cors_configuration" "public_storage" {
  bucket = aws_s3_bucket.public_storage.id

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
#   - ObjectCreated under avatars/  → notism-avatar-resizing
#   - ObjectCreated under food/     → notism-food-resizing
#
# NOTE: S3 event notifications require the target Lambda to reside in the same
# AWS region as the bucket.  The Lambda functions currently live in us-east-1
# while this bucket is in ap-northeast-1.  This resource is gated behind
# var.enable_s3_lambda_notifications (default: false) and must remain disabled
# until the Lambda migration follow-on story is complete.  Flip the variable to
# true in terraform.tfvars after the migration to activate the notifications.
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_notification" "private_storage" {
  count  = var.enable_s3_lambda_notifications ? 1 : 0
  bucket = aws_s3_bucket.private_storage.id

  lambda_function {
    lambda_function_arn = aws_lambda_function.avatar_resizing.arn
    events              = ["s3:ObjectCreated:*"]
    filter_prefix       = "avatars/"
  }

  lambda_function {
    lambda_function_arn = aws_lambda_function.food_resizing.arn
    events              = ["s3:ObjectCreated:*"]
    filter_prefix       = "food/"
  }

  depends_on = [
    aws_lambda_permission.s3_invoke_food_resizing,
    aws_lambda_permission.s3_invoke_avatar_resizing,
  ]
}

# ------------------------------------------------------------------------------
# Bucket Policies
# ------------------------------------------------------------------------------

resource "aws_s3_bucket_policy" "private_storage" {
  bucket = aws_s3_bucket.private_storage.id

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
        Resource = "${aws_s3_bucket.private_storage.arn}/*"
        Condition = {
          StringLike = {
            "aws:Referer" = [
              "http://localhost:4200/*",
              "https://${aws_cloudfront_distribution.web_prod.domain_name}/*",
              "http://${aws_cloudfront_distribution.web_prod.domain_name}/*",
            ]
          }
        }
      }
    ]
  })
}

resource "aws_s3_bucket_policy" "public_storage" {
  bucket = aws_s3_bucket.public_storage.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Sid       = "PublicReadGetObject"
        Effect    = "Allow"
        Principal = "*"
        Action    = "s3:GetObject"
        Resource  = "${aws_s3_bucket.public_storage.arn}/*"
      }
    ]
  })
}

resource "aws_s3_bucket_policy" "web_prod" {
  bucket = aws_s3_bucket.web_prod.id

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
        Resource = "${aws_s3_bucket.web_prod.arn}/*"
        Condition = {
          ArnLike = {
            "AWS:SourceArn" = aws_cloudfront_distribution.web_prod.arn
          }
        }
      }
    ]
  })
}
