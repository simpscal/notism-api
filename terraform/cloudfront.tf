# ------------------------------------------------------------------------------
# CloudFront - Managed Cache Policy
# ------------------------------------------------------------------------------

data "aws_cloudfront_cache_policy" "caching_optimized" {
  name = "Managed-CachingOptimized"
}

locals {
  cloudfront_web_prod_origin_id = aws_s3_bucket.web_prod.bucket_regional_domain_name
}

# ------------------------------------------------------------------------------
# Origin Access Controls
# ------------------------------------------------------------------------------

resource "aws_cloudfront_origin_access_control" "web_prod" {
  name                              = "oac-notism-web-prod.s3.${var.aws_region}.amazonaws.com-mhulcbx9s1m"
  description                       = "Created by CloudFront"
  origin_access_control_origin_type = "s3"
  signing_behavior                  = "always"
  signing_protocol                  = "sigv4"
}

# ------------------------------------------------------------------------------
# CloudFront Distribution - Prod (notism-web-prod)
# ------------------------------------------------------------------------------

resource "aws_cloudfront_distribution" "web_prod" {
  enabled             = true
  is_ipv6_enabled     = true
  http_version        = "http2"
  default_root_object = "index.html"
  price_class         = "PriceClass_All"
  staging             = false

  origin {
    domain_name              = aws_s3_bucket.web_prod.bucket_regional_domain_name
    origin_id                = local.cloudfront_web_prod_origin_id
    origin_access_control_id = aws_cloudfront_origin_access_control.web_prod.id
    connection_attempts      = 3
    connection_timeout       = 10
  }

  default_cache_behavior {
    target_origin_id       = local.cloudfront_web_prod_origin_id
    viewer_protocol_policy = "redirect-to-https"
    compress               = true
    cache_policy_id        = data.aws_cloudfront_cache_policy.caching_optimized.id

    allowed_methods = ["GET", "HEAD"]
    cached_methods  = ["GET", "HEAD"]
  }

  custom_error_response {
    error_code            = 403
    response_code         = 200
    response_page_path    = "/index.html"
    error_caching_min_ttl = 10
  }

  restrictions {
    geo_restriction {
      restriction_type = "none"
    }
  }

  viewer_certificate {
    cloudfront_default_certificate = true
    minimum_protocol_version       = "TLSv1"
  }

  tags = {
    Name = "notism-web-prod"
  }
}
