using Microsoft.EntityFrameworkCore;
using mmotors_back.Data;
using mmotors_back.Features.DocumentTemplates.Repositories;
using mmotors_back.Models;

namespace mmotors_back.Tests.Features.DocumentTemplates
{
	public class DocumentTemplatesRepositoryTests
	{
		private readonly DbContextOptions<AppDbContext> _dbOptions;

		public DocumentTemplatesRepositoryTests()
		{
			_dbOptions = new DbContextOptionsBuilder<AppDbContext>()
				.UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
				.Options;
		}

		[Fact]
		public async Task GetDocumentTemplateByIdAsync_ReturnsTemplate_WhenTemplateExists()
		{
			using var context = new AppDbContext(_dbOptions);
			var template = new DocumentTemplate
			{
				Id = 1,
				Name = "Proof of Identity",
				Type = DocumentType.COMMON_APPLICATION,
				IsActive = true
			};

			context.DocumentTemplates.Add(template);
			await context.SaveChangesAsync();

			var repository = new DocumentTemplateRepository(context);

			var result = await repository.GetDocumentTemplateByIdAsync(1);

			Assert.NotNull(result);
			Assert.Equal(template.Id, result.Id);
			Assert.Equal(template.Name, result.Name);
			Assert.Equal(template.Type, result.Type);
		}

		[Fact]
		public async Task GetDocumentTemplateByIdAsync_Throws_WhenTemplateDoesNotExist()
		{
			using var context = new AppDbContext(_dbOptions);
			var repository = new DocumentTemplateRepository(context);

			await Assert.ThrowsAsync<KeyNotFoundException>(() => repository.GetDocumentTemplateByIdAsync(999));
		}

		[Fact]
		public async Task UpdateDocumentTemplateAsync_UpdatesTemplate_WhenTemplateExists()
		{
			using var context = new AppDbContext(_dbOptions);
			var originalUpdatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			var template = new DocumentTemplate
			{
				Id = 1,
				Name = "Old Template",
				Type = DocumentType.COMMON_APPLICATION,
				IsActive = true,
				CreatedAt = originalUpdatedAt,
				UpdatedAt = originalUpdatedAt
			};

			context.DocumentTemplates.Add(template);
			await context.SaveChangesAsync();

			var repository = new DocumentTemplateRepository(context);
			var updatedTemplate = new DocumentTemplate
			{
				Id = 1,
				Name = "Updated Template",
				Type = DocumentType.SALES_APPLICATION,
				IsActive = false
			};

			var result = await repository.UpdateDocumentTemplateAsync(updatedTemplate);
			var persistedTemplate = await context.DocumentTemplates.FindAsync(1);

			Assert.True(result);
			Assert.NotNull(persistedTemplate);
			Assert.Equal("Updated Template", persistedTemplate.Name);
			Assert.Equal(DocumentType.SALES_APPLICATION, persistedTemplate.Type);
			Assert.False(persistedTemplate.IsActive);
			Assert.True(persistedTemplate.UpdatedAt > originalUpdatedAt);
		}
	}
}
