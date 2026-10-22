using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class MemberProfileValidation : AbstractValidator<VACMemberProfileUpdateRequest>
    {
        public MemberProfileValidation()
        {
            {
                RuleFor(x => x.MemberNumber).NotNull().NotEmpty();
                RuleFor(x => x.CustomerId).NotNull().NotEmpty();

            }
        }
    }
}
