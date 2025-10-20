using FluentValidation;
using MongoDB.Bson;

namespace Template.Application.Features.ChatMessages.Commands;

public class CreateChatMessageCommandValidator : AbstractValidator<CreateChatMessageCommand>
{
    public CreateChatMessageCommandValidator()
    {
        RuleFor(x => x.Message)
            .NotEmpty().WithMessage("ChatMessage content is required.")
            .MaximumLength(2000).WithMessage("ChatMessage content must not exceed 2000 characters.")
            .When(x => !string.IsNullOrWhiteSpace(x.Message));

        RuleFor(x => x.ProjectId)
            .NotEmpty().WithMessage("ProjectId is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid ProjectId format.");
    }

    private bool BeValidObjectId(string projectId)
        => ObjectId.TryParse(projectId, out _);

}


public class UpdateChatMessageCommandValidator : AbstractValidator<UpdateChatMessageCommand>
{
    public UpdateChatMessageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ChatMessage Id is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid ChatMessage Id format.");
    }

    private bool BeValidObjectId(string id)
        => ObjectId.TryParse(id, out _);
}

public class DeleteChatMessageCommandValidator : AbstractValidator<DeleteChatMessageCommand>
{
    public DeleteChatMessageCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("ChatMessage Id is required.")
            .Must(BeValidObjectId)
            .WithMessage("Invalid ChatMessage Id format.");
    }

    private bool BeValidObjectId(string id)
        => ObjectId.TryParse(id, out _);
}
