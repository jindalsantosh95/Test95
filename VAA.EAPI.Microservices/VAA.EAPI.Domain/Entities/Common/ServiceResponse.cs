
namespace VAA.EAPI.Domain.Entities.Common
{
    public class ServiceResponse
    {
        public string Id { get; set; }
        public ServiceResponseProcessStatus Status { get; set; }
        public string JsonResponseData { get; set; }
        public string Type { get; set; }

    }
}
