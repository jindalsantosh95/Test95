using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.AddressUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Validations
{
    public class AddressUpdateCommandValidator : AbstractValidator<AddressUpdateNotificationEventData>
    {
        public AddressUpdateCommandValidator()
        {
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.CustomerId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
    }
}
