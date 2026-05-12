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

# Lambda functions were manually created in us-east-1.
# This alias is used to import and manage them in-place while the REGION
# environment variable directs the Node SDK to the correct ap-northeast-1
# S3 endpoints.  A future migration story will move the functions to
# ap-northeast-1 once the sharp layer is published there.
provider "aws" {
  alias  = "us_east_1"
  region = "us-east-1"

  default_tags {
    tags = {
      Project     = "notism"
      Environment = var.environment
      ManagedBy   = "terraform"
    }
  }
}
