using FluentValidation;
using System;
using System.Net.Http;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class CreateAccrualTransactionValidator : AbstractValidator<CreateTransactionMutationRequest>
    {
        public CreateAccrualTransactionValidator(HttpMethod verb)
        {
            if (verb == HttpMethod.Post)
            {
                RuleFor(x => x.MemberNumber).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.CustomerId));
                RuleFor(x => x.CustomerId).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.MemberNumber));
                RuleFor(x => x.ActivityDate).LessThanOrEqualTo(DateTime.Today).WithMessage("Activity Date cannot be greater than current date for Accrual");
                RuleFor(x => x.TransactionSource).NotNull().NotEmpty();
                RuleFor(x => x.CreatedBy).NotNull().NotEmpty();
                RuleFor(x => x.PartnerCode).NotNull().NotEmpty();
                RuleFor(x => x.ProductName).NotNull().NotEmpty();
            }
        }
    }
}

