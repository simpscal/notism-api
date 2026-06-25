locals {
  instance_name         = "notism-api${var.name_suffix}"
  eip_name              = "notism-api-eip${var.name_suffix}"
  role_name             = "notism-ec2-role${var.name_suffix}"
  s3_policy_name        = "notism-ec2-s3-access${var.name_suffix}"
  instance_profile_name = "notism-ec2-profile${var.name_suffix}"
}

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
  instance_type          = var.instance_type
  subnet_id              = var.subnet_id
  vpc_security_group_ids = [var.security_group_id]
  iam_instance_profile   = aws_iam_instance_profile.ec2.name
  user_data              = local.user_data

  key_name = var.key_name

  root_block_device {
    volume_size = 16
    volume_type = "gp3"

    tags = {
      Environment = var.environment
      ManagedBy   = "terraform"
      Project     = "notism"
    }
  }

  tags = {
    Name = local.instance_name
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
# Elastic IP
# ------------------------------------------------------------------------------

resource "aws_eip" "api" {
  domain = "vpc"

  tags = {
    Name = local.eip_name
  }
}

resource "aws_eip_association" "api" {
  instance_id   = aws_instance.api.id
  allocation_id = aws_eip.api.id
}

# ------------------------------------------------------------------------------
# EC2 Role
# ------------------------------------------------------------------------------

resource "aws_iam_role" "ec2" {
  name = local.role_name

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
    Name = local.role_name
  }
}

resource "aws_iam_role_policy" "ec2_s3" {
  name = local.s3_policy_name
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
  name = local.instance_profile_name
  role = aws_iam_role.ec2.name

  tags = {
    Name = local.instance_profile_name
  }
}
