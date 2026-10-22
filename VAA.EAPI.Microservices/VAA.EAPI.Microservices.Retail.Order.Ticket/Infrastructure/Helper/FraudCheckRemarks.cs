namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Helper
{
    public class FraudCheckRemarks
    {
        public static string PnrRemarks(string tokenizedCard, string cardType)
        {
            return Constants.Application.FraudCheckRemarks + cardType + tokenizedCard.Substring(tokenizedCard.Length - 4).PadLeft(12, '*');
        }
    }
}