using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Validations
{
    public class TransactionStatementTextRequestValidator : AbstractValidator<TransactionStatementTextQuery>
    {
        public TransactionStatementTextRequestValidator()
        {
            RuleFor(x => x.StatementDescriptionRequest.TransactionType).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.StatementDescriptionRequest.TransactionSubType).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.StatementDescriptionRequest.ProductName).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
