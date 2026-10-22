using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.Request;

namespace VAA.EAPI.GraphQL.Domain.Security.Validators.APIM
{
    public class UserAccountValidator : AbstractValidator<UserAccountDetails>
    {
        public UserAccountValidator()
        {
            When(model => model is not null, () =>
            {
                RuleFor(model => model.FirstName).NotEmpty().NotNull();
                RuleFor(model => model.LastName).NotEmpty().NotNull();
                RuleFor(model => model.EmailAddress).NotEmpty().NotNull();

            }).Otherwise(() =>
            {
                RuleFor(model => model).NotEmpty().NotNull();
            });

        }

    }
}
