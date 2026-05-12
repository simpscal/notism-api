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

resource "aws_lambda_function" "food_resizing" {
  function_name = "notism-food-resizing"
  role          = aws_iam_role.lambda_image_resizing.arn
  handler       = "index.handler"
  runtime       = "nodejs22.x"
  timeout       = 30
  memory_size   = 256
  architectures = ["arm64"]

  # Code is deployed by CI/CD; Terraform manages configuration only.
  filename = "./lambda-packages/notism-food-resizing.zip"

  layers = [var.lambda_sharp_layer_arn]

  environment {
    variables = {
      DESTINATION_BUCKET   = "public-notism-storage"
      INVOKE_NEXT_FUNCTION = "notism-food-detail-resizing"
      RESIZE_WIDTH         = "400"
      RESIZE_HEIGHT        = "400"
      REGION               = var.aws_region
    }
  }

  lifecycle {
    ignore_changes = [filename, source_code_hash, layers, runtime]
  }

  tags = {
    Name = "notism-food-resizing"
  }
}

resource "aws_lambda_function" "food_detail_resizing" {
  function_name = "notism-food-detail-resizing"
  role          = aws_iam_role.lambda_image_resizing.arn
  handler       = "index.handler"
  runtime       = "nodejs22.x"
  timeout       = 30
  memory_size   = 256
  architectures = ["arm64"]

  filename = "./lambda-packages/notism-food-detail-resizing.zip"

  layers = [var.lambda_sharp_layer_arn]

  environment {
    variables = {
      DESTINATION_BUCKET = "public-notism-storage"
      DESTINATION_PREFIX = "food-detail"
      RESIZE_WIDTH       = "800"
      RESIZE_HEIGHT      = "800"
      REGION             = var.aws_region
    }
  }

  lifecycle {
    ignore_changes = [filename, source_code_hash, layers, runtime]
  }

  tags = {
    Name = "notism-food-detail-resizing"
  }
}

resource "aws_lambda_function" "avatar_resizing" {
  function_name = "notism-avatar-resizing"
  role          = aws_iam_role.lambda_image_resizing.arn
  handler       = "index.handler"
  runtime       = "nodejs22.x"
  timeout       = 3
  memory_size   = 128
  architectures = ["arm64"]

  filename = "./lambda-packages/notism-avatar-resizing.zip"

  layers = [var.lambda_sharp_layer_arn]

  environment {
    variables = {
      DESTINATION_BUCKET = "public-notism-storage"
      RESIZE_WIDTH       = "200"
      RESIZE_HEIGHT      = "200"
      REGION             = var.aws_region
    }
  }

  lifecycle {
    ignore_changes = [filename, source_code_hash, layers, runtime]
  }

  tags = {
    Name = "notism-avatar-resizing"
  }
}

# ------------------------------------------------------------------------------
# Lambda resource-based policies — allow S3 to invoke each function
# ------------------------------------------------------------------------------

resource "aws_lambda_permission" "s3_invoke_food_resizing" {
  statement_id   = "AllowS3InvokeFood"
  action         = "lambda:InvokeFunction"
  function_name  = aws_lambda_function.food_resizing.function_name
  principal      = "s3.amazonaws.com"
  source_arn     = aws_s3_bucket.private_storage.arn
  source_account = data.aws_caller_identity.current.account_id
}

resource "aws_lambda_permission" "s3_invoke_avatar_resizing" {
  statement_id   = "AllowS3InvokeAvatar"
  action         = "lambda:InvokeFunction"
  function_name  = aws_lambda_function.avatar_resizing.function_name
  principal      = "s3.amazonaws.com"
  source_arn     = aws_s3_bucket.private_storage.arn
  source_account = data.aws_caller_identity.current.account_id
}
