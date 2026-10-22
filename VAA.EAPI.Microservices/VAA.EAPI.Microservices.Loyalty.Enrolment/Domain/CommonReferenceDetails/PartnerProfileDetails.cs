using System;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails
{
    public class PartnerProfileDetails
    {
        public string RowId { get; set; }
        public string SourceIdentifier { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string AccountTypeCode { get; set; }
        public string CurrencyCode { get; set; }
        public decimal? MaxTransactionLimitForPoints { get; set; }
        public string Competitor { get; set; }
        public string AccountStatus { get; set; }
        public string Description { get; set; }
        public string DisableMailings { get; set; }
        public string AutomatedRetroPartner { get; set; }
        public string InvestorFlag { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerType { get; set; }
        public string MasterAccountId { get; set; }
        public string Name { get; set; }
        public string PartnerDivisionId { get; set; }
        public string PartnerAccountId { get; set; }
        public DateTime? PartnershipStartDate { get; set; }
        public string PartnerCategory { get; set; }
        public string ShippingAddress { get; set; }
        public string TotalPotentialVolumeCurrencyCode { get; set; }
        public string TotalPotentialVolumeExchangeDate { get; set; }
        public decimal? TotalPotentialVolume { get; set; }
        public string ReferenceFlag { get; set; }
        public string PartnerTerritoryCode { get; set; }
        public string PartnerGroup { get; set; }
        public string VaautomatedRetroPartner { get; set; }
        public string ActiveFlag { get; set; }
        public string AuthorisedToClaimDeductions { get; set; }
        public string AccountGoodStandingFlag { get; set; }
        public string AgencyFlag { get; set; }
        public string BriefingLayout { get; set; }
        public string EnterpriseFlag { get; set; }
        public DateTime? CustomerSinceDate { get; set; }
        public string AuthorizedDistributionListStatus { get; set; }
        public string BrancyTypeCode { get; set; }
        public string CountryRegion { get; set; }
        public string DedupToken { get; set; }
        public string FreightTermsInformation { get; set; }
    }
}
