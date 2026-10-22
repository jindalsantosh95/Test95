using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Security.Models.APIM.BaseClasses;

namespace VAA.EAPI.GraphQL.Domain.Security.Validators.APIM
{
    public class BaseValidator : AbstractValidator<SubscriptionRequestBase>
    {
        public BaseValidator()
        {
            RuleFor(model => model.Channel).NotEmpty().NotNull();
            RuleFor(model => model.Environment).NotEmpty().NotNull();
        }
    }
}
