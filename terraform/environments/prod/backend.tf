# Remote state for the prod env root.
#
# Authored as code; NOT initialised by this refactor. At pre-release a human
# runs `terraform init -migrate-state` once to move the existing local/flat
# state into this backend (see terraform/README.md).
terraform {
  backend "s3" {
    bucket         = "notism-tfstate"
    key            = "env/prod/terraform.tfstate"
    region         = "ap-northeast-1"
    dynamodb_table = "notism-tflock"
    encrypt        = true
  }
}
