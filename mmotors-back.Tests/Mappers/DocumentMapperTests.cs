using mmotors_back.Features.Documents.Dtos;
using mmotors_back.Mappers;
using mmotors_back.Models;

namespace mmotors_back.Tests.Mappers;

public class DocumentMapperTests
{
    #region ToDto
    [Fact]
    public void ToDto_ShouldMapAllFields_WhenEntityIsValid()
    {
        // Arrange
        var uploadedAt = DateTime.UtcNow;
        var document = new Document
        {
            Id = 8,
            ApplicationId = 11,
            VehicleId = 4,
            FileName = "quote.pdf",
            MimeType = "application/pdf",
            Extension = ".pdf",
            Url = "https://example.com/quote.pdf",
            Key = "documents/quote.pdf",
            Type = DocumentType.COMMON_APPLICATION,
            UploadedAt = uploadedAt
        };

        // Act
        var result = DocumentMapper.ToDto(document);

        // Assert
        Assert.Equal(document.Id, result.Id);
        Assert.Equal(document.ApplicationId, result.ApplicationId);
        Assert.Equal(document.VehicleId, result.VehicleId);
        Assert.Equal(document.FileName, result.FileName);
        Assert.Equal(document.MimeType, result.MimeType);
        Assert.Equal(document.Extension, result.Extension);
        Assert.Equal(document.Url, result.Url);
        Assert.Equal(document.Key, result.Key);
        Assert.Equal(document.UploadedAt, result.UploadedAt);
        Assert.Equal(document.Type, result.Type);
    }

    [Fact]
    public void ToDto_ShouldMapNullableUploadFieldsAsNull_WhenMetadataIsMissing()
    {
        // Arrange
        var document = new Document
        {
            Id = 2,
            FileName = "identity-card",
            Type = DocumentType.SALES_APPLICATION,
            MimeType = null,
            Extension = null,
            Url = null,
            Key = null
        };

        // Act
        var result = DocumentMapper.ToDto(document);

        // Assert
        Assert.Null(result.MimeType);
        Assert.Null(result.Extension);
        Assert.Null(result.Url);
        Assert.Null(result.Key);
    }
    #endregion

    #region ToEntity
    [Fact]
    public void ToEntity_ShouldMapCreateDtoFields_WhenDtoIsValid()
    {
        // Arrange
        var createDocumentDto = new CreateDocumentDto
        {
            ApplicationId = 6,
            VehicleId = 2,
            FileName = "registration-form",
            Type = DocumentType.RENTAL_APPLICATION
        };

        // Act
        var result = DocumentMapper.ToEntity(createDocumentDto);

        // Assert
        Assert.Equal(createDocumentDto.ApplicationId, result.ApplicationId);
        Assert.Equal(createDocumentDto.VehicleId, result.VehicleId);
        Assert.Equal(createDocumentDto.FileName, result.FileName);
        Assert.Equal(createDocumentDto.Type, result.Type);
    }

    [Fact]
    public void ToEntity_ShouldLeaveUploadMetadataUnset_WhenCreateDtoDoesNotContainIt()
    {
        // Arrange
        var createDocumentDto = new CreateDocumentDto
        {
            FileName = "driver-license",
            Type = DocumentType.COMMON_APPLICATION
        };

        // Act
        var result = DocumentMapper.ToEntity(createDocumentDto);

        // Assert
        Assert.Null(result.Url);
        Assert.Null(result.Key);
        Assert.Null(result.MimeType);
        Assert.Null(result.Extension);
    }
    #endregion
}