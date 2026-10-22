using VAA.EAPI.Domain.Entities.Common;
namespace VAA.EAPI.Microservices.Loyalty.Vred.Domain.CustomerDetail
{
    public class UpdateVredCustomerDetailsQueryResponse
    {
        public string Id { get; set; }
        public string ProcessingStatus { get; set; }
        public string JsonResponseData { get; set; }
        public string Type { get; set; }
    }
}
