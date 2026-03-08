output "vpc_id" {
  description = "ID of the notism VPC"
  value       = aws_vpc.main.id
}

output "ec2_instance_id" {
  description = "ID of the notism-api EC2 instance"
  value       = aws_instance.api.id
}

output "ec2_public_ip" {
  description = "Public IP of the API (Elastic IP)"
  value       = aws_eip.api.public_ip
}

output "rds_endpoint" {
  description = "RDS PostgreSQL endpoint"
  value       = aws_db_instance.main.endpoint
}

output "rds_address" {
  description = "RDS hostname (for connection string)"
  value       = aws_db_instance.main.address
}

output "ecr_repository_url" {
  description = "ECR repository URL for notism-api image"
  value       = aws_ecr_repository.api.repository_url
}

output "connection_string_hint" {
  description = "Hint for building the PostgreSQL connection string (password is sensitive)"
  value       = "Host=${aws_db_instance.main.address};Database=notism_db;Username=notismadmin;Password=<from var.db_password>;Port=5432"
  sensitive   = true
}

output "api_url" {
  description = "API base URL (HTTP)"
  value       = "http://${aws_eip.api.public_ip}:5000"
}
