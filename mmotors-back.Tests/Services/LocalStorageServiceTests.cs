using Microsoft.AspNetCore.Http;
using mmotors_back.Features.Shared.Services;

namespace mmotors_back.Tests.Services;

public class LocalStorageServiceTests
{
    #region UploadFileAsync
    [Fact]
    public async Task UploadFileAsync_ShouldCreateFileAndReturnKey_WhenFileIsValid()
    {
        // Arrange
        var storagePath = CreateStoragePath();
        var service = new LocalStorageService(storagePath);
        var file = CreateFormFile("contract.pdf", "application/pdf", "file-content");

        try
        {
            // Act
            var result = await service.UploadFileAsync(file, "documents");

            // Assert
            var expectedPath = Path.Combine(storagePath, "documents", result.Key);
            Assert.Equal(expectedPath, result.Url);
            Assert.True(File.Exists(expectedPath));
            Assert.EndsWith(".pdf", result.Key);
        }
        finally
        {
            Directory.Delete(storagePath, true);
        }
    }

    [Fact]
    public async Task UploadFileAsync_ShouldCreateSubfolder_WhenSubfolderDoesNotExist()
    {
        // Arrange
        var storagePath = CreateStoragePath();
        var service = new LocalStorageService(storagePath);
        var file = CreateFormFile("image.jpg", "image/jpeg", "image-content");

        try
        {
            // Act
            var result = await service.UploadFileAsync(file, "vehicles");

            // Assert
            Assert.True(Directory.Exists(Path.Combine(storagePath, "vehicles")));
            Assert.True(File.Exists(Path.Combine(storagePath, "vehicles", result.Key)));
        }
        finally
        {
            Directory.Delete(storagePath, true);
        }
    }
    #endregion

    #region DeleteFileAsync
    [Fact]
    public async Task DeleteFileAsync_ShouldDeleteFile_WhenFileExists()
    {
        // Arrange
        var storagePath = CreateStoragePath();
        Directory.CreateDirectory(Path.Combine(storagePath, "vehicles"));
        var filePath = Path.Combine(storagePath, "vehicles", "vehicle.jpg");
        await File.WriteAllTextAsync(filePath, "content");
        var service = new LocalStorageService(storagePath);

        try
        {
            // Act
            await service.DeleteFileAsync("vehicle.jpg", "vehicles");

            // Assert
            Assert.False(File.Exists(filePath));
        }
        finally
        {
            Directory.Delete(storagePath, true);
        }
    }

    [Fact]
    public async Task DeleteFileAsync_ShouldDoNothing_WhenFileDoesNotExist()
    {
        // Arrange
        var storagePath = CreateStoragePath();
        var service = new LocalStorageService(storagePath);

        try
        {
            // Act
            await service.DeleteFileAsync("missing.jpg", "vehicles");

            // Assert
            Assert.True(Directory.Exists(storagePath));
        }
        finally
        {
            Directory.Delete(storagePath, true);
        }
    }
    #endregion

    #region GetFileAsync
    [Fact]
    public async Task GetFileAsync_ShouldReturnReadableStream_WhenFileExists()
    {
        // Arrange
        var storagePath = CreateStoragePath();
        Directory.CreateDirectory(Path.Combine(storagePath, "documents"));
        var filePath = Path.Combine(storagePath, "documents", "contract.pdf");
        await File.WriteAllTextAsync(filePath, "content");
        var service = new LocalStorageService(storagePath);

        try
        {
            // Act
            await using var result = await service.GetFileAsync("contract.pdf", "documents");
            using var reader = new StreamReader(result);
            var content = await reader.ReadToEndAsync();

            // Assert
            Assert.Equal("content", content);
        }
        finally
        {
            Directory.Delete(storagePath, true);
        }
    }

    [Fact]
    public async Task GetFileAsync_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var storagePath = CreateStoragePath();
        var service = new LocalStorageService(storagePath);

        try
        {
            // Act
            var action = () => service.GetFileAsync("missing.pdf", "documents");

            // Assert
            await Assert.ThrowsAsync<FileNotFoundException>(action);
        }
        finally
        {
            Directory.Delete(storagePath, true);
        }
    }
    #endregion

    #region GetFileUrl
    [Fact]
    public async Task GetFileUrl_ShouldReturnPublicUrl_WhenFileExists()
    {
        // Arrange
        var storagePath = CreateStoragePath();
        Directory.CreateDirectory(Path.Combine(storagePath, "documents"));
        await File.WriteAllTextAsync(Path.Combine(storagePath, "documents", "contract.pdf"), "content");
        var service = new LocalStorageService(storagePath);

        try
        {
            // Act
            var result = service.GetFileUrl("contract.pdf", "documents");

            // Assert
            Assert.Equal("http://localhost:5049/uploads/documents/contract.pdf", result);
        }
        finally
        {
            Directory.Delete(storagePath, true);
        }
    }

    [Fact]
    public void GetFileUrl_ShouldThrowFileNotFoundException_WhenFileDoesNotExist()
    {
        // Arrange
        var storagePath = CreateStoragePath();
        var service = new LocalStorageService(storagePath);

        try
        {
            // Act
            var action = () => service.GetFileUrl("missing.pdf", "documents");

            // Assert
            Assert.Throws<FileNotFoundException>(action);
        }
        finally
        {
            Directory.Delete(storagePath, true);
        }
    }
    #endregion

    private static string CreateStoragePath()
    {
        return Path.Combine(Path.GetTempPath(), "mmotors-local-storage-tests", Guid.NewGuid().ToString());
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