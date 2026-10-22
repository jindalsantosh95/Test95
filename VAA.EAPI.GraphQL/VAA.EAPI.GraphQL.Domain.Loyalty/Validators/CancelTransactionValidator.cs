using FluentValidation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Transaction;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Validators
{
    public class CancelTransactionValidator : AbstractValidator<CancelTransactionMutationRequest>
    {
        public CancelTransactionValidator()
        {
            RuleFor(x => x.MemberNumber).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.CustomerId));
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().When(x => string.IsNullOrEmpty(x.MemberNumber));
            RuleFor(x => x.TransactionSource).NotNull().NotEmpty();
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty();
            RuleFor(x => x.CancellationReason).NotNull().NotEmpty();
            RuleFor(x => x.InitiatedTransactionId).NotNull().NotEmpty();
            RuleFor(x => x.CancelTransactionDetails).NotNull().WithMessage("Cancellation Transaction Details cannot be null");
        }
    }
}
