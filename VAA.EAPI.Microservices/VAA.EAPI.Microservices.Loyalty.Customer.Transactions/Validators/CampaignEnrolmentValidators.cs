using FluentValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CampaignEnrolmentDoc;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Validators
{
    public class CampaignEnrolmentCreateValidator : AbstractValidator<CampaignEnrolment>
    {
        public CampaignEnrolmentCreateValidator()
        {
        }
    }
}
