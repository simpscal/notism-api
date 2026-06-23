data "aws_caller_identity" "current" {}

# ------------------------------------------------------------------------------
# EC2 instance (notism-api)
# ------------------------------------------------------------------------------

data "aws_ami" "amazon_linux_2023_arm" {
  most_recent = true
  owners      = ["amazon"]

  filter {
    name   = "name"
    values = ["al2023-ami-*-kernel-6.1-arm64"]
  }

  filter {
    name   = "state"
    values = ["available"]
  }
}

locals {
  user_data = <<-EOT
#!/bin/bash
set -e
yum update -y
yum install -y docker
systemctl enable docker
systemctl start docker

mkdir -p /usr/local/lib/docker/cli-plugins
curl -SL https://github.com/docker/compose/releases/latest/download/docker-compose-linux-aarch64 \
  -o /usr/local/lib/docker/cli-plugins/docker-compose
chmod +x /usr/local/lib/docker/cli-plugins/docker-compose
EOT
}

resource "aws_instance" "api" {
  ami                    = data.aws_ami.amazon_linux_2023_arm.id
  instance_type          = "t4g.micro"
  subnet_id              = var.public_subnet_id
  vpc_security_group_ids = [aws_security_group.ec2.id]
  iam_instance_profile   = aws_iam_instance_profile.ec2.name
  user_data              = local.user_data

  key_name = var.key_name

  root_block_device {
    volume_size = 16
    volume_type = "gp3"

    tags = {
      Environment = "prod"
      ManagedBy   = "terraform"
      Project     = "notism"
    }
  }

  tags = {
    Name = "notism-api"
  }

  # Prevent AMI version bumps from triggering an instance replacement.
  # The data source uses most_recent = true so a newly published AMI would
  # otherwise force a destroy-then-create on every plan.  AMI updates are
  # applied intentionally by removing this block and running a targeted apply.
  lifecycle {
    ignore_changes = [ami]
  }
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
          var.private_storage_arn,
          "${var.private_storage_arn}/*",
          var.public_storage_arn,
          "${var.public_storage_arn}/*",
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

# ------------------------------------------------------------------------------
# Elastic IP
# ------------------------------------------------------------------------------

resource "aws_eip" "api" {
  domain = "vpc"

  tags = {
    Name = "notism-api-eip"
  }
}

resource "aws_eip_association" "api" {
  instance_id   = aws_instance.api.id
  allocation_id = aws_eip.api.id
}

# ------------------------------------------------------------------------------
# Security Group
# ------------------------------------------------------------------------------

resource "aws_security_group" "ec2" {
  name        = "notism-ec2-sg"
  description = "Notism EC2 - API instance"
  vpc_id      = var.vpc_id

  ingress {
    description = "SSH"
    from_port   = 22
    to_port     = 22
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTP"
    from_port   = 80
    to_port     = 80
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  ingress {
    description = "HTTPS"
    from_port   = 443
    to_port     = 443
    protocol    = "tcp"
    cidr_blocks = ["0.0.0.0/0"]
  }

  egress {
    from_port   = 0
    to_port     = 0
    protocol    = "-1"
    cidr_blocks = ["0.0.0.0/0"]
  }

  tags = {
    Name = "notism-ec2-sg"
  }
}

# ------------------------------------------------------------------------------
# ECR
# ------------------------------------------------------------------------------

resource "aws_ecr_repository" "api" {
  name                 = "notism-api"
  image_tag_mutability = "MUTABLE"

  image_scanning_configuration {
    scan_on_push = true
  }

  tags = {
    Name = "notism-api"
  }
}

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

# ------------------------------------------------------------------------------
# CloudFront - Managed Cache Policy
# ------------------------------------------------------------------------------

data "aws_cloudfront_cache_policy" "caching_optimized" {
  name = "Managed-CachingOptimized"
}

locals {
  cloudfront_web_prod_origin_id = var.web_prod_bucket_regional_domain_name
}

# ------------------------------------------------------------------------------
# Origin Access Controls
# ------------------------------------------------------------------------------

resource "aws_cloudfront_origin_access_control" "web_prod" {
  name                              = "oac-notism-web-prod.s3.${var.aws_region}.amazonaws.com-mhulcbx9s1m"
  description                       = "Created by CloudFront"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

# ------------------------------------------------------------------------------
# CloudFront Distribution - Prod (notism-web-prod)
# ------------------------------------------------------------------------------

resource "aws_cloudfront_distribution" "web_prod" {
  enabled             = true
  is_ipv6_enabled     = true
  http_version        = "http2"
  default_root_object = "index.html"
  price_class         = "PriceClass_All"
  staging             = false

  origin {
    domain_name              = var.web_prod_bucket_regional_domain_name
    origin_id                = local.cloudfront_web_prod_origin_id
    origin_access_control_id = aws_cloudfront_origin_access_control.web_prod.id
    connection_attempts      = 3
    connection_timeout       = 10
  }

  default_cache_behavior {
    target_origin_id       = local.cloudfront_web_prod_origin_id
    viewer_protocol_policy = "redirect-to-https"
    compress               = true
    cache_policy_id        = data.aws_cloudfront_cache_policy.caching_optimized.id

    allowed_methods = ["GET", "HEAD"]
    cached_methods  = ["GET", "HEAD"]
  }

  custom_error_response {
    error_code            = 403
    response_code         = 200
    response_page_path    = "/index.html"
    error_caching_min_ttl = 10
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    cloudfront_default_certificate = true
    minimum_protocol_version       = "TLSv1"
  }

  tags = {
    Name = "notism-web-prod"
  }
}
