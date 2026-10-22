namespace VAA.EAPI.Automation.Search.Models.OfferSearch
{
    public class SetVoucherRedemptionSelectionGQLRequest
    {
        public string operationName { get; set; }
        public string query { get; set; }
        public SetVoucherRedemptionSelectionVariables variables { get; set; }
    }

    public class SetVoucherRedemptionSelectionVariables
    {
        public SetVoucherRedemptionSelectionRequest request { get; set; }
    }

    public class SetVoucherRedemptionSelectionRequest
    {
        public string basketId { get; set; }

        public string[] selectedVoucherIds { get; set; }
    }
}
