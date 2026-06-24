# ------------------------------------------------------------------------------
# EC2
# ------------------------------------------------------------------------------

output "instance_id" {
  description = "ID of the notism-api EC2 instance"
  value       = aws_instance.api.id
}

output "eip_public_ip" {
  description = "Public IP of the API (Elastic IP)."
  value       = aws_eip.api.public_ip
}

output "security_group_id" {
  description = "ID of the EC2 security group"
  value       = aws_security_group.ec2.id
}

output "ec2_role_arn" {
  description = "ARN of the EC2 instance IAM role"
  value       = aws_iam_role.ec2.arn
}

# ------------------------------------------------------------------------------
# ECR
# ------------------------------------------------------------------------------

output "ecr_repository_url" {
  description = "ECR repository URL for notism-api image"
  value       = aws_ecr_repository.api.repository_url
}

# ------------------------------------------------------------------------------
# Lambda
# ------------------------------------------------------------------------------

output "image_resizing_arn" {
  description = "ARN of the image-resizing Lambda function"
  value       = aws_lambda_function.image_resizing.arn
}

output "image_resizing_function_name" {
  description = "Name of the image-resizing Lambda function"
  value       = aws_lambda_function.image_resizing.function_name
}

output "lambda_image_resizing_role_arn" {
  description = "ARN of the image-resizing Lambda execution role"
  value       = aws_iam_role.lambda_image_resizing.arn
}
