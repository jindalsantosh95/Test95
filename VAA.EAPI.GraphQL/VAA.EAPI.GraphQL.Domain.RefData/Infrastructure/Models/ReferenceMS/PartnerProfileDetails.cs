namespace VAA.EAPI.GraphQL.Domain.RefData.Infrastructure.Models.ReferenceMS
{
    [RefDataType("partnerProfile", "partnerProfile")]
    public class PartnerProfileDetails
    {
        public string Name { get; set; }
        public string RowId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerType { get; set; }
        public string PartnerSubType { get; set; }
    }
}
