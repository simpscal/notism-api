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
