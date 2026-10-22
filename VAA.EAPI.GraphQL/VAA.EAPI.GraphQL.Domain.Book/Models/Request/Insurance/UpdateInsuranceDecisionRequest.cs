namespace VAA.EAPI.GraphQL.Domain.Book.Models.Request.Insurance
{
    public class UpdateInsuranceDecisionRequest
    {
        public string BasketId { get; set; }
        public bool InsuranceSelected { get; set; }
    }
}
