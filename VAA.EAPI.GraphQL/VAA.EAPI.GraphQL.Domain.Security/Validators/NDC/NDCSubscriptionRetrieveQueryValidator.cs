using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;
using VAA.EAPI.GraphQL.Domain.Security.Validators.APIM;

namespace VAA.EAPI.GraphQL.Domain.Security.Validators.NDC
{
    public class NDCSubscriptionRetrieveQueryValidator : AbstractValidator<NDCSubscriptionRetrieveQueryRequest>
    {
        private readonly string operationName = "Retrieve";
        public NDCSubscriptionRetrieveQueryValidator()
        {
            When(model => model.Party is not null, () =>
            {
                RuleFor(model => model.Party.Agency).SetValidator(new NDCAgencyValidator(operationName));
                RuleFor(model => model.Party.Aggregator).SetValidator(new NDCAggregatorValidator(operationName));
            });
            When(model => model.Party.Agency is null && model.Party.Aggregator is null, () =>
            {
                RuleFor(model => model.Party.Agency).NotEmpty();
                RuleFor(model => model.Party.Aggregator).NotEmpty();
            });
            Include(new BaseValidator());
        }
    }
}
