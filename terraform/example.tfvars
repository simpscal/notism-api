# Copy to terraform.tfvars and set values. Do not commit terraform.tfvars.

aws_region  = "us-east-1"
environment = "prod"
key_name    = "notism-api"

# Set to true to provision a managed RDS instance instead of running Postgres on EC2
use_rds     = false
# db_password = "your-rds-password"  # required only when use_rds = true