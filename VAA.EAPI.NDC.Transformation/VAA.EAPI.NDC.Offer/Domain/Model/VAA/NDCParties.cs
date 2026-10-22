using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.NDC.Offer.Domain.RequestResponseModels.SeatAvailabilityRQ;

namespace VAA.EAPI.NDC.Offer.Domain.Model.VAA
{
    public class NDCParties
    {
        public NDCParty Sender { get; set; }
        public IEnumerable<NDCParty> Participants { get; set; }

        public static explicit operator NDCParties(DistributionChainLinkType[] source)
        {
            if (source.Length == 1)
            {
                return new NDCParties()
                {
                    Sender = (NDCParty)source.FirstOrDefault()
                };
            }
            else if (source.Length > 1)
            {
                return new NDCParties()
                {
                    Sender = (NDCParty)source.LastOrDefault(),
                    Participants = new NDCParty[] { (NDCParty)source.FirstOrDefault() },
                };
            }

            return null;
        }
    }
}
