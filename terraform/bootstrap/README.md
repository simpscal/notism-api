# bootstrap

Provisions the remote-state backend shared by every env root:

- S3 bucket `notism-tfstate` — versioned, SSE-S3 encrypted, public access blocked, `prevent_destroy`.
- DynamoDB table `notism-tflock` — PK `LockID`, `PAY_PER_REQUEST`.

This root keeps **local state** on purpose (it creates the very backend the env
roots consume) and is applied **once, by hand, before any env root**.

```bash
cd terraform/bootstrap
terraform init          # local backend, no -backend=false needed at apply time
terraform apply
```

After this succeeds, the `environments/prod` and `environments/staging` roots
can `terraform init` against the S3 backend (see each root's `backend.tf`).

Do not destroy this root — the state bucket carries `prevent_destroy`.
