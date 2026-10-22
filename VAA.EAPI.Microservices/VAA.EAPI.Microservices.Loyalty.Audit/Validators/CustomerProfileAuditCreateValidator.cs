using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Validators
{
    public class CustomerProfileAuditCreateValidator : AbstractValidator<CustomerProfileAudit>
    {
        public CustomerProfileAuditCreateValidator()
        {
            RuleFor(m => m.CustomerId).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.FirstName).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
            RuleFor(x => x.LastName).NotNull().NotEmpty().WithMessage("Mandatroy Field is missing or invalid");
        }
    }
}