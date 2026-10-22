using HotChocolate;

namespace VAA.EAPI.GraphQL.Domain.WebForms.Models
{
    [GraphQLDescription(" Webform Mutation Payment request")]
    public class Payment
    {
        [GraphQLDescription("Unique Payment Number against a Case registered in CRM")]
        public string PaymentNumber { get; set; }

        [GraphQLDescription("Bank acount country name")]
        public string BankCountryName { get; set; }
        
        [GraphQLDescription("Bank acount sort code")]
        public string SortCode { get; set; }
        
        [GraphQLDescription("Bank acount name")]
        public string BankName { get; set; }
        
        [GraphQLDescription("Bank acount number")]
        public string AccountNumber { get; set; }
        
        [GraphQLDescription("Bank acount IBAN number")]
        public string Iban { get; set; }
        
        [GraphQLDescription("Bank account Payee address")]
        public string PayeeAddress { get; set; }
        
        [GraphQLDescription("Bank acount routing code")]
        public string RoutingCode { get; set; }
        
        [GraphQLDescription("Bank acount IFSC code")]
        public string IfscCode { get; set; }
        
        [GraphQLDescription("Bank acount holder name")]
        public string AccountHolderName { get; set; }
        
        [GraphQLDescription("Bank acount swift code")]
        public string SwiftCode { get; set; }
        
        [GraphQLDescription("Bank acount clabe number")]
        public string ClabeNumber { get; set; }
        
        [GraphQLDescription("Bank code number")]
        public string BankCodeNumber { get; set; }
        
        [GraphQLDescription("Street")]
        public string Street { get; set; }
        
        [GraphQLDescription("Town")]
        public string Town { get; set; }
        
        [GraphQLDescription("State")]
        public string State { get; set; }
        
        [GraphQLDescription("Country")]
        public string Country { get; set; }
        
        [GraphQLDescription("Post Code")]
        public string PostCode { get; set; }
        
        [GraphQLDescription("Account Suffix")]
        public string AccountSuffix { get; set; }
    }
}
