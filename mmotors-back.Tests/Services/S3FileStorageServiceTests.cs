using Amazon.S3;
using Amazon.S3.Model;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using mmotors_back.Features.Shared.Services;
using Moq;

namespace mmotors_back.Tests.Services;

public class S3FileStorageServiceTests
{
    #region Constructor
    [Fact]
    public void Constructor_ShouldThrowInvalidOperationException_WhenBucketNameIsMissing()
    {
        // Arrange
        var s3Mock = new Mock<IAmazonS3>();
        var configuration = new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string?>()).Build();

        // Act
        var action = () => new S3FileStorageService(s3Mock.Object, configuration);

        // Assert
        Assert.Throws<InvalidOperationException>(action);
    }
    #endregion

    #region UploadFileAsync
    [Fact]
    public async Task UploadFileAsync_ShouldUploadToProvidedSubfolderAndReturnUrl_WhenFileIsValid()
    {
        // Arrange
        PutObjectRequest? capturedRequest = null;
        var s3Mock = new Mock<IAmazonS3>();
        s3Mock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .Callback<PutObjectRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new PutObjectResponse());
        var service = CreateService(s3Mock.Object);
        var file = CreateFormFile("contract.pdf", "application/pdf", "contract-content");

        // Act
        var result = await service.UploadFileAsync(file, "01_applications");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal("mmotors-bucket", capturedRequest!.BucketName);
        Assert.StartsWith("01_applications/", capturedRequest.Key);
        Assert.EndsWith(".pdf", capturedRequest.Key);
        Assert.Equal("application/pdf", capturedRequest.ContentType);
        Assert.Equal($"https://mmotors-bucket.s3.amazonaws.com/{capturedRequest.Key}", result.Url);
        Assert.Equal(capturedRequest.Key, result.Key);
    }

    [Fact]
    public async Task UploadFileAsync_ShouldUseDocumentsPrefix_WhenSubfolderIsEmpty()
    {
        // Arrange
        PutObjectRequest? capturedRequest = null;
        var s3Mock = new Mock<IAmazonS3>();
        s3Mock.Setup(s3 => s3.PutObjectAsync(It.IsAny<PutObjectRequest>(), default))
            .Callback<PutObjectRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new PutObjectResponse());
        var service = CreateService(s3Mock.Object);
        var file = CreateFormFile("identity.png", "image/png", "identity-content");

        // Act
        var result = await service.UploadFileAsync(file);

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.StartsWith("documents/", capturedRequest!.Key);
        Assert.EndsWith(".png", result.Key);
    }
    #endregion

    #region DeleteFileAsync
    [Fact]
    public async Task DeleteFileAsync_ShouldSendDeleteRequestWithBucketAndKey()
    {
        // Arrange
        DeleteObjectRequest? capturedRequest = null;
        var s3Mock = new Mock<IAmazonS3>();
        s3Mock.Setup(s3 => s3.DeleteObjectAsync(It.IsAny<DeleteObjectRequest>(), default))
            .Callback<DeleteObjectRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new DeleteObjectResponse());
        var service = CreateService(s3Mock.Object);

        // Act
        await service.DeleteFileAsync("documents/contract.pdf", "ignored-subfolder");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal("mmotors-bucket", capturedRequest!.BucketName);
        Assert.Equal("documents/contract.pdf", capturedRequest.Key);
    }
    #endregion

    #region GetFileAsync
    [Fact]
    public async Task GetFileAsync_ShouldReturnResponseStream_WhenObjectExists()
    {
        // Arrange
        GetObjectRequest? capturedRequest = null;
        var responseStream = new MemoryStream(System.Text.Encoding.UTF8.GetBytes("stored-content"));
        var s3Mock = new Mock<IAmazonS3>();
        s3Mock.Setup(s3 => s3.GetObjectAsync(It.IsAny<GetObjectRequest>(), default))
            .Callback<GetObjectRequest, CancellationToken>((request, _) => capturedRequest = request)
            .ReturnsAsync(new GetObjectResponse { ResponseStream = responseStream });
        var service = CreateService(s3Mock.Object);

        // Act
        await using var result = await service.GetFileAsync("documents/contract.pdf", "ignored-subfolder");
        using var reader = new StreamReader(result);
        var content = await reader.ReadToEndAsync();

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal("mmotors-bucket", capturedRequest!.BucketName);
        Assert.Equal("documents/contract.pdf", capturedRequest.Key);
        Assert.Equal("stored-content", content);
    }
    #endregion

    #region GetFileUrl
    [Fact]
    public void GetFileUrl_ShouldReturnPreSignedUrl_WhenKeyIsValid()
    {
        // Arrange
        GetPreSignedUrlRequest? capturedRequest = null;
        var s3Mock = new Mock<IAmazonS3>();
        s3Mock.Setup(s3 => s3.GetPreSignedURL(It.IsAny<GetPreSignedUrlRequest>()))
            .Callback<GetPreSignedUrlRequest>(request => capturedRequest = request)
            .Returns("https://signed.example.com/file");
        var service = CreateService(s3Mock.Object);

        // Act
        var result = service.GetFileUrl("documents/contract.pdf", "ignored-subfolder");

        // Assert
        Assert.NotNull(capturedRequest);
        Assert.Equal("mmotors-bucket", capturedRequest!.BucketName);
        Assert.Equal("documents/contract.pdf", capturedRequest.Key);
        Assert.True(capturedRequest.Expires > DateTime.UtcNow);
        Assert.Equal("https://signed.example.com/file", result);
    }
    #endregion

    private static S3FileStorageService CreateService(IAmazonS3 s3)
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Storage:S3:BucketName"] = "mmotors-bucket"
            })
            .Build();

        return new S3FileStorageService(s3, configuration);
    }

    private static IFormFile CreateFormFile(string fileName, string contentType, string content)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(content);
        var stream = new MemoryStream(bytes);

        return new FormFile(stream, 0, bytes.Length, "file", fileName)
        {
            Headers = new HeaderDictionary(),
            ContentType = contentType
        };
    }
}