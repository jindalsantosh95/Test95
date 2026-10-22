using HotChocolate;
using VAA.EAPI.GraphQL.Common.Middleware;

namespace VAA.EAPI.GraphQL.Domain.Operations.Models.PassengerManifest
{
    [VAAFilterConvention(filterName: "PassengerManifestServiceFilter")]
    [GraphQLName("PassengerManifestService")]
    public class Service
    {
        public string ProductType { get; set; }
        public string ServiceType { get; set; }
        public string ServiceCode { get; set; }
        public string ServiceName { get; set; }
        public string ServiceDescription { get; set; }
        public string ServiceStatus { get; set; }
        public string Remarks { get; set; }

        public static explicit operator Service(DTOs.PassengerManifest_SCVv7.Service source)
        {
            return new Service
            {
                ProductType = source.ProductType,
                ServiceType = source.ServiceType,
                ServiceCode = source.ServiceCode,
                ServiceName = source.ServiceName,
                ServiceDescription = source.ServiceDescription,
                ServiceStatus = source.ServiceStatus,
                Remarks = source.Remarks

            };
        }

    }
}
