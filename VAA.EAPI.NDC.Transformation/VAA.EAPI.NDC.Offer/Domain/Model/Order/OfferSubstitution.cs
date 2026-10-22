
using System;
using System.Linq;
using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public partial class OfferSubstitution : IEquatable<OfferSubstitution>
    {
        public IEnumerable<OriginDestinationRequest> OriginDestinations { get; set; }
        public IEnumerable<OriginDestinationRequestCabinType> CabinType { get; set; }
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((OfferSubstitution)obj);
        }
        public bool Equals(OfferSubstitution other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    OriginDestinations == other.OriginDestinations ||
                    OriginDestinations != null &&
                    OriginDestinations.SequenceEqual(other.OriginDestinations)
                );
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                if (OriginDestinations != null)
                    hashCode = hashCode * 59 + OriginDestinations.GetHashCode();
                return hashCode;
            }
        }


    }
}
