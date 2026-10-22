using VAA.EAPI.GraphQL.Common.Serialisation;

namespace VAA.EAPI.GraphQL.Domain.Search.Models.OfferSearch.Offers.Request
{
    public class OfferSearchRequest : IEquatable<OfferSearchRequest>
    {
        public IEnumerable<Slice> Slices { get; set; }
        public IEnumerable<string> Passengers { get; set; }
        public IEnumerable<string> CabinClasses { get; set; }

        public bool Equals(OfferSearchRequest? other)
        {
            if (other is null)
                return false;

            return GetHashCode() == other.GetHashCode();
        }

        public override int GetHashCode()
        {
            return Json.Serialize(this).GetHashCode();
        }
    }


}
