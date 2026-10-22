using System;
using System.Collections.Generic;
using System.Linq;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public partial class ProductOffersRequestTravellers : IEquatable<ProductOffersRequestTravellers>
    {
        public IEnumerable<ProductOffersRequestTravellersKnownTravellers> KnownTravellers { get; set; }
        public IEnumerable<AnonymousCustomer> AnonymousTravellers { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((ProductOffersRequestTravellers)obj);
        }
        public bool Equals(ProductOffersRequestTravellers other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;

            return
                (
                    KnownTravellers == other.KnownTravellers ||
                    KnownTravellers != null &&
                    KnownTravellers.SequenceEqual(other.KnownTravellers)
                ) &&
                (
                    AnonymousTravellers == other.AnonymousTravellers ||
                    AnonymousTravellers != null &&
                    AnonymousTravellers.SequenceEqual(other.AnonymousTravellers)
                );
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                var hashCode = 41;
                // Suitable nullity checks etc, of course :)
                if (KnownTravellers != null)
                    hashCode = hashCode * 59 + KnownTravellers.GetHashCode();
                if (AnonymousTravellers != null)
                    hashCode = hashCode * 59 + AnonymousTravellers.GetHashCode();
                return hashCode;
            }
        }


    }
}
