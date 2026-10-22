namespace VAA.EAPI.NDC.Order.Infrastructure.Model.ValidationMS
{
    public class AgencyPermissions
    {
        public bool? BspActiveStatusInd { get; set; }
        public bool? PciDssComplianceInd { get; set; }
        public bool? CardFopAllowedInd { get; set; }
        public bool? CashFopAllowedInd { get; set; }
        public bool? EasyPayFopAllowedInd { get; set; }
        public bool? AirlineTicketingAuthorityInd { get; set; }
    }
}
