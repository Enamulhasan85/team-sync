using FluentValidation;
using MongoDB.Bson;

namespace Template.Application.Features.Projects.Queries;

public class GetProjectByIdQueryValidator : AbstractValidator<GetProjectByIdQuery>
{
    public GetProjectByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project Id is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid Project Id format.");
    }

    private bool BeValidObjectId(string id)
        => ObjectId.TryParse(id, out _);
}

public class GetPaginatedProjectsQueryValidator : AbstractValidator<GetPaginatedProjectsQuery>
{
    public GetPaginatedProjectsQueryValidator()
    {
        RuleFor(x => x.PageNumber)
            .GreaterThan(0).WithMessage("Page number must be greater than 0.");

        RuleFor(x => x.PageSize)
            .GreaterThan(0).WithMessage("Page size must be greater than 0.")
            .LessThanOrEqualTo(100).WithMessage("Page size must not exceed 100.");

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid project status.")
            .When(x => x.Status.HasValue);

        RuleFor(x => x.SortBy)
            .Must(BeValidSortField)
            .When(x => !string.IsNullOrWhiteSpace(x.SortBy))
            .WithMessage("Invalid sort field. Allowed values: Name, Status, StartDate, EndDate, CreatedAt.");
    }

    private bool BeValidSortField(string? sortBy)
    {
        if (string.IsNullOrWhiteSpace(sortBy))
            return true;

        var allowedFields = new[] { "Name", "Status", "StartDate", "EndDate", "CreatedAt", "LastModifiedAt" };
        return allowedFields.Contains(sortBy, StringComparer.OrdinalIgnoreCase);
    }
}
