namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.Enrolment
{
    public class CustomerAttribute
    {
        public enum CustomerAttributeTypes
        {
            IphoneApp,
            TwitterHandle,
            VAFacebookApp
        }
        public CustomerAttributeTypes? Type { get; set; }
        public string? Value { get; set; }
    }
}
