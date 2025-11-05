using FluentValidation;

namespace Template.Application.Features.Authentication.Queries.GetPaginatedUsers
{
    public class GetPaginatedUsersQueryValidator : AbstractValidator<GetPaginatedUsersQuery>
    {
        public GetPaginatedUsersQueryValidator()
        {
            RuleFor(x => x.PageNumber)
                .GreaterThan(0)
                .WithMessage("Page number must be greater than 0");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("Page size must be greater than 0")
                .LessThanOrEqualTo(100)
                .WithMessage("Page size must not exceed 100");

            RuleFor(x => x.SearchTerm)
                .MaximumLength(100)
                .When(x => !string.IsNullOrEmpty(x.SearchTerm))
                .WithMessage("Search term must not exceed 100 characters");
        }
    }
}
