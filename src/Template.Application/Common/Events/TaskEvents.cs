using Template.Domain.Enums;

namespace Template.Application.Common.Events;

/// <summary>
/// Base class for task-related events
/// </summary>
public abstract class TaskEventBase
{
    public string TaskId { get; set; } = string.Empty;
    public string ProjectId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime EventTimestamp { get; set; } = DateTime.UtcNow;
}

/// <summary>
/// Event published when a new task is created
/// </summary>
public class TaskCreatedEvent : TaskEventBase
{
    public string? Description { get; set; }
    public TaskWorkflowStatus Status { get; set; }
    public string? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? CreatedBy { get; set; }
}

/// <summary>
/// Event published when a task is updated
/// </summary>
public class TaskUpdatedEvent : TaskEventBase
{
    public string? Description { get; set; }
    public TaskWorkflowStatus Status { get; set; }
    public string? AssigneeId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? LastModifiedBy { get; set; }
    public List<string> ChangedFields { get; set; } = new();
}

/// <summary>
/// Event published when a task is deleted
/// </summary>
public class TaskDeletedEvent : TaskEventBase
{
    public string? DeletedBy { get; set; }
}
