using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.Documents.Controllers;
using mmotors_back.Features.Documents.Dtos;
using mmotors_back.Features.Documents.Interfaces;
using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Models;
using Moq;

namespace mmotors_back.Tests.Features.Documents;

public class DocumentsControllerTests
{
    #region UploadDocument
    [Fact]
    public async Task UploadDocument_ShouldReturnOkAndUpdateMetadata_WhenDocumentIsUploaded()
    {
        // Arrange
        var document = new DocumentDto { Id = 5, FileName = "contract", Type = DocumentType.COMMON_APPLICATION };
        var formFileMock = CreateFormFileMock("contract.pdf", "application/pdf", 2048);
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.UploadFileAsync(formFileMock.Object, "01_applications")).ReturnsAsync(("https://cdn.example.com/contract.pdf", "applications/contract.pdf"));
        var repositoryMock = new Mock<IDocumentRepository>();
        repositoryMock.Setup(repository => repository.GetDocumentByIdAsync(5)).ReturnsAsync(document);
        repositoryMock.Setup(repository => repository.UpdateDocumentAsync(It.Is<DocumentDto>(updatedDocument => updatedDocument.Url == "https://cdn.example.com/contract.pdf" && updatedDocument.Key == "applications/contract.pdf" && updatedDocument.Extension == ".pdf" && updatedDocument.MimeType == "application/pdf"))).Returns(Task.CompletedTask);
        var controller = new DocumentsController(storageMock.Object, repositoryMock.Object);

