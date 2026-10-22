namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class CustomerAttribute
    {
        public CustomerAttribute(string type = default, string value = default)
        {
            Type = type;
            Value = value;
        }

        public string Type { get; set; }
        public string Value { get; set; }
    }
}
