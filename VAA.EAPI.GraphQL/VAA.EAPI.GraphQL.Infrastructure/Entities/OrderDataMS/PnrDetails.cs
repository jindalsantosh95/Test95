using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.OrderDataMS
{
    public class PnrDetails
    {
        public IEnumerable<PassengerDetails> Passengers { get; set; }
        public IEnumerable<FrequentFlyerDetails> FrequentFlyer { get; set; }
    }
}
