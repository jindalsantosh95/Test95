using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Constants;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.BulkMemberUpdate;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class BulkMemberUpdateValidator : AbstractValidator<BulkMemberUpdateRequest>
    {
        public BulkMemberUpdateValidator()
        {
            RuleFor(x => x.BlobFilePath)
                .NotNull().NotEmpty().WithMessage("BlobFilePath is required");

            RuleFor(x => x.InitiatedBy)
                .NotNull().NotEmpty().WithMessage("InitiatedBy (user email) is required")
                .EmailAddress().WithMessage("InitiatedBy must be a valid email");

            RuleFor(x => x.MemberNotes)
                .NotNull().NotEmpty().WithMessage("MemberNotes is required when MemberStatus is provided")
                .When(x => !string.IsNullOrWhiteSpace(x.MemberStatus));

            RuleFor(x => x.MemberStatus)
                .Must(status => string.IsNullOrWhiteSpace(status) ||
                                Application.ValidMemberStatuses.Contains(status!.ToUpperInvariant()))
                .WithMessage($"Invalid MemberStatus. Allowed values: {string.Join(", ", Application.ValidMemberStatuses)}")
                .When(x => !string.IsNullOrWhiteSpace(x.MemberStatus));
        }
    }
}