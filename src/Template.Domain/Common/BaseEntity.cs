using MongoDbGenericRepository.Models;

namespace Template.Domain.Common;

public abstract class BaseEntity<TKey> : IDocument<TKey> where TKey : IEquatable<TKey>
{
    public TKey Id { get; set; } = default!;
    public int Version { get; set; }
}

public abstract class AuditableEntity<TKey> : BaseEntity<TKey> where TKey : IEquatable<TKey>
{
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public string? CreatedBy { get; set; }
    public DateTime? LastModifiedAt { get; set; }
    public string? LastModifiedBy { get; set; }
    public bool IsDeleted { get; set; }

    public void MarkCreated(string? userId = null)
    {
        CreatedAt = DateTime.UtcNow;
        CreatedBy = userId;
    }

    public void MarkModified(string? userId = null)
    {
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = userId;
    }

    public void MarkDeleted(string? userId = null)
    {
        IsDeleted = true;
        LastModifiedAt = DateTime.UtcNow;
        LastModifiedBy = userId;
    }
}


