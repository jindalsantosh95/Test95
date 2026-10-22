using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Validators
{
    public class PreferenceAuditCreateValidator : AbstractValidator<PreferenceAudit>
    {
        public PreferenceAuditCreateValidator()
        {
            RuleFor(m => m.CustomerId).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.VAMarketingPreference).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.DoNotCall).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.DoNotSMS).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.DoNotEmail).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
        }
    }
}