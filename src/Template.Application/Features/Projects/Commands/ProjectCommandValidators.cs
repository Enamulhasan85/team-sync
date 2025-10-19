using FluentValidation;
using MongoDB.Bson;

namespace Template.Application.Features.Projects.Commands;

public class CreateProjectCommandValidator : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Project description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before end date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid project status.");

        RuleFor(x => x.MemberIds)
            .Must(BeValidObjectIdList)
            .WithMessage("One or more MemberIds have invalid format.")
            .When(x => x.MemberIds != null && x.MemberIds.Any());
    }

    private bool BeValidObjectIdList(List<string>? memberIds)
    {
        if (memberIds == null || !memberIds.Any())
            return true;

        return memberIds.All(id => !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _));
    }
}

public class UpdateProjectCommandValidator : AbstractValidator<UpdateProjectCommand>
{
    public UpdateProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project Id is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid Project Id format.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Project name is required.")
            .MaximumLength(200).WithMessage("Project name must not exceed 200 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(2000).WithMessage("Project description must not exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Description));

        RuleFor(x => x.StartDate)
            .LessThan(x => x.EndDate)
            .WithMessage("Start date must be before end date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.EndDate)
            .GreaterThan(x => x.StartDate)
            .WithMessage("End date must be after start date.")
            .When(x => x.StartDate.HasValue && x.EndDate.HasValue);

        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid project status.");

        RuleFor(x => x.MemberIds)
            .Must(BeValidObjectIdList)
            .WithMessage("One or more MemberIds have invalid format.")
            .When(x => x.MemberIds != null && x.MemberIds.Any());
    }

    private bool BeValidObjectId(string id)
        => ObjectId.TryParse(id, out _);

    private bool BeValidObjectIdList(List<string>? memberIds)
    {
        if (memberIds == null || !memberIds.Any())
            return true;

        return memberIds.All(id => !string.IsNullOrWhiteSpace(id) && ObjectId.TryParse(id, out _));
    }
}

public class DeleteProjectCommandValidator : AbstractValidator<DeleteProjectCommand>
{
    public DeleteProjectCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Project Id is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid Project Id format.");
    }

    private bool BeValidObjectId(string id)
        => ObjectId.TryParse(id, out _);
}
