using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange
{
    public class Address
    {
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string AddressLine3 { get; set; }
        public string City { get; set; }
        public string PostalCode { get; set; }
        [OpenApiProperty(Description = "Specifies the state/region/Province name for the city/postal code")]
        public string Region { get; set; }
        [OpenApiProperty(Description = "ISO country code. example GB")]
        public string Country { get; set; }
    }
}
