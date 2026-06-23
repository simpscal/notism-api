# notism-image-resizing

Single, config-driven Lambda behind the S3 image-resizing pipeline. On
`s3:ObjectCreated` it reads `RESIZE_JOBS` (JSON env, keyed by source key prefix),
resizes the upload to every configured variant with `sharp`, and writes each to
`DESTINATION_BUCKET`. The source key's first path segment is replaced with the
variant's `outputPrefix`; the rest of the key is preserved.

Config lives in Terraform (`terraform/lambda.tf` → `aws_lambda_function.image_resizing`):

| Source prefix | Variants (outputPrefix · WxH) |
|---------------|-------------------------------|
| `avatar/`     | `avatar` 200×200              |
| `food/`       | `food` 400×400 · `food-detail` 800×800 |

Source prefixes mirror the app's real upload folders — see
`GenerateUploadUrlHandler` / `StorageTypeConstants`. Output prefixes match what
`S3StorageService.GetPublicUrl` expects, so served URLs are unchanged.

## Build the deployment package

`sharp` ships prebuilt native binaries — install them for the Lambda target
(`linux-arm64`), then zip the function with its `node_modules`:

```bash
cd terraform/lambda-src/image-resizing
npm ci --omit=dev --cpu=arm64 --os=linux
zip -r ../../lambda-packages/notism-image-resizing.zip index.mjs node_modules package.json
```

Terraform references `./lambda-packages/notism-image-resizing.zip` with
`ignore_changes = [filename, source_code_hash]`, so code is deployed out of band
(build the zip, then `aws lambda update-function-code` or `terraform apply` on a
fresh function). Ship the package **before/with** the `apply` that creates the
function, or the first invoke has no code.
