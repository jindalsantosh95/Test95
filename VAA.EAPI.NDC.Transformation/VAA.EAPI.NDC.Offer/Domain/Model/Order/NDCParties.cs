using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class NDCParties
    {
        public NDCParty Sender { get; set; }
        public IEnumerable<NDCParty> Participants { get; set; }

    }
}
