
using mmotors_back.Models;

namespace mmotors_back.Features.DocumentTemplates.Dtos
{
    public class DocumentTemplateDto
    {
        public int? Id { get; set; }
        public required string Name { get; set; }
        public DocumentType Type { get; set; }
        public bool IsActive { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}