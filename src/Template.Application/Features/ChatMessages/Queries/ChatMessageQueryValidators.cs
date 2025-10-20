using FluentValidation;
using MongoDB.Bson;

namespace Template.Application.Features.ChatMessages.Queries
{
    public class GetPaginatedChatMessagesQueryValidator : AbstractValidator<GetPaginatedChatMessagesQuery>
    {
        public GetPaginatedChatMessagesQueryValidator()
        {
            RuleFor(x => x.ProjectId)
                .NotEmpty().WithMessage("ProjectId is required")
                .Must(BeValidObjectId).WithMessage("ProjectId must be a valid ObjectId");

            RuleFor(x => x.PageNumber)
                .GreaterThan(0).WithMessage("PageNumber must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0).WithMessage("PageSize must be greater than 0")
                .LessThanOrEqualTo(100).WithMessage("PageSize must not exceed 100");

            RuleFor(x => x.SortBy)
                .Must(BeValidSortField).When(x => !string.IsNullOrEmpty(x.SortBy))
                .WithMessage("SortBy must be one of: SenderName, CreatedAt");
        }

        private bool BeValidObjectId(string id)
        {
            return ObjectId.TryParse(id, out _);
        }

        private bool BeValidSortField(string? sortBy)
        {
            if (string.IsNullOrEmpty(sortBy))
                return true;

            var validFields = new[] { "sendername", "createdat" };
            return validFields.Contains(sortBy.ToLower());
        }
    }
}
