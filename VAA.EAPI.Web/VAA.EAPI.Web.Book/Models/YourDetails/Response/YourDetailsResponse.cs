using System.Collections.Generic;
using VAA.EAPI.Web.Book.Models.Book.Confirmation;

namespace VAA.EAPI.Web.Book.Models.YourDetails.Response
{
    public class YourDetailsResponse
    {
        public string Link { get; set; }
        public string Validation { get; set; }
        public ConfigurationDetails ConfigurationDetails { get; set; }
        public IEnumerable<Person> People { get; set; }
        public AddressDetails AddressDetails { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public TermsAndConditions TermsAndConditions { get; set; }
        public MembershipDetails MembershipDetails { get; set; }
        public VoucherDetails VoucherDetails { get; set; }
        public MilesDetails MilesDetails { get; set; }
        public DriverDetails DriverDetails { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public AffinityPartnerDetail AffinityPartnerDetail { get; set; }
        public PromotionalCodeDetails PromotionalCodeDetails { get; set; }
        public UpsellOptions UpsellOptions { get; set; }
        public Pennies Pennies { get; set; }
        public DonationDetails DonationDetails { get; set; }
        public SectorDetails SectorDetails { get; set; }
        public IEnumerable<string> Errata { get; set; }
        public Summary Summary { get; set; }
        public string ResourceType { get; set; }
        public string PaymentLink { get; set; }
        public CustomerProfile CustomerProfile { get; set; }
        public FCEarnPoints EarnPoints { get; set; }
        public bool? IsInsuranceAdded { get; set; }
        public PointOfSale? PointOfSale { get; set; }
        public bool IsCoBrandEligible { get; set; }
        public string CoBrandStatus { get; set; }
        public bool? FcAutoMatched { get; set; }
    }
}