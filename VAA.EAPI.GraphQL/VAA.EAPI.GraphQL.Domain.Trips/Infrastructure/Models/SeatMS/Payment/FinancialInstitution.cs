
namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.SeatMS.Payment
{
    public class FinancialInstitution
    {
        public string AccountHolderName { get; set; }
        public string ProviderId { get; set; }
        public string ReturnUrl { get; set; }
        public PaymentMS.Customer.Address Address { get; set; }

        public static explicit operator FinancialInstitution(PaymentMS.PaymentDeails.FinancialInstitution source)
        {
            if (source == null) return null;

            return new FinancialInstitution
            {
                AccountHolderName = source.AccountHolderName,
                ProviderId = source.ProviderId,
                ReturnUrl = source.ReturnUrl,
                Address = source.Address
            };
        }
    }

}