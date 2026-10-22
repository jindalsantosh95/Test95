using VAA.EAPI.Microservices.Retail.Trip.Domain.Enum;

namespace VAA.EAPI.Microservices.Retail.Trip.Domain.DeepLink
{
    public class DeeplinkResponse
    {
        public string? DeeplinkUrl { get; set; } 
        public DeeplinkOption DeeplinkOption { get; set; }
    }
}
