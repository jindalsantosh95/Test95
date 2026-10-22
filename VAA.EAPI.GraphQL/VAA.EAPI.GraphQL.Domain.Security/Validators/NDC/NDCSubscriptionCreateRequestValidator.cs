using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Security.Models.NDC;
using VAA.EAPI.GraphQL.Domain.Security.Validators.APIM;

namespace VAA.EAPI.GraphQL.Domain.Security.Validators.NDC
{
    public class NDCSubscriptionCreateRequestValidator : AbstractValidator<NDCSubscriptionCreateMutationRequest>
    {
        private readonly string operationName = "Create";
        public NDCSubscriptionCreateRequestValidator()
        {

            When(model => model.Party.Agency is not null || model.Party.Aggregator is not null, () =>
            {
                RuleFor(model => model.Party.Agency).SetValidator(new NDCAgencyValidator(operationName));
                RuleFor(model => model.Party.Aggregator).SetValidator(new NDCAggregatorValidator(operationName));
            });
            When(model => model.Party.Agency is null && model.Party.Aggregator is null, () =>
            {
                RuleFor(model => model.Party.Agency).NotEmpty();
                RuleFor(model => model.Party.Aggregator).NotEmpty();
            });
            When(model => model.Party is not null && model.Party.Aggregator?.SingleKeyAggregator == true, () =>
            {
                RuleFor(model => model.Party.Agency)
                    .Null()
                    .WithMessage("Agency details must not be supplied for a single-key aggregator.");
            });
            When(model => model.Party is not null && model.Party.Agency is null && model.Party.Aggregator is not null, () =>
            {
                RuleFor(model => model.Party.Aggregator!.SingleKeyAggregator)
                    .Equal(true)
                    .WithMessage("Either agency details must be supplied, or SingleKeyAggregator must be true.");
            });
            RuleFor(model => model.UserAccount).SetValidator(new UserAccountValidator());
            Include(new BaseValidator());
        }
    }
}
