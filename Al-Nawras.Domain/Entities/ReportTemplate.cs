using Al_Nawras.Domain.Enums;

namespace Al_Nawras.Domain.Entities
{
    public class ReportTemplate
    {
        public Guid Id { get; private set; }
        public string Name { get; private set; }
        public string Slug { get; private set; }
        public ReportTemplateCategory Category { get; private set; }
        public string Description { get; private set; }
        public string DefinitionJson { get; private set; }
        public bool IsSystem { get; private set; }
        public int? CreatedByUserId { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        public User? CreatedByUser { get; private set; }
        public ICollection<ReportImport> ReportImports { get; private set; } = new List<ReportImport>();

        private ReportTemplate() { }

        public ReportTemplate(
            string name,
            string slug,
            ReportTemplateCategory category,
            string description,
            string definitionJson,
            bool isSystem,
            int? createdByUserId = null)
        {
            Id = Guid.NewGuid();
            Name = name;
            Slug = slug;
            Category = category;
            Description = description;
            DefinitionJson = definitionJson;
            IsSystem = isSystem;
            CreatedByUserId = createdByUserId;
            CreatedAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        public void Update(
            string name,
            string slug,
            ReportTemplateCategory category,
            string description,
            string definitionJson)
        {
            Name = name;
            Slug = slug;
            Category = category;
            Description = description;
            DefinitionJson = definitionJson;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
