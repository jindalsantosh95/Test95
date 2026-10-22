namespace VAA.EAPI.GraphQL.Domain.Servicing.Models.PnrOrderValidation
{
    public sealed class PnrOrderValidationRequest
    {
        public string Pnr { get; set; } = string.Empty;
        public string GivenName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
    }
}
