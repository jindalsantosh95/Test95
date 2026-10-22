using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Validators
{
    public class CardLinkingAuditValidator : AbstractValidator<CardLinkingAudit>
    {
        public CardLinkingAuditValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.CardLinkingDocID).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.StatusEnhancementCode).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.PartnerCode).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.StartDate).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
        }
    }
}
