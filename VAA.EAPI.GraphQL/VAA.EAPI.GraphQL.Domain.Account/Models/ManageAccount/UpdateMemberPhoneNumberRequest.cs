namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount
{
    public class UpdateMemberPhoneNumberRequest
    {
        public string CustomerID { get; set; }
        public string PhoneNumber { get; set; }
        public string CountryCode { get; set; }
        public string FcNumber { get; set; }
    }
}
