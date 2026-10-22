using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CampaignEnrolmentDoc;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Command.CampaignEnrolmentDoc
{
    public class CampaignEnrolmentCommandBase : TransactionBaseCommand
    {
        public CampaignEnrolment CampaignEnrolmentDocument { get; set; }
    }
}
