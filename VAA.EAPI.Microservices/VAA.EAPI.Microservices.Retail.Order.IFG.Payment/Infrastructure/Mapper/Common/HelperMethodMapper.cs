using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Mapper.Common
{
    public static class HelperMethodMapper
    {
        public static MerchantDefinedInformation[] GetMerchantDefinedInformation(Parties parties)
        {
            return parties.Participants.Where(p => p.PartyType == Participant.PartyTypeEnum.Seller && p.RequestAgentType == Constants.Application.IataRequestAgentType).Select(p => new[]
            {
                new MerchantDefinedInformation
                {
                    Key = "TravelAgentName",
                    Value = p.Name
                },
                new MerchantDefinedInformation
                {
                    Key = "TravelAgentEmail",
                    Value = p.Contacts.Email
                }}).FirstOrDefault();
        }
    }
}
