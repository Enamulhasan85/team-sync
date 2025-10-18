using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;
using Template.Domain.Common;

namespace Template.Domain.Entities
{
    [CollectionName("projects")]
    public class Project : AuditableEntity<ObjectId>
    {
        public required string Name { get; set; }
        public string? Description { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public ProjectStatus Status { get; set; }
        public List<ObjectId> MemberIds { get; set; } = new();
    }
}