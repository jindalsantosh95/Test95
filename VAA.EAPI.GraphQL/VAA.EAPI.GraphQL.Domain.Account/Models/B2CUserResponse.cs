namespace VAA.EAPI.GraphQL.Domain.Account.Models
{
    public class B2CUserResponse
    {
        public string ObjectId { get; set; }
        public bool IsB2COperationSuccess { get; set; }
        public string B2CFaliureMessage { get; set; }
    }
}
