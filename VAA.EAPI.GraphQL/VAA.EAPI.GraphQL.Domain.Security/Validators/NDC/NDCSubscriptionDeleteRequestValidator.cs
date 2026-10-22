using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;
using VAA.EAPI.GraphQL.Domain.Security.Validators.APIM;

namespace VAA.EAPI.GraphQL.Domain.Security.Validators.NDC
{
    public class NDCSubscriptionDeleteRequestValidator : AbstractValidator<NDCSubscriptionDeleteMutationRequest>
    {
        private readonly string operationName = "Delete";
        public NDCSubscriptionDeleteRequestValidator()
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
            When(model => model.Party is not null && model.Party.Agency is null && model.Party.Aggregator is not null, () =>
            {
                RuleFor(model => model.Party.Aggregator!.SingleKeyAggregator)
                    .Equal(true)
                    .WithMessage("SingleKeyAggregator must be true when only aggregator details are supplied.");
            });
            Include(new BaseValidator());
        }
    }
}
