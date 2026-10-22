using FluentValidation;
using System;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Vred.Applications.Commands.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Validators
{
    public class CreateTransactionRequestCommandValidator : AbstractValidator<CreateTransactionRequestCommand>
    {

        public CreateTransactionRequestCommandValidator()
        {
            RuleFor(m => m.Request).NotNull().NotEmpty().WithMessage("Request is missing or invalid");            
            RuleFor(x => x.Request.MemberNumber).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(m => m.Request.ActivityDate).NotNull().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.PartnerCode).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.PartnerName).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.ProductName).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.CreatedBy).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.TransactionChannel).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.ActivityType).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.ActivityType.ToUpper()).Must(x => new[] { Constants.Application.AccrualType, Constants.Application.RedemptionType }.Contains(x, StringComparer.OrdinalIgnoreCase)).WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.StatementText).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.TransactionSubType).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
            RuleFor(x => x.Request.Status).NotNull().NotEmpty().WithMessage("Mandatory Field is missing or invalid");
        }
    }
}