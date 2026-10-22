
using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.FlightSummary
{
    [GraphQLName("SummaryPassengerBaggageAllowance")]
    public class PassengerBaggageAllowance
    {
        public FreeBaggageAllowance? FreeBaggageAllowance { get; set; }
        public IEnumerable<PaidBaggage>? PaidBaggage { get; set; }


    }
}
