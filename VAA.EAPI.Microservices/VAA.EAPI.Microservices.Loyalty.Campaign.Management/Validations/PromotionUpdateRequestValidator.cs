using FluentValidation;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.PromotionUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class PromotionUpdateRequestValidator : AbstractValidator<PromotionUpdateRequestData>
    {

        public PromotionUpdateRequestValidator()
        {
            RuleFor(x => x.PromotionId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.PromotionName).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.PartnerName).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.StartDate).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing).Must(BeAValidDate).WithMessage(Constants.ErrorCodes.InvalidDateFormat);
            RuleFor(x => x.EndDate).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing).Must(BeAValidDate).WithMessage(Constants.ErrorCodes.InvalidDateFormat);
            RuleFor(x => x.Active).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
        private bool BeAValidDate(string value)
        {
            return DateTime.TryParse(value, out _);
        }
    }
}
