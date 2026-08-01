using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.Extensions.Logging;

namespace EHRPlatform.Services.FileStorage.Infrastructure.Storage;

/// <summary>
/// S3 storage service for uploading, downloading, and managing documents in AWS S3.
/// Handles encryption, versioning, and lifecycle policies.
/// </summary>
public class S3StorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName;
    private readonly ILogger<S3StorageService> _logger;

    public S3StorageService(
        IAmazonS3 s3Client,
        string bucketName,
        ILogger<S3StorageService> logger)
    {
        _s3Client = s3Client ?? throw new ArgumentNullException(nameof(s3Client));
        _bucketName = bucketName ?? throw new ArgumentNullException(nameof(bucketName));
        _logger = logger;
    }

    public async Task<string> UploadAsync(
        string key,
        Stream fileStream,
        string contentType,
        bool encrypted = true,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new PutObjectRequest
            {
                BucketName = _bucketName,
                Key = key,
                InputStream = fileStream,
                ContentType = contentType,
                ServerSideEncryptionMethod = encrypted ? ServerSideEncryptionMethod.AES256 : null,
                Metadata = new Dictionary<string, string>
                {
                    ["Encrypted"] = encrypted.ToString(),
                    ["UploadedAt"] = DateTime.UtcNow.ToString("O")
                }
            };

            var response = await _s3Client.PutObjectAsync(request, cancellationToken);
            _logger.LogInformation("Document uploaded to S3: {Key}", key);
            return key;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to upload document to S3: {Key}", key);
            throw;
        }
    }

    public async Task<Stream> DownloadAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            var response = await _s3Client.GetObjectAsync(request, cancellationToken);
            _logger.LogInformation("Document downloaded from S3: {Key}", key);
            return response.ResponseStream;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download document from S3: {Key}", key);
            throw;
        }
    }

    public async Task<bool> DeleteAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new DeleteObjectRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.DeleteObjectAsync(request, cancellationToken);
            _logger.LogInformation("Document deleted from S3: {Key}", key);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete document from S3: {Key}", key);
            return false;
        }
    }

    public async Task<bool> ExistsAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var request = new GetObjectMetadataRequest
            {
                BucketName = _bucketName,
                Key = key
            };

            await _s3Client.GetObjectMetadataAsync(request, cancellationToken);
            return true;
        }
        catch (Amazon.S3.AmazonS3Exception ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking if document exists in S3: {Key}", key);
            throw;
        }
    }
}
