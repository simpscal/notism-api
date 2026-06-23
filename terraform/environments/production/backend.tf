# Remote state — S3 backend with DynamoDB state locking.
# Bucket + lock table are bootstrapped manually at cutover (see PR cutover notes);
# do NOT run `terraform init` against this backend as part of the refactor.
terraform {
  backend "s3" {
    bucket         = "notism-terraform-state"
    key            = "production/terraform.tfstate"
    region         = "ap-northeast-1"
    dynamodb_table = "notism-terraform-locks"
    encrypt        = true
  }
}
