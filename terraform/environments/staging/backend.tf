# Remote state for the staging env root.
#
# Authored as code; NOT initialised by this refactor. At pre-release a human
# runs `terraform init` against this backend before the first staging apply
# (see terraform/README.md). Staging is a fresh create — no state migration.
terraform {
  backend "s3" {
    bucket         = "notism-tfstate"
    key            = "env/staging/terraform.tfstate"
    region         = "ap-northeast-1"
    dynamodb_table = "notism-tflock"
    encrypt        = true
  }
}
