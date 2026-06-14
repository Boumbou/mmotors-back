using Microsoft.AspNetCore.Mvc;
using mmotors_back.Features.DocumentTemplates.Controllers;
using mmotors_back.Features.DocumentTemplates.Dtos;
using mmotors_back.Features.DocumentTemplates.Interfaces;
using mmotors_back.Models;
using Moq;

namespace mmotors_back.Tests.Features.DocumentTemplates;

public class DocumentTemplateControllerTests
{
    #region GetAllDocumentTemplatesAsync
    [Fact]
    public async Task GetAllDocumentTemplatesAsync_ShouldReturnOk_WhenTemplatesExist()
    {
        // Arrange
        var templates = new List<DocumentTemplate>
        {
            new() { Id = 1, Name = "Identity card", Type = DocumentType.COMMON_APPLICATION },
            new() { Id = 2, Name = "Proof of income", Type = DocumentType.RENTAL_APPLICATION }
        };
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.GetAllDocumentTemplatesAsync()).ReturnsAsync(templates);
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.GetAllDocumentTemplatesAsync();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        var returnedTemplates = Assert.IsAssignableFrom<IEnumerable<DocumentTemplate>>(okResult.Value);
        Assert.Equal(2, returnedTemplates.Count());
    }
    #endregion

    #region GetDocumentTemplateByIdAsync
    [Fact]
    public async Task GetDocumentTemplateByIdAsync_ShouldReturnOk_WhenTemplateExists()
    {
        // Arrange
        var template = new DocumentTemplate { Id = 1, Name = "Identity card", Type = DocumentType.COMMON_APPLICATION };
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.GetDocumentTemplateByIdAsync(1)).ReturnsAsync(template);
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.GetDocumentTemplateByIdAsync(1);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(template.Id, ((DocumentTemplate)okResult.Value!).Id);
    }

    [Fact]
    public async Task GetDocumentTemplateByIdAsync_ShouldReturnNotFound_WhenTemplateDoesNotExist()
    {
        // Arrange
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.GetDocumentTemplateByIdAsync(2)).ReturnsAsync((DocumentTemplate)null!);
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.GetDocumentTemplateByIdAsync(2);

        // Assert
        Assert.IsType<NotFoundResult>(result.Result);
    }
    #endregion

    #region CreateDocumentTemplateAsync
    [Fact]
    public async Task CreateDocumentTemplateAsync_ShouldReturnCreatedAtAction_WhenTemplateIsCreated()
    {
        // Arrange
        var templateDto = new DocumentTemplateDto { Name = "Proof of address", Type = DocumentType.COMMON_APPLICATION, IsActive = true };
        var createdTemplate = new DocumentTemplate { Id = 4, Name = templateDto.Name, Type = templateDto.Type, IsActive = true };
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.CreateDocumentTemplateAsync(templateDto)).ReturnsAsync(createdTemplate);
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.CreateDocumentTemplateAsync(templateDto);

        // Assert
        var createdAtActionResult = Assert.IsType<CreatedAtActionResult>(result.Result);
        Assert.Equal(createdTemplate.Id, ((DocumentTemplate)createdAtActionResult.Value!).Id);
    }

    [Fact]
    public async Task CreateDocumentTemplateAsync_ShouldReturnBadRequest_WhenRepositoryThrows()
    {
        // Arrange
        var templateDto = new DocumentTemplateDto { Name = "Proof of address", Type = DocumentType.COMMON_APPLICATION, IsActive = true };
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.CreateDocumentTemplateAsync(templateDto)).ThrowsAsync(new Exception("duplicate"));
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.CreateDocumentTemplateAsync(templateDto);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
    }
    #endregion

    #region UpdateDocumentTemplateAsync
    [Fact]
    public async Task UpdateDocumentTemplateAsync_ShouldReturnNoContent_WhenUpdateSucceeds()
    {
        // Arrange
        var templateDto = new DocumentTemplateDto { Id = 3, Name = "Payslip", Type = DocumentType.RENTAL_APPLICATION, IsActive = true };
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.UpdateDocumentTemplateAsync(templateDto)).ReturnsAsync(true);
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.UpdateDocumentTemplateAsync(3, templateDto);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task UpdateDocumentTemplateAsync_ShouldReturnBadRequest_WhenIdDoesNotMatchDtoId()
    {
        // Arrange
        var templateDto = new DocumentTemplateDto { Id = 3, Name = "Payslip", Type = DocumentType.RENTAL_APPLICATION, IsActive = true };
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.UpdateDocumentTemplateAsync(7, templateDto);

        // Assert
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task UpdateDocumentTemplateAsync_ShouldReturnNotFound_WhenRepositoryReturnsFalse()
    {
        // Arrange
        var templateDto = new DocumentTemplateDto { Id = 3, Name = "Payslip", Type = DocumentType.RENTAL_APPLICATION, IsActive = true };
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.UpdateDocumentTemplateAsync(templateDto)).ReturnsAsync(false);
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.UpdateDocumentTemplateAsync(3, templateDto);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    #endregion

    #region DeleteDocumentTemplateAsync
    [Fact]
    public async Task DeleteDocumentTemplateAsync_ShouldReturnNoContent_WhenDeleteSucceeds()
    {
        // Arrange
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.DeleteDocumentTemplateAsync(5)).ReturnsAsync(true);
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.DeleteDocumentTemplateAsync(5);

        // Assert
        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteDocumentTemplateAsync_ShouldReturnNotFound_WhenRepositoryReturnsFalse()
    {
        // Arrange
        var repositoryMock = new Mock<IDocumentTemplateRepository>();
        repositoryMock.Setup(repository => repository.DeleteDocumentTemplateAsync(5)).ReturnsAsync(false);
        var controller = new DocumentTemplateController(repositoryMock.Object);

        // Act
        var result = await controller.DeleteDocumentTemplateAsync(5);

        // Assert
        Assert.IsType<NotFoundResult>(result);
    }
    #endregion
}