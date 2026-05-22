/*
    * this fils tests the DocumentRepository class
    * it uses the Moq library to mock the dependencies of the DocumentRepository class
    * it uses the xUnit library to run the tests
    * it tests the GetDocumentByIdAsync and UpdateDocumentAsync methods of the DocumentRepository class
    * it checks if the methods return the expected results and if they interact with the dependencies as expected
    * it uses the IStorageService to handle the storage of documents
    * it receives the documents id and the document IformFile from the request and uses the IStorageService to handle the storage of the documents
*/

using System;
using System.Threading.Tasks;
using Xunit;
using Moq;
using mmotors_back.Features.Documents.Repositories;
using mmotors_back.Features.Shared.Interfaces;
using mmotors_back.Data;
using mmotors_back.Models;
using Microsoft.EntityFrameworkCore;
using mmotors_back.Features.Documents.Interfaces;
using mmotors_back.Features.Documents.Dtos;
using mmotors_back.Mappers;

namespace mmotors_back.Tests.Features.Documents
{
    public class DocumentRepositoryTests
    {
        
        private readonly DbContextOptions<AppDbContext> _dbOptions;

        public DocumentRepositoryTests()
        {
            _dbOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        #region GetDocumentByIdAsync Tests
            [Fact]
            public async Task GetDocumentByIdAsync_ReturnsDocument_WhenDocumentExists()
            {
                // Arrange
                using (var context = new AppDbContext(_dbOptions))
                {
                    var document = new Document { Id = 1, FileName = "test.pdf", Url = "http://example.com/test.pdf" };
                    await context.Documents.AddAsync(document);
                    await context.SaveChangesAsync();

                    var repository = new DocumentRepository(context);

                    // Act
                    var result = await repository.GetDocumentByIdAsync(1);

                    // Assert
                    Assert.NotNull(result);
                    Assert.Equal(result.Id, document.Id);
                    Assert.Equal(result.FileName, document.FileName);
                }
            }

        #endregion

        #region UpdateDocumentAsync Tests
            [Fact]
            public async Task UpdateDocumentAsync_UpdatesDocument_WhenDocumentExists()
            {
                // Arrange
                using (var context = new AppDbContext(_dbOptions))
                {
                    var documentToUpdate = new Document { Id = 1, FileName = "test.pdf", Url = "http://example.com/test.pdf" };
                    context.Documents.Add(documentToUpdate);
                    await context.SaveChangesAsync();

                    var repository = new DocumentRepository(context);
                    var documentDto = DocumentMapper.ToDto(documentToUpdate);
                    documentDto.FileName = "updated_test.pdf";

                    // Act
                    await repository.UpdateDocumentAsync(documentDto);

                    // Assert
                    var updatedDocument = await context.Documents.FindAsync(1);
                    Assert.NotNull(updatedDocument);
                    Assert.Equal("updated_test.pdf", updatedDocument.FileName);
                }
            }

        #endregion
    }
}