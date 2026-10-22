using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class TargetedMembershipCreateValidator : AbstractValidator<TargetedMembership>
    {
        public TargetedMembershipCreateValidator()
        {
        }
    }

    public class TargetedMembershipDeleteValidator : AbstractValidator<TargetedMembership>
    {
        public TargetedMembershipDeleteValidator()
        {
        }
    }

    public class TargetedMembershipPatchValidator : AbstractValidator<TargetedMembership>
    {
        public TargetedMembershipPatchValidator()
        {
        }
    }

    public class TargetedMembershipIDValidator : AbstractValidator<TargetedMembership>
    {
        public TargetedMembershipIDValidator()
        {
        }

    }
    public class TargetedMembershipContentValidator : AbstractValidator<TargetedMembership>
    {

        public TargetedMembershipContentValidator()
        {
        }
    }


}
