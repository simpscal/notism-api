provider "aws" {
  region = var.aws_region

  default_tags {
    tags = {
      Project     = "notism"
      Environment = var.environment
      ManagedBy   = "terraform"
    }
  }
}
