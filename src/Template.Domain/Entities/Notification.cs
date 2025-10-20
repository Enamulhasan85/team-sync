using System;
using MongoDB.Bson;
using MongoDbGenericRepository.Attributes;
using Template.Domain.Common;

namespace Template.Domain.Entities
{
    [CollectionName("notifications")]
    public class Notification : AuditableEntity<ObjectId>
    {
        public ObjectId RecipientId { get; set; }
        public string Title { get; set; }
        public string Message { get; set; }
        public DateTime? ReadAt { get; set; }
        public bool IsRead => ReadAt.HasValue;

        public Notification(ObjectId recipientId, string title, string message)
        {
            RecipientId = recipientId;
            Title = title;
            Message = message;
        }

        public void MarkAsRead()
        {
            if (!IsRead)
            {
                ReadAt = DateTime.UtcNow;
            }
        }

        public void MarkAsUnread()
        {
            ReadAt = null;
        }
    }
}