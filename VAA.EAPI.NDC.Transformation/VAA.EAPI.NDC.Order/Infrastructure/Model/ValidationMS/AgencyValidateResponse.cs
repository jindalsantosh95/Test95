namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS
{
    public class AgencyValidateResponse
    {
        public AgencyDetails AgencyDetails { get; set; }
        public AgencyPermissions AgencyPermissions { get; set; }
        public AggregatorDetails AggregatorDetails { get; set; }
        public CorporateDetails CorporateDetails { get; set; }
        public string Environment { get; set; }
        public string KeyStatus { get; set; }
        public string KeyType { get; set; }
        public bool OCNFlag { get; set; }
    }
}
