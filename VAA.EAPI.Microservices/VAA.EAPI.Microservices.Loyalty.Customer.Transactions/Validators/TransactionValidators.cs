using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class TransactionCreateValidator : AbstractValidator<TransactionDocument>
    {
        public TransactionCreateValidator()
        {
            Include(new TransactionProductIDValidator());
            Include(new TransactionPartnerIDValidator());
        }
    }

    public class TransactionProductIDValidator : AbstractValidator<TransactionDocument>
    {
        public TransactionProductIDValidator()
        {
            RuleFor(x => x.ProductId).NotNull().NotEmpty();
        }
    }

    public class TransactionPartnerIDValidator : AbstractValidator<TransactionDocument>
    {
        public TransactionPartnerIDValidator()
        {
            RuleFor(x => x.PartnerId).NotNull().NotEmpty();
        }
    }

    public class TransactionCancelValidator : AbstractValidator<TransactionDocument>
    {
        public TransactionCancelValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty();
            RuleFor(x => x.TransactionId).NotNull().NotEmpty();
            RuleFor(x => x.CancellationReasonCode).NotNull().NotEmpty();
        }
    }

    public class TransactionUpdateValidator : AbstractValidator<TransactionDocument>
    {
        public TransactionUpdateValidator()
        {
            RuleFor(x => x.CustomerId).NotNull().NotEmpty();
            RuleFor(x => x.TransactionId).NotNull().NotEmpty();
            RuleFor(x => x.StatusCode).NotNull().NotEmpty();
        }
    }
}
