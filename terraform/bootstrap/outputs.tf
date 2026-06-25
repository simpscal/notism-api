output "state_bucket" {
  description = "Name of the S3 bucket holding remote state for all env roots"
  value       = aws_s3_bucket.tfstate.bucket
}

output "lock_table" {
  description = "Name of the DynamoDB table used for state locking"
  value       = aws_dynamodb_table.tflock.name
}
