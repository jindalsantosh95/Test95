using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class FlyingClubMemberPatchValidators : AbstractValidator<FlyingClubMember>
    {
        public FlyingClubMemberPatchValidators()
        {
        }
    }
}
