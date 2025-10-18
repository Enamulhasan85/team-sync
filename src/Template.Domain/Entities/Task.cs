using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;
using Template.Domain.Common;
using Template.Domain.Enums;

namespace Template.Domain.Entities
{
    [CollectionName("tasks")]
    public class Task : AuditableEntity<ObjectId>
    {
        public ObjectId ProjectId { get; set; }
        public required string Title { get; set; }
        public string? Description { get; set; }
        public TaskWorkflowStatus Status { get; set; }
        public ObjectId? AssigneeId { get; set; }
        public DateTime? DueDate { get; set; }
    }
}