using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Validations
{
    public class FeedbackRequestValidator: AbstractValidator<RetroFeedbackRequest>
    {
        public FeedbackRequestValidator()
        {
            RuleFor(x => x.TrackID).NotNull().NotEmpty();
            RuleFor(m => m.UniqueIdentificationNumber).NotNull().NotEmpty();
            RuleFor(m => m.OperatingCarrierCode).NotNull().NotEmpty();
            RuleFor(x => x.FFPCarrierCode).NotNull().NotEmpty();
            RuleFor(x => x.PostingStatus).NotNull().NotEmpty();
            RuleFor(x => x.BaseMiles).NotNull().NotEmpty();
        }
    }
}
