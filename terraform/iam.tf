# ------------------------------------------------------------------------------
# Lambda — Image Resizing Execution Role
# ------------------------------------------------------------------------------

resource "aws_iam_role" "lambda_image_resizing" {
  name        = "notism-image-resizing-role"
  description = "Execution role for all Notism image resizing Lambdas (avatar, food, food-detail)"

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
          "${aws_s3_bucket.public_storage.arn}/*",
          "${aws_s3_bucket.private_storage.arn}/*",
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "lambda_image_resizing_basic_execution" {
  role       = aws_iam_role.lambda_image_resizing.name
  policy_arn = "arn:aws:iam::aws:policy/service-role/AWSLambdaBasicExecutionRole"
}

# ------------------------------------------------------------------------------
# EC2 Role
# ------------------------------------------------------------------------------

resource "aws_iam_role" "ec2" {
  name = "notism-ec2-role"

  assume_role_policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Principal = {
          Service = "ec2.amazonaws.com"
        }
        Action = "sts:AssumeRole"
      }
    ]
  })

  tags = {
    Name = "notism-ec2-role"
  }
}

resource "aws_iam_role_policy" "ec2_s3" {
  name = "notism-ec2-s3-access"
  role = aws_iam_role.ec2.id

  policy = jsonencode({
    Version = "2012-10-17"
    Statement = [
      {
        Effect = "Allow"
        Action = [
          "s3:GetObject",
          "s3:PutObject",
          "s3:DeleteObject",
          "s3:ListBucket",
        ]
        Resource = [
          aws_s3_bucket.private_storage.arn,
          "${aws_s3_bucket.private_storage.arn}/*",
          aws_s3_bucket.public_storage.arn,
          "${aws_s3_bucket.public_storage.arn}/*",
        ]
      }
    ]
  })
}

resource "aws_iam_role_policy_attachment" "ec2_ecr" {
  role       = aws_iam_role.ec2.name
  policy_arn = "arn:aws:iam::aws:policy/AmazonEC2ContainerRegistryReadOnly"
}

resource "aws_iam_instance_profile" "ec2" {
  name = "notism-ec2-profile"
  role = aws_iam_role.ec2.name

  tags = {
    Name = "notism-ec2-profile"
  }
}