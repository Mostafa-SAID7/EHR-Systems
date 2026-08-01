namespace EHRPlatform.Services.FileStorage.Infrastructure.Services;

using EHRPlatform.Services.FileStorage.Application.Services;
using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;

/// <summary>
/// AWS S3 storage service implementation.
/// Handles all S3 operations: upload, download, delete, presigned URLs.
/// </summary>
public class S3StorageService : IS3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly ILogger<S3StorageService> _logger;
    private readonly IConfiguration _configuration;

    public S3StorageService(
        IAmazonS3 s3Client,
        ILogger<S3StorageService> logger,
        IConfiguration configuration)
    {
        _s3Client = s3Client;
        _logger = logger;
        _configuration = configuration;
    }

    public async Task<string> UploadFileAsync(string bucket, string key, Stream fileStream, string contentType, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Uploading file to S3: s3://{Bucket}/{Key}", bucket, key);

        try
        {
            var putRequest = new PutObjectRequest
            {
                BucketName = bucket,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = ServerSideEncryptionMethod.AES256,
                StorageClass = S3StorageClass.Standard
            };

            var response = await _s3Client.PutObjectAsync(putRequest, cancellationToken);

            _logger.LogInformation("File uploaded successfully. ETag: {ETag}", response.ETag);
            return response.ETag;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file to S3");
            throw;
        }
    }

    public async Task<Stream> DownloadFileAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Downloading file from S3: s3://{Bucket}/{Key}", bucket, key);

        try
        {
            var response = await _s3Client.GetObjectAsync(bucket, key, cancellationToken);
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error downloading file from S3");
            throw;
        }
    }

    public async Task DeleteFileAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting file from S3: s3://{Bucket}/{Key}", bucket, key);

        try
        {
            await _s3Client.DeleteObjectAsync(bucket, key, cancellationToken);
            _logger.LogInformation("File deleted successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file from S3");
            throw;
        }
    }

    public async Task<string> GeneratePresignedUrlAsync(string bucket, string key, TimeSpan expiration, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Generating presigned URL for s3://{Bucket}/{Key}", bucket, key);

        try
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucket,
                Key = key,
                Expires = DateTime.UtcNow.Add(expiration),
                Verb = HttpVerb.GET
            };

            var url = _s3Client.GetPreSignedURL(request);
            _logger.LogInformation("Presigned URL generated");
            return url;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating presigned URL");
            throw;
        }
    }

    public async Task<bool> FileExistsAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        _logger.LogDebug("Checking if file exists: s3://{Bucket}/{Key}", bucket, key);

        try
        {
            await _s3Client.GetObjectMetadataAsync(bucket, key, cancellationToken);
            return true;
        }
        catch (AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking file existence");
            throw;
        }
    }

    public async Task<S3FileMetadata> GetFileMetadataAsync(string bucket, string key, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting file metadata: s3://{Bucket}/{Key}", bucket, key);

        try
        {
            var response = await _s3Client.GetObjectMetadataAsync(bucket, key, cancellationToken);

            return new S3FileMetadata
            {
                Key = key,
                ContentLength = response.ContentLength,
                ContentType = response.Headers.ContentType,
                LastModified = response.LastModified,
                ETag = response.ETag
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting file metadata");
            throw;
        }
    }
}
