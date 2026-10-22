using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;

namespace VAA.EAPI.GraphQL.Domain.Security.Validators.NDC
{
    public class NDCAggregatorValidator : AbstractValidator<NDCAggregatorDetails?>
    {
        public NDCAggregatorValidator(string operationName)
        {
            When(model => model is not null, () =>
            {
                RuleFor(model => model!.AggregatorId).NotEmpty().NotNull();
                if (operationName.Equals("Create"))
                    RuleFor(model => model!.AggregatorName).NotEmpty().NotNull();

            });
        }
    }
}
