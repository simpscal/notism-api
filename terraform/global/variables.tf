variable "aws_region" {
  description = "AWS region"
  type        = string
  default     = "ap-northeast-1"
}

variable "environment" {
  description = "Environment tag for cross-repo shared IAM"
  type        = string
  default     = "global"
}

# The api_deploy role's ec2:StartInstances policy is scoped to the API EC2
# instance, which lives in a SEPARATE state (environments/production). Terraform
# cannot reference another state's resource directly without a circular
# dependency, so the instance id is passed in here. The default is the known
# live id; at cutover, source it from `terraform output ec2_instance_id` in
# environments/production and pass it via -var or terraform.tfvars.
variable "api_instance_id" {
  description = "ID of the notism-api EC2 instance (sourced from the production state at cutover)"
  type        = string
  default     = "i-0637aed64215bf80f"
}
