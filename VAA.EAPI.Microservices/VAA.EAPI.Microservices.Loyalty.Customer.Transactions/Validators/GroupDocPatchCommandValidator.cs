using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.GroupDoc;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class GroupDocPatchCommandValidator : AbstractValidator<GroupDocCommandBase>
    {
        public GroupDocPatchCommandValidator()
        {
            RuleFor(x => x.GroupDoc)
                .NotNull()
                .WithMessage(Constants.ErrorCodes.GroupDocumentNotFoundMessage);
            RuleFor(x => x.GroupDoc.GroupId)
                .NotNull().NotEmpty()
                .WithMessage(Constants.ErrorCodes.GroupIdNotFoundMessage);
        }
    }
}
