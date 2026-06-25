output "instance_id" {
  description = "ID of the notism-api EC2 instance"
  value       = aws_instance.api.id
}

output "instance_arn" {
  description = "ARN of the notism-api EC2 instance"
  value       = aws_instance.api.arn
}

output "public_ip" {
  description = "Public IP of the API (Elastic IP)"
  value       = aws_eip.api.public_ip
}

output "role_arn" {
  description = "ARN of the EC2 IAM role"
  value       = aws_iam_role.ec2.arn
}
