using System.Collections.Generic;
using VAA.EAPI.Microservices.Flight.Services.Models.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR
{
    public class SpecialServiceRequest
    {
        public string Code { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
        public string RecordNumber { get; set; }
        public bool Selected { get; set; }
        public bool Eligible { get; set; }
        public bool AllowChanges { get; set; }
        public IEnumerable<string> Remarks { get; set; }
        public string SsrGroupCode { get; set; }
        public IEnumerable<string> AvailableOptions { get; set; }

        public static explicit operator SpecialServiceRequest(ServiceRequest source)
        {
            if (source == null)
                return null;

            return new SpecialServiceRequest
            {
                Code = source.ServiceTypeCode,
                Status = source.ServiceStatus
            };
        }
    }
}
