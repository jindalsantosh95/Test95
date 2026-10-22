using VAA.EAPI.Automation.MarketingLoyalty.Requests.Events;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.GraphQL;

namespace VAA.EAPI.Automation.MarketingLoyalty.Requests.Mapper
{
    public class SupplementarySilverDissolveEventOnTierDowngrade
    {

        public static CloudEvent MapSupplementarySilverDissolveEvent(SupplementarySilverDissolve suppSilverDetails, string serviceBusTopicName)
        {
            return new CloudEvent
            {
                Specversion = Constants.Application.SpecVersion,
                Type = Constants.Application.DissolveSuppSilverAccountOnTierDowngrade,
                Source = serviceBusTopicName,
                Id = Guid.NewGuid().ToString(),
                Originator = null,
                Time = DateTime.Now.ToString(Constants.Application.CloudEventTimeFormat),
                Subject = Constants.Application.DissolveSuppSilverAccountOnTierDowngrade,
                Dataschema = null,
                DataContentType = Constants.Application.DataContentType,
                Data = suppSilverDetails
            };
        }


        public static SupplementarySilverDissolve SupplementarySilverDissolveDetails(string customerId, string initiatedBy, string oldTier, string newTier)
        {
            return new SupplementarySilverDissolve
            {

                CustomerId = customerId,
                InitiatedBy = initiatedBy,
                OldTier = oldTier,
                NewTier = newTier

            };
        }
    }
}
