# Staging environment — SCAFFOLD ONLY. Not initialised, not applied.
#
# Mirrors the production root's module wiring so the layout validates, but is not
# wired for a real apply yet. Before applying staging, the storage module's
# hard-coded global bucket names ("private-notism-storage", "public-notism-storage",
# "notism-web-prod") and the compute module's fixed resource names must be made
# per-environment (e.g. name prefix from var.environment) — otherwise a staging
# apply collides with the production resources. Tracked as follow-up.

module "vpc" {
  source = "../../modules/vpc"

  vpc_cidr           = var.vpc_cidr
  public_subnet_cidr = var.public_subnet_cidr
}
