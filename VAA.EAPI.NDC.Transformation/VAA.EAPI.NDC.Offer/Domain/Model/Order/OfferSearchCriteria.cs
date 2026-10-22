using System;
using System.Linq;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public partial class OfferSearchCriteria : IEquatable<OfferSearchCriteria>, IValidatableObject
    {
        public OfferSearchCriteria(string CreationDateRange = default(string), string POSAgency = default(string), string POSIATANo = default(string), string GroupName = default(string), string Services = default(string), string AccountableDocument = default(string), string Payments = default(string), string TravelDateRange = default(string), string FlightOrigin = default(string), string FlightDestination = default(string), string FlightNumber = default(string), string FareBasisCode = default(string), IEnumerable<string> ProductCode = default(IEnumerable<string>), string ClassOfService = default(string), IEnumerable<Customer> Travellers = default(IEnumerable<Customer>))
        {
            this.CreationDateRange = CreationDateRange;
            this.POSAgency = POSAgency;
            this.POSIATANo = POSIATANo;
            this.GroupName = GroupName;
            this.Services = Services;
            this.AccountableDocument = AccountableDocument;
            this.Payments = Payments;
            this.TravelDateRange = TravelDateRange;
            this.FlightOrigin = FlightOrigin;
            this.FlightDestination = FlightDestination;
            this.FlightNumber = FlightNumber;
            this.FareBasisCode = FareBasisCode;
            this.ProductCode = ProductCode;
            this.ClassOfService = ClassOfService;
            this.Travellers = Travellers;
        }
        public string CreationDateRange { get; set; }
        public string POSAgency { get; set; }
        public string POSIATANo { get; set; }
        public string GroupName { get; set; }
        public string Services { get; set; }
        public string AccountableDocument { get; set; }
        public string Payments { get; set; }
        public string TravelDateRange { get; set; }
        public string FlightOrigin { get; set; }
        public string FlightDestination { get; set; }
        public string FlightNumber { get; set; }
        public string FareBasisCode { get; set; }
        public IEnumerable<string> ProductCode { get; set; }
        public string ClassOfService { get; set; }
        public IEnumerable<Customer> Travellers { get; set; }

        public override bool Equals(object input)
        {
            return this.Equals(input as OfferSearchCriteria);
        }
        public bool Equals(OfferSearchCriteria input)
        {
            if (input == null)
                return false;

            return
                (
                    this.CreationDateRange == input.CreationDateRange ||
                    (this.CreationDateRange != null &&
                    this.CreationDateRange.Equals(input.CreationDateRange))
                ) &&
                (
                    this.POSAgency == input.POSAgency ||
                    (this.POSAgency != null &&
                    this.POSAgency.Equals(input.POSAgency))
                ) &&
                (
                    this.POSIATANo == input.POSIATANo ||
                    (this.POSIATANo != null &&
                    this.POSIATANo.Equals(input.POSIATANo))
                ) &&
                (
                    this.GroupName == input.GroupName ||
                    (this.GroupName != null &&
                    this.GroupName.Equals(input.GroupName))
                ) &&
                (
                    this.Services == input.Services ||
                    (this.Services != null &&
                    this.Services.Equals(input.Services))
                ) &&
                (
                    this.AccountableDocument == input.AccountableDocument ||
                    (this.AccountableDocument != null &&
                    this.AccountableDocument.Equals(input.AccountableDocument))
                ) &&
                (
                    this.Payments == input.Payments ||
                    (this.Payments != null &&
                    this.Payments.Equals(input.Payments))
                ) &&
                (
                    this.TravelDateRange == input.TravelDateRange ||
                    (this.TravelDateRange != null &&
                    this.TravelDateRange.Equals(input.TravelDateRange))
                ) &&
                (
                    this.FlightOrigin == input.FlightOrigin ||
                    (this.FlightOrigin != null &&
                    this.FlightOrigin.Equals(input.FlightOrigin))
                ) &&
                (
                    this.FlightDestination == input.FlightDestination ||
                    (this.FlightDestination != null &&
                    this.FlightDestination.Equals(input.FlightDestination))
                ) &&
                (
                    this.FlightNumber == input.FlightNumber ||
                    (this.FlightNumber != null &&
                    this.FlightNumber.Equals(input.FlightNumber))
                ) &&
                (
                    this.FareBasisCode == input.FareBasisCode ||
                    (this.FareBasisCode != null &&
                    this.FareBasisCode.Equals(input.FareBasisCode))
                ) &&
                (
                    this.ProductCode == input.ProductCode ||
                    this.ProductCode != null &&
                    this.ProductCode.SequenceEqual(input.ProductCode)
                ) &&
                (
                    this.ClassOfService == input.ClassOfService ||
                    (this.ClassOfService != null &&
                    this.ClassOfService.Equals(input.ClassOfService))
                ) &&
                (
                    this.Travellers == input.Travellers ||
                    this.Travellers != null &&
                    this.Travellers.SequenceEqual(input.Travellers)
                );
        }
        public override int GetHashCode()
        {
            unchecked // Overflow is fine, just wrap
            {
                int hashCode = 41;
                if (this.CreationDateRange != null)
                    hashCode = hashCode * 59 + this.CreationDateRange.GetHashCode();
                if (this.POSAgency != null)
                    hashCode = hashCode * 59 + this.POSAgency.GetHashCode();
                if (this.POSIATANo != null)
                    hashCode = hashCode * 59 + this.POSIATANo.GetHashCode();
                if (this.GroupName != null)
                    hashCode = hashCode * 59 + this.GroupName.GetHashCode();
                if (this.Services != null)
                    hashCode = hashCode * 59 + this.Services.GetHashCode();
                if (this.AccountableDocument != null)
                    hashCode = hashCode * 59 + this.AccountableDocument.GetHashCode();
                if (this.Payments != null)
                    hashCode = hashCode * 59 + this.Payments.GetHashCode();
                if (this.TravelDateRange != null)
                    hashCode = hashCode * 59 + this.TravelDateRange.GetHashCode();
                if (this.FlightOrigin != null)
                    hashCode = hashCode * 59 + this.FlightOrigin.GetHashCode();
                if (this.FlightDestination != null)
                    hashCode = hashCode * 59 + this.FlightDestination.GetHashCode();
                if (this.FlightNumber != null)
                    hashCode = hashCode * 59 + this.FlightNumber.GetHashCode();
                if (this.FareBasisCode != null)
                    hashCode = hashCode * 59 + this.FareBasisCode.GetHashCode();
                if (this.ProductCode != null)
                    hashCode = hashCode * 59 + this.ProductCode.GetHashCode();
                if (this.ClassOfService != null)
                    hashCode = hashCode * 59 + this.ClassOfService.GetHashCode();
                if (this.Travellers != null)
                    hashCode = hashCode * 59 + this.Travellers.GetHashCode();
                return hashCode;
            }
        }
        IEnumerable<System.ComponentModel.DataAnnotations.ValidationResult> IValidatableObject.Validate(ValidationContext validationContext)
        {
            yield break;
        }
    }

}
