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
