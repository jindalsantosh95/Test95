using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Validators
{
    public class AwardAuditValidator : AbstractValidator<AwardAudit>
    {
        public AwardAuditValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.AwardId).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.AwardProductId).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
        }
    }
}
