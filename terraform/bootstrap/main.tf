# ------------------------------------------------------------------------------
# Bootstrap — Remote State Backend
#
# Provisions the S3 bucket + DynamoDB table that hold every env root's remote
# state and state lock. This root uses LOCAL state itself (chicken-and-egg: it
# creates the very backend the other roots use) and is applied ONCE, by hand,
# before any env root is initialised. See README.md.
# ------------------------------------------------------------------------------

terraform {
  # Local state on purpose — bootstrap provisions the remote backend others use.
}

provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project   = "notism"
      ManagedBy = "terraform"
      Component = "tfstate-backend"
    }
  }
}

variable "aws_region" {
  description = "AWS region for the state backend"
  type        = string
  default     = "ap-northeast-1"
}

locals {
  state_bucket = "notism-tfstate"
  lock_table   = "notism-tflock"
}

# ------------------------------------------------------------------------------
# Remote state bucket
# ------------------------------------------------------------------------------

resource "aws_s3_bucket" "tfstate" {
  bucket = local.state_bucket

  # Remote state is irreplaceable; guard against accidental destroy.
  lifecycle {
    prevent_destroy = true
  }

  tags = {
    Name = local.state_bucket
  }
}

resource "aws_s3_bucket_versioning" "tfstate" {
  bucket = aws_s3_bucket.tfstate.id

  versioning_configuration {
    status = "Enabled"
  }
}

resource "aws_s3_bucket_server_side_encryption_configuration" "tfstate" {
  bucket = aws_s3_bucket.tfstate.id

  rule {
    apply_server_side_encryption_by_default {
      sse_algorithm = "AES256"
    }
    bucket_key_enabled = true
  }
}

resource "aws_s3_bucket_public_access_block" "tfstate" {
  bucket = aws_s3_bucket.tfstate.id

  block_public_acls       = true
  ignore_public_acls      = true
  block_public_policy     = true
  restrict_public_buckets = true
}

# ------------------------------------------------------------------------------
# State lock table
# ------------------------------------------------------------------------------

resource "aws_dynamodb_table" "tflock" {
  name         = local.lock_table
  billing_mode = "PAY_PER_REQUEST"
  hash_key     = "LockID"

  attribute {
    name = "LockID"
    type = "S"
  }

  tags = {
    Name = local.lock_table
  }
}
