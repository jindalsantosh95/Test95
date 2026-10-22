namespace VAA.EAPI.Microservices.Retail.Order.IFG.Payment.Infrastructure.Models
{
    public class AdditionalSalesInformation
    {
        public string Fare { get; set; }
        public string Total { get; set; }
        public string FareCalculationModeIndicator { get; set; }
        public string FareCalculationPricingIndicator { get; set; }
        public string FareCalculationArea1 { get; set; }
        public string FareCalulationArea2 { get; set; }
        public string OriginalTicketDocumentNumber { get; set; }
        public string OriginalIssueLocationCityCode { get;set; }
        public string OriginalIssueDate { get; set; }
        public string OriginalIssueAgentNumericCode { get; set; }
    }
}
