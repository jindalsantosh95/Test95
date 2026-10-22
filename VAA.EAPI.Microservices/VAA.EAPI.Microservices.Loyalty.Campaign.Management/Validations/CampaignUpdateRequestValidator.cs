using FluentValidation;
using System;
using VAA.EAPI.Microservices.Loyalty.Campaign.Management.Domain.Entities.CampaignUpdate;

namespace VAA.EAPI.Microservices.Loyalty.Campaign.Management.Validations
{
    public class CampaignUpdateRequestValidator : AbstractValidator<CampaignUpdateRequestData>
    {
        public CampaignUpdateRequestValidator()
        {
            RuleFor(x => x.CampaignId).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.CampaignCode).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.CampaignName).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.StartDate).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing).Must(BeAValidDate).WithMessage(Constants.ErrorCodes.InvalidDateFormat);
            RuleFor(x => x.EndDate).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing).Must(BeAValidDate).WithMessage(Constants.ErrorCodes.InvalidDateFormat);
            RuleFor(x => x.Type).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            RuleFor(x => x.InitiatedBy).NotNull().NotEmpty().WithMessage(Constants.ErrorCodes.MandatoryInputFieldsMissing);
        }
        private bool BeAValidDate(string value)
        {
            return DateTime.TryParse(value, out _);
        }
    }
}
