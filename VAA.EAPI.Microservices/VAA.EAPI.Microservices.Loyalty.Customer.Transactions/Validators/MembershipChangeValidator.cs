using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MembershipChange;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class MembershipChangeCreateValidator : AbstractValidator<MembershipChange>
    {
        public MembershipChangeCreateValidator()
        {
        }
    }

    public class MembershipChangePatchValidator : AbstractValidator<MembershipChange>
    {
        public MembershipChangePatchValidator()
        {
        }
    }
}
