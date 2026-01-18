# Image Resizing Flow with AWS S3 and Lambda

## Overview

This document describes the image resizing workflow that uses AWS S3 with two buckets (private and public) and AWS Lambda to automatically resize images. Images are uploaded to the private bucket first, then processed by Lambda and stored in the public bucket.

## Architecture

```
Client → API → Presigned URL → Private S3 Bucket → S3 Event → Lambda → Public S3 Bucket
```

## Components

### 1. S3 Buckets

#### Private Bucket
- **Purpose**: Receives original image uploads from clients
- **Access**: Private, requires presigned URLs for upload/download
- **Configuration**: Defined in `AwsSettings.PrivateBucketName`
- **Lifecycle**: Original images can be retained or deleted after processing

#### Public Bucket
- **Purpose**: Stores resized/processed images for public access
- **Access**: Public read access (via CloudFront CDN recommended)
- **Configuration**: Defined in `AwsSettings.PublicBucketName`
- **URL Format**: `https://{PublicBucketName}.s3.{Region}.amazonaws.com/{fileKey}`

### 2. AWS Lambda Function

#### Trigger
- **Event Source**: S3 ObjectCreated event on private bucket
- **Event Types**: `s3:ObjectCreated:*` (PUT, POST, CompleteMultipartUpload)
- **Filter**: Only process image files (e.g., `.jpg`, `.jpeg`, `.png`, `.webp`)

#### Processing Steps
1. Receive S3 event notification
2. Download original image from private bucket
3. Resize image to configured dimensions (multiple sizes if needed)
4. Optimize image (compression, format conversion)
5. Upload resized images to public bucket
6. Optionally delete original from private bucket
7. Update database with public URLs (if applicable)

### 3. API Integration

#### Upload Flow
1. Client requests presigned URL from API
2. API generates presigned PUT URL for private bucket
3. Client uploads image directly to private bucket using presigned URL
4. S3 triggers Lambda function on successful upload
5. Lambda processes and uploads to public bucket
6. Client can retrieve public URL from API (after processing completes)

## Detailed Flow

### Step 1: Client Requests Upload URL

**Process:**
- Client requests presigned upload URL from API
- API generates presigned PUT URL for private bucket
- Response includes upload URL, file key, and expiration time

**Key Points:**
- Presigned URL is time-limited
- File key follows structured format for organization
- Uses private bucket for initial upload

### Step 2: Client Uploads to Private Bucket

**Process:**
- Client performs PUT request directly to presigned URL
- Uploads image file directly to S3 (bypasses API)
- S3 stores file in private bucket

**Key Points:**
- Upload happens directly between client and S3
- No API bandwidth consumed for file transfer
- Presigned URL expires after configured time

### Step 3: S3 Triggers Lambda

**Process:**
- S3 detects new object creation in private bucket
- S3 event notification triggers Lambda function
- Event includes bucket name, object key, and metadata

**Lambda Trigger Configuration:**
- Event notification configured on private bucket
- Filters by prefix and file extensions (e.g., `.jpg`, `.png`)
- Invokes Lambda function asynchronously

### Step 4: Lambda Processes Image

**Processing Steps:**

1. **Download Original**: Lambda downloads original image from private bucket
2. **Resize Image**: Lambda resizes image to configured dimensions (multiple sizes if needed)
3. **Optimize Image**: Applies compression and format optimization
4. **Upload to Public Bucket**: Uploads resized images to public bucket with appropriate naming
5. **Cleanup**: Optionally deletes original from private bucket after successful processing

**Key Points:**
- Multiple sizes can be generated (thumbnail, small, medium, large)
- Images are optimized for web delivery
- Processing happens asynchronously

### Step 5: Retrieve Public URLs

**Process:**
- Client can retrieve public URL from API after processing completes
- Public URLs point to resized images in public bucket
- URLs follow standard S3 public URL format