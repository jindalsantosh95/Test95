using FluentValidation;
using System.Net.Http;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.UpgradeVACMember;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class UpgradeVACMemberValidator : AbstractValidator<UpgradeVACMemberRequest>
    {
        public UpgradeVACMemberValidator(HttpMethod verb)
        {
            if (verb == HttpMethod.Post)
            {
                RuleFor(x => x.CustomerId).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.MemberNumber));
                RuleFor(x => x.MemberNumber).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.CustomerId));
                RuleFor(x => x.InitiatedBy).NotNull().NotEmpty();

                RuleFor(x => x.CustomerDetails).NotNull().NotEmpty();
                RuleFor(x => x.CustomerDetails.FirstName).NotNull().NotEmpty();
                RuleFor(x => x.CustomerDetails.LastName).NotNull().NotEmpty();
                RuleFor(x => x.CustomerDetails.Gender).NotNull().NotEmpty();
                RuleFor(x => x.CustomerDetails.DateOfBirth).NotNull().NotEmpty();
                RuleFor(x => x.CustomerDetails.Preferences).NotNull().NotEmpty();
                RuleFor(x => x.CustomerDetails.AddressDetails).NotNull().NotEmpty();
                RuleFor(x => x.CustomerDetails.LoyaltyMember.MediaCode).NotNull().NotEmpty();
                RuleFor(x => x.CustomerDetails.TravelProfile.ReasonForTravel).NotNull().NotEmpty();
            }
        }
    }
}
