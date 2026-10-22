namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.BasketMS
{
    public class ContactUpdateRequest
    {
        public string BasketId { get; set; }
        public ContactDetails ContactDetails { get; set; }
        public bool? FcAutoMatched { get; set; }
    }
}
