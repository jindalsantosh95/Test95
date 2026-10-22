using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public partial class ProductOffersRequestTravellersKnownTravellers : IEquatable<ProductOffersRequestTravellersKnownTravellers>, IValidatableObject
    {
        public ProductOffersRequestTravellersKnownTravellers(Customer Traveller = default(Customer), ProgrammeQualifier ProgrammeQualifier = default(ProgrammeQualifier))
        {
            this.Traveller = Traveller;
            this.ProgrammeQualifier = ProgrammeQualifier;
        }
        public Customer Traveller { get; set; }
        public ProgrammeQualifier ProgrammeQualifier { get; set; }

        public override bool Equals(object input)
        {
            return this.Equals(input as ProductOffersRequestTravellersKnownTravellers);
        }
        public bool Equals(ProductOffersRequestTravellersKnownTravellers input)
        {
            if (input == null)
                return false;

            return
                (
                    this.Traveller == input.Traveller ||
                    (this.Traveller != null &&
                    this.Traveller.Equals(input.Traveller))
                ) &&
                (
                    this.ProgrammeQualifier == input.ProgrammeQualifier ||
                    (this.ProgrammeQualifier != null &&
                    this.ProgrammeQualifier.Equals(input.ProgrammeQualifier))
                );
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.Traveller != null)
                    hashCode = hashCode * 59 + this.Traveller.GetHashCode();
                if (this.ProgrammeQualifier != null)
                    hashCode = hashCode * 59 + this.ProgrammeQualifier.GetHashCode();
                return hashCode;
            }
        }
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
