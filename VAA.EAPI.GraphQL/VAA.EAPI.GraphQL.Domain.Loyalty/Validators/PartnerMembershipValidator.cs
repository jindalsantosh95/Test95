using FluentValidation;
using System.Net.Http;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class PartnerMembershipValidator : AbstractValidator<PartnerMembershipMutationRequest>
    {
        public PartnerMembershipValidator(HttpMethod verb)
        {
            RuleFor(x => x.PartnerCode).NotNull().NotEmpty();

            if (verb != HttpMethod.Post)
            {
                RuleFor(x => x.Id).NotNull().NotEmpty();
                RuleFor(x => x.EndDate).NotNull().NotEmpty();
            }
        }
    }
}
