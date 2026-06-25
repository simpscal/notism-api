output "function_arn" {
  description = "ARN of the image-resizing Lambda function"
  value       = aws_lambda_function.image_resizing.arn
}

output "function_name" {
  description = "Name of the image-resizing Lambda function"
  value       = aws_lambda_function.image_resizing.function_name
}

output "invoke_permission_id" {
  description = "ID of the Lambda resource-based permission allowing S3 invocation (dependency handle for the bucket notification)"
  value       = aws_lambda_permission.s3_invoke_image_resizing.id
}
