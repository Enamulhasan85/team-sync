using FluentValidation;
using MongoDB.Bson;

namespace Template.Application.Features.Tasks.Queries;

public class GetTaskByIdQueryValidator : AbstractValidator<GetTaskByIdQuery>
{
    public GetTaskByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Task Id is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid Task Id format.");
    }

    private bool BeValidObjectId(string id)
        => ObjectId.TryParse(id, out _);
}

public class GetPaginatedTasksQueryValidator : AbstractValidator<GetPaginatedTasksQuery>
{
    public GetPaginatedTasksQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

        RuleFor(x => x.ProjectId)
            .Must(BeValidObjectIdOrNull)
            .WithMessage("Invalid ProjectId format.");

        RuleFor(x => x.AssigneeId)
            .Must(BeValidObjectIdOrNull)
            .WithMessage("Invalid AssigneeId format.");

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage("Invalid sort field. Allowed values: Title, Status, DueDate, CreatedAt, UpdatedAt.");
    }

    private bool BeValidObjectIdOrNull(string? id)
        => string.IsNullOrWhiteSpace(id) || ObjectId.TryParse(id, out _);

    private bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return true;

        var allowedFields = new[] { "Title", "Status", "DueDate", "CreatedAt", "UpdatedAt" };
        return allowedFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }
}
