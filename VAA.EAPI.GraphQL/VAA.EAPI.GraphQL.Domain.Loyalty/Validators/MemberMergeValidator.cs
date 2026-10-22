using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class MemberMergeValidator : AbstractValidator<MemberMergeMutationRequest>
    {
        public MemberMergeValidator()
        {
            RuleFor(x => x.MergedCustomerId).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.MergedMemberNumber));
            RuleFor(x => x.MergedMemberNumber).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.MergedCustomerId));
            RuleFor(x => x.SurvivorCustomerId).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.SurvivorMemberNumber));
            RuleFor(x => x.SurvivorMemberNumber).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.SurvivorCustomerId));
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty();
        }
    }
}
