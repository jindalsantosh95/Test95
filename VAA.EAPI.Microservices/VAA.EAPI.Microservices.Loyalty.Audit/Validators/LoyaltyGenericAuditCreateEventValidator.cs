using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Validators
{
    public class LoyaltyGenericAuditCreateEventValidator : AbstractValidator<LoyaltyGenericAudit>
    {
        public LoyaltyGenericAuditCreateEventValidator()
        {
            RuleFor(m => m.CustomerId).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.AuditCategory).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.CreatedBy).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.CreatedOn).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.DataUpdatedBy).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.DataUpdatedOn).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
        }
    }
}