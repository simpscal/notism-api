data "aws_caller_identity" "current" {}

# ------------------------------------------------------------------------------
# Lambda — Image / File Processing
#
# These functions are deployed in ap-northeast-1 (default provider) alongside
# the S3 buckets they process.  Deploying in the same region eliminates the
# S3 connection failures that occurred when the functions lived in us-east-1.
#
# The REGION environment variable tells the Node.js AWS SDK which region to use
# when initialising S3 clients.
#
# Handler source lives in lambda-src/image-resizing/ (see its README for the
# package build). The deployment zip is built and uploaded out of band and is
# excluded from Terraform management via ignore_changes — Terraform owns only
# configuration (env vars, IAM role, memory, timeout).
#
# runtime is set to nodejs22.x here because the current AWS provider version
# (5.100.0) does not yet enumerate nodejs24.x in its validation schema.
# ignore_changes = [runtime] prevents Terraform from modifying the actual
# runtime (nodejs24.x) during plan/apply.
# ------------------------------------------------------------------------------

locals {
  sharp_layer_arn = "arn:aws:lambda:${var.aws_region}:${data.aws_caller_identity.current.account_id}:layer:sharp:1"
}

resource "aws_lambda_function" "image_resizing" {
  function_name = "notism-image-resizing"
  role          = aws_iam_role.lambda_image_resizing.arn
  handler       = "index.handler"
  runtime       = "nodejs22.x"
  timeout       = 30
  memory_size   = 256
  architectures = ["arm64"]

  # Code is deployed by CI/CD; Terraform manages configuration only.
  filename = "${path.module}/lambda-packages/notism-image-resizing.zip"

  layers = [local.sharp_layer_arn]

  # RESIZE_JOBS maps a source upload prefix to the resize variants written to
  # DESTINATION_BUCKET in a single invocation. The handler replaces the source
  # key's first path segment with outputPrefix, so output prefixes match the
  # consuming StorageTypeConstants (avatar, food, food-detail). Keys mirror the
  # app's real upload folders (avatar/, food/ — see GenerateUploadUrlHandler).
  environment {
    variables = {
      DESTINATION_BUCKET = "public-notism-storage"
      REGION             = var.aws_region
      RESIZE_JOBS = jsonencode({
        "avatar/" = [{ outputPrefix = "avatar", width = 200, height = 200 }]
        "food/" = [
          { outputPrefix = "food", width = 400, height = 400 },
          { outputPrefix = "food-detail", width = 800, height = 800 },
        ]
      })
    }
  }

  lifecycle {
    ignore_changes = [filename, source_code_hash, layers, runtime]
  }

  tags = {
    Name = "notism-image-resizing"
  }
}

# ------------------------------------------------------------------------------
# Image Resizing — Execution Role
# ------------------------------------------------------------------------------

resource "aws_iam_role" "lambda_image_resizing" {
  name        = "notism-image-resizing-role"
  description = "Execution role for the Notism image-resizing Lambda"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "lambda.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  })

  tags = {
    Name = "notism-image-resizing-role"
  }
}

resource "aws_iam_role_policy" "lambda_image_resizing_s3" {
  name = "notism-s3-actions"
  role = aws_iam_role.lambda_image_resizing.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject",
        ]
        Resource = [
          "${var.public_storage_arn}/*",
          "${var.private_storage_arn}/*",
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_image_resizing_basic_execution" {
  role       = aws_iam_role.lambda_image_resizing.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}
