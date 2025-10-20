using System.ComponentModel.DataAnnotations;

namespace Template.Domain.Enums;

public enum TaskWorkflowStatus
{
    [Display(Name = "To Do")]
    ToDo = 1,

    [Display(Name = "In Progress")]
    InProgress = 2,

    [Display(Name = "Done")]
    Done = 3,

    [Display(Name = "Blocked")]
    Blocked = 4
}