using System;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;
using Template.Domain.Common;

namespace Template.Domain.Entities
{
    [CollectionName("chatMessages")]
    public class ChatMessage : AuditableEntity<ObjectId>
    {
        public ObjectId ProjectId { get; set; }
        public ObjectId SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public required string Message { get; set; }
    }
}