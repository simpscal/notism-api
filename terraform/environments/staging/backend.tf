# Remote state — S3 backend with DynamoDB state locking.
# Scaffold only — not initialised or applied. Bucket + lock table are
# bootstrapped manually at cutover (see PR cutover notes).
terraform {
  backend "s3" {
    bucket         = "notism-terraform-state"
    key            = "staging/terraform.tfstate"
    region         = "ap-northeast-1"
    dynamodb_table = "notism-terraform-locks"
    encrypt        = true
  }
}
