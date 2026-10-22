namespace VAA.EAPI.GraphQL.Domain.Account.Models.Enrolment
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
        public string? Value { get; set; }
    }
}
