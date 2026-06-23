output "vpc_id" {
  description = "ID of the development VPC"
  value       = module.vpc.vpc_id
}

output "ec2_instance_id" {
  description = "ID of the development EC2 instance"
  value       = module.compute.instance_id
}

output "ec2_public_ip" {
  description = "Public IP of the development API (Elastic IP)."
  value       = module.compute.eip_public_ip
}
