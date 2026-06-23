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
# Code packages are deployed by CI/CD and are excluded from Terraform management
# via ignore_changes.  Terraform only owns configuration (env vars, IAM role,
# memory, timeout).
#
# runtime is set to nodejs22.x here because the current AWS provider version
# (5.100.0) does not yet enumerate nodejs24.x in its validation schema.
# ignore_changes = [runtime] prevents Terraform from modifying the actual
# runtime (nodejs24.x) during plan/apply.
# ------------------------------------------------------------------------------

data "aws_caller_identity" "current" {}

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
  filename = "./lambda-packages/notism-image-resizing.zip"

  layers = [local.sharp_layer_arn]

  # RESIZE_JOBS maps a source upload prefix to the resize variants written to
  # DESTINATION_BUCKET in a single invocation. Output prefixes are byte-identical
  # to the consuming StorageTypeConstants (avatar, food, food-detail).
  environment {
    variables = {
      DESTINATION_BUCKET = "public-notism-storage"
      REGION             = var.aws_region
      RESIZE_JOBS = jsonencode({
        "avatars/" = [{ outputPrefix = "avatar", width = 200, height = 200 }]
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
# Lambda resource-based policy — allow S3 to invoke the resize function
# ------------------------------------------------------------------------------

resource "aws_lambda_permission" "s3_invoke_image_resizing" {
  statement_id   = "AllowS3InvokeImageResizing"
  action         = "lambda:InvokeFunction"
  function_name  = aws_lambda_function.image_resizing.function_name
  principal      = "s3.amazonaws.com"
  source_arn     = aws_s3_bucket.private_storage.arn
  source_account = data.aws_caller_identity.current.account_id
}
