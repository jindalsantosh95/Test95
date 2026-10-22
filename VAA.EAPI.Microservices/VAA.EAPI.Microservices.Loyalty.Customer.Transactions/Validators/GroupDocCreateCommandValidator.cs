using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.GroupDoc;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class GroupDocCreateCommandValidator : AbstractValidator<GroupDocCommandBase>
    {
        public GroupDocCreateCommandValidator()
        {
            RuleFor(x => x.GroupDoc)
                .NotNull()
                .WithMessage(Constants.ErrorCodes.GroupDocumentNotFoundMessage);
        }
    }
}
