namespace VAA.EAPI.Microservices.Loyalty.Identity.Domain.Customer
{
    public class AccountCustomerAttribute
    {
        public enum AccountCustomerAttributeTypes
        {
            IphoneApp,
            TwitterHandle,
            VaFacebookApp
        }
        public AccountCustomerAttributeTypes? Type { get; set; }
        public string Value { get; set; }
    }
}
