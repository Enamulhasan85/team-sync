using FluentValidation;
using MongoDB.Bson;

namespace Template.Application.Features.Authentication.Queries.UserInfo
{
    public class UserInfoQueryValidator : AbstractValidator<UserInfoQuery>
    {
        public UserInfoQueryValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");
        }
    }
}
