using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public partial class RepriceOffersRequest : IEquatable<RepriceOffersRequest>, IValidatableObject
    {
        public RepriceOffersRequest(IEnumerable<OfferIDSummary> Offers = default(IEnumerable<OfferIDSummary>), POS POS = default(POS))
        {
            this.Offers = Offers;
            this.POS = POS;
            this.Parties = Parties;
        }
        public POS POS { get; set; }
        public IEnumerable<OfferIDSummary> Offers { get; set; }
        public NDCParties Parties { get; set; }

        public override bool Equals(object input)
        {
            return this.Equals(input as RepriceOffersRequest);
        }
        public bool Equals(RepriceOffersRequest input)
        {
            if (input == null)
                return false;

            return
                (
                    this.Offers == input.Offers ||
                    this.Offers != null &&
                    this.Offers.SequenceEqual(input.Offers)
                );
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Offers != null)
                    hashCode = hashCode * 59 + this.Offers.GetHashCode();
                return hashCode;
            }
        }
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
