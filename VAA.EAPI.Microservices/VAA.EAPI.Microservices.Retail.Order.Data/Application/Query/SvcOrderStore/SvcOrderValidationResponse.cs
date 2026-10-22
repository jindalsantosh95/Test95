namespace VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore
{
    public sealed class SvcOrderValidationResponse
    {
        public bool Pnr { get; set; }
        public bool IsGroup { get; set; }
        public bool FirstName { get; set; }
        public bool Surname { get; set; }
    }
}
