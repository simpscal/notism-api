import { S3Client, GetObjectCommand, PutObjectCommand } from '@aws-sdk/client-s3';
import sharp from 'sharp';

const s3Client = new S3Client({ region: process.env.AWS_REGION || 'us-east-1' });

// RESIZE_JOBS maps a source key prefix (first path segment + "/") to the variants
// to produce. Each variant replaces the source key's first segment with
// outputPrefix and resizes to width x height, written to DESTINATION_BUCKET.
//   { "avatar/": [{ outputPrefix, width, height }], "food/": [ ... ] }
const RESIZE_JOBS = JSON.parse(process.env.RESIZE_JOBS || '{}');
const DESTINATION_BUCKET = process.env.DESTINATION_BUCKET;

export const handler = async (event) => {
    const s3Record = event.Records[0].s3;
    const sourceBucket = s3Record.bucket.name;
    const sourceKey = decodeURIComponent(s3Record.object.key.replace(/\+/g, ' '));

    const firstSlash = sourceKey.indexOf('/');
    const sourcePrefix = firstSlash >= 0 ? sourceKey.substring(0, firstSlash + 1) : '';
    const jobs = RESIZE_JOBS[sourcePrefix];

    if (!jobs || jobs.length === 0) {
        console.log(`No resize jobs for prefix "${sourcePrefix}" (key ${sourceKey}); skipping.`);
        return { statusCode: 200, body: JSON.stringify({ skipped: true, sourceKey }) };
    }

    try {
        const getObjectResponse = await s3Client.send(
            new GetObjectCommand({ Bucket: sourceBucket, Key: sourceKey }),
        );
        const imageBuffer = await streamToBuffer(getObjectResponse.Body);
        const format = (await sharp(imageBuffer).metadata()).format;
        const rest = firstSlash >= 0 ? sourceKey.substring(firstSlash + 1) : sourceKey;

        const results = [];
        for (const job of jobs) {
            const resizedBuffer = await resize(imageBuffer, format, job.width, job.height);
            const destinationKey = `${job.outputPrefix}/${rest}`;

            await s3Client.send(
                new PutObjectCommand({
                    Bucket: DESTINATION_BUCKET,
                    Key: destinationKey,
                    Body: resizedBuffer,
                    ContentType: getContentType(format),
                }),
            );

            console.log(`Wrote ${destinationKey} (${job.width}x${job.height}) to ${DESTINATION_BUCKET}`);
            results.push({ destinationKey, width: job.width, height: job.height, size: resizedBuffer.length });
        }

        return { statusCode: 200, body: JSON.stringify({ success: true, sourceKey, results }) };
    } catch (error) {
        console.error('Error processing image:', error);
        return { statusCode: 500, body: JSON.stringify({ success: false, error: error.message }) };
    }
};

async function resize(imageBuffer, format, width, height) {
    const instance = sharp(imageBuffer).resize({ width, height });

    if (format === 'png') {
        return instance.png({ quality: 100 }).toBuffer();
    }
    if (format === 'webp') {
        return instance.webp({ quality: 100 }).toBuffer();
    }
    return instance.jpeg({ quality: 100 }).toBuffer();
}

async function streamToBuffer(stream) {
    const chunks = [];
    for await (const chunk of stream) {
        chunks.push(chunk);
    }
    return Buffer.concat(chunks);
}

function getContentType(format) {
    const contentTypes = {
        jpeg: 'image/jpeg',
        jpg: 'image/jpeg',
        png: 'image/png',
        webp: 'image/webp',
        gif: 'image/gif',
        svg: 'image/svg+xml',
    };
    return contentTypes[format?.toLowerCase()] || 'image/jpeg';
}