        // Act
        var result = await controller.UploadDocument(5, formFileMock.Object);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var returnedDocument = Assert.IsType<DocumentDto>(okResult.Value);
        Assert.Equal("applications/contract.pdf", returnedDocument.Key);
    }

    [Fact]
    public async Task UploadDocument_ShouldReturnBadRequest_WhenDocumentIsNull()
    {
        // Arrange
        var storageMock = new Mock<IStorageService>();
        var repositoryMock = new Mock<IDocumentRepository>();
        var controller = new DocumentsController(storageMock.Object, repositoryMock.Object);

        // Act
        var result = await controller.UploadDocument(5, null);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadDocument_ShouldReturnBadRequest_WhenDocumentIsEmpty()
    {
        // Arrange
        var formFileMock = CreateFormFileMock("empty.pdf", "application/pdf", 0);
        var storageMock = new Mock<IStorageService>();
        var repositoryMock = new Mock<IDocumentRepository>();
        var controller = new DocumentsController(storageMock.Object, repositoryMock.Object);

        // Act
        var result = await controller.UploadDocument(5, formFileMock.Object);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task UploadDocument_ShouldDeleteExistingFileBeforeUploading_WhenDocumentAlreadyHasAKey()
    {
        // Arrange
        var document = new DocumentDto { Id = 7, FileName = "passport", Type = DocumentType.COMMON_APPLICATION, Key = "applications/old-passport.pdf" };
        var formFileMock = CreateFormFileMock("passport.pdf", "application/pdf", 1024);
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.DeleteFileAsync("applications/old-passport.pdf", "01_applications")).Returns(Task.CompletedTask);
        storageMock.Setup(storage => storage.UploadFileAsync(formFileMock.Object, "01_applications")).ReturnsAsync(("https://cdn.example.com/passport.pdf", "applications/passport.pdf"));
        var repositoryMock = new Mock<IDocumentRepository>();
        repositoryMock.Setup(repository => repository.GetDocumentByIdAsync(7)).ReturnsAsync(document);
        repositoryMock.Setup(repository => repository.UpdateDocumentAsync(It.IsAny<DocumentDto>())).Returns(Task.CompletedTask);
        var controller = new DocumentsController(storageMock.Object, repositoryMock.Object);

        // Act
        var result = await controller.UploadDocument(7, formFileMock.Object);

        // Assert
        Assert.IsType<OkObjectResult>(result);
        storageMock.Verify(storage => storage.DeleteFileAsync("applications/old-passport.pdf", "01_applications"), Times.Once);
    }
    #endregion

    #region DownloadDocument
    [Fact]
    public async Task DownloadDocument_ShouldReturnFile_WhenStorageReturnsStream()
    {
        // Arrange
        var stream = new MemoryStream(new byte[] { 1, 2, 3 });
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.GetFileAsync("applications/file.pdf", "01_applications")).ReturnsAsync(stream);
        var repositoryMock = new Mock<IDocumentRepository>();
        var controller = new DocumentsController(storageMock.Object, repositoryMock.Object);

        // Act
        var result = await controller.DownloadDocument("applications/file.pdf");

        // Assert
        var fileResult = Assert.IsType<FileStreamResult>(result);
        Assert.Equal("application/octet-stream", fileResult.ContentType);
    }

    [Fact]
    public async Task DownloadDocument_ShouldReturnNotFound_WhenStorageReturnsNull()
    {
        // Arrange
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.GetFileAsync("missing.pdf", "01_applications")).ReturnsAsync((Stream)null!);
        var repositoryMock = new Mock<IDocumentRepository>();
        var controller = new DocumentsController(storageMock.Object, repositoryMock.Object);

        // Act
        var result = await controller.DownloadDocument("missing.pdf");

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    #endregion

    #region DeleteDocument
    [Fact]
    public async Task DeleteDocument_ShouldClearStoredMetadataAndReturnNoContent_WhenDocumentHasAKey()
    {
        // Arrange
        var document = new DocumentDto
        {
            Id = 6,
            FileName = "justification",
            Type = DocumentType.RENTAL_APPLICATION,
            Key = "applications/justification.pdf",
            Url = "https://cdn.example.com/justification.pdf",
            MimeType = "application/pdf",
            Extension = ".pdf"
        };
        var storageMock = new Mock<IStorageService>();
        storageMock.Setup(storage => storage.DeleteFileAsync("applications/justification.pdf", "01_applications")).Returns(Task.CompletedTask);
        var repositoryMock = new Mock<IDocumentRepository>();
        repositoryMock.Setup(repository => repository.GetDocumentByIdAsync(6)).ReturnsAsync(document);
        repositoryMock.Setup(repository => repository.UpdateDocumentAsync(It.Is<DocumentDto>(updatedDocument => updatedDocument.Key == null && updatedDocument.Url == null && updatedDocument.MimeType == null && updatedDocument.Extension == null))).Returns(Task.CompletedTask);
        var controller = new DocumentsController(storageMock.Object, repositoryMock.Object);

        // Act
        var result = await controller.DeleteDocument(6);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteDocument_ShouldReturnNoContentWithoutStorageCall_WhenDocumentHasNoKey()
    {
        // Arrange
        var document = new DocumentDto { Id = 6, FileName = "justification", Type = DocumentType.RENTAL_APPLICATION, Key = null };
        var storageMock = new Mock<IStorageService>();
        var repositoryMock = new Mock<IDocumentRepository>();
        repositoryMock.Setup(repository => repository.GetDocumentByIdAsync(6)).ReturnsAsync(document);
        var controller = new DocumentsController(storageMock.Object, repositoryMock.Object);

        // Act
        var result = await controller.DeleteDocument(6);

        // Assert
        Assert.IsType<NoContentResult>(result);
        storageMock.Verify(storage => storage.DeleteFileAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        repositoryMock.Verify(repository => repository.UpdateDocumentAsync(It.IsAny<DocumentDto>()), Times.Never);
    }
    #endregion

    private static Mock<IFormFile> CreateFormFileMock(string fileName, string contentType, long length)
    {
        var formFileMock = new Mock<IFormFile>();
        formFileMock.Setup(file => file.FileName).Returns(fileName);
        formFileMock.Setup(file => file.ContentType).Returns(contentType);
        formFileMock.Setup(file => file.Length).Returns(length);
        return formFileMock;
    }
}