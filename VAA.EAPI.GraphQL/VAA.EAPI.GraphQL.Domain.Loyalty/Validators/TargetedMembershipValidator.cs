using FluentValidation;
using System.Net.Http;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class TargetedMembershipValidator : AbstractValidator<TargetedMembership>
    {
        public TargetedMembershipValidator(HttpMethod verb)
        {
            if (verb != HttpMethod.Post)
            {
                RuleFor(x => x.Id).NotNull().NotEmpty();
            }
        }
    }
}
