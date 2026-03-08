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
EOT
}

resource "aws_instance" "api" {
  ami                    = data.aws_ami.amazon_linux_2023_arm.id
  instance_type          = "t4g.micro"
  subnet_id              = aws_subnet.public.id
  vpc_security_group_ids = [aws_security_group.ec2.id]
  iam_instance_profile   = aws_iam_instance_profile.ec2.name
  user_data              = local.user_data

  key_name = var.key_name

  tags = {
    Name = "notism-api"
  }
}
