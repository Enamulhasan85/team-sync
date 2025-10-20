using FluentValidation;
using MongoDB.Bson;

namespace Template.Application.Features.Tasks.Commands;

public class CreateTaskCommandValidator : AbstractValidator<CreateTaskCommand>
{
    public CreateTaskCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Task description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid ProjectId format.");

        RuleFor(x => x.AssigneeId)
            .Must(BeValidObjectIdOrNull)
            .WithMessage("Invalid AssigneeId format.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid task status.");
    }

    private bool BeValidObjectId(string projectId)
        => ObjectId.TryParse(projectId, out _);

    private bool BeValidObjectIdOrNull(string? id)
        => string.IsNullOrWhiteSpace(id) || ObjectId.TryParse(id, out _);
}


public class UpdateTaskCommandValidator : AbstractValidator<UpdateTaskCommand>
{
    public UpdateTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task Id is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid Task Id format.");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Task title is required.")
            .MaximumLength(200).WithMessage("Task title must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Task description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid ProjectId format.");

        RuleFor(x => x.AssigneeId)
            .Must(BeValidObjectIdOrNull)
            .WithMessage("Invalid AssigneeId format.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow.Date)
            .WithMessage("Due date must be in the future.")
            .When(x => x.DueDate.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid task status.");
    }

    private bool BeValidObjectId(string id)
        => ObjectId.TryParse(id, out _);

    private bool BeValidObjectIdOrNull(string? id)
        => string.IsNullOrWhiteSpace(id) || ObjectId.TryParse(id, out _);
}

public class DeleteTaskCommandValidator : AbstractValidator<DeleteTaskCommand>
{
    public DeleteTaskCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task Id is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid Task Id format.");
    }

    private bool BeValidObjectId(string id)
        => ObjectId.TryParse(id, out _);
}
