using HotChocolate;
using VAA.EAPI.GraphQL.Domain.Trips.Models.ReferenceData;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Upsell
{
    [GraphQLName("Upsell_UpsellOffer")]
    public class UpsellOffer
    {
        public string OfferItemNumber { get; set; }
        public CabinClass CabinClass { get; set; }
        public IEnumerable<UpsellOfferDetail> UpsellOfferDetails { get; set; }
        public int OfferPriority { get; set; }
        public int DisplayPriority { get; set; }
        public int AvailableSeatsRemaining { get; set; }


        public static explicit operator UpsellOffer((Infrastructure.Models.UpsellOffer.UpsellOffer source, IEnumerable<CabinRefData> cabinRefData) data)
        {
            var (source, cabinRefData) = data;
            if (source == null) return null;

            return new UpsellOffer()
            {
                OfferItemNumber = source.OfferItemNumber,
                CabinClass = (CabinClass)(source.CabinClass, cabinRefData),
                UpsellOfferDetails = source.UpsellOfferDetails?.Select(x => (UpsellOfferDetail)x),
                OfferPriority = source.OfferPriority,
                DisplayPriority = source.DisplayPriority,
                AvailableSeatsRemaining = source.AvailableSeatsRemaining
            };
        }
    }
}