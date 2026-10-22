using VAA.EAPI.GraphQL.Domain.Search.Models.Brand;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.BFFSummary
{
    public class PassengerBaggageAllowance
    {
        public FreeBaggageAllowance? FreeBaggageAllowance { get; set; }
        public IEnumerable<PaidBaggage>? PaidBaggage { get; set; }

        public static explicit operator PassengerBaggageAllowance(BaggagePolicy? source)
        {
            if (source is null)
                return null;

            IEnumerable<PaidBaggage>? paidBaggageAllowance = source?.PaidBags?.Any() ?? false
               ? source.PaidBags.Select(b => (PaidBaggage)b)
               : null;

            return new PassengerBaggageAllowance()
            {
                FreeBaggageAllowance = (FreeBaggageAllowance?)source?.FreeBags,
                PaidBaggage = paidBaggageAllowance
            };
        }
    }
}
