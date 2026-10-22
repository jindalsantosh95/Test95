using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Flight.Services.Infrastructure.Delta.SSR;

namespace VAA.EAPI.Microservices.Flight.Services.Models.SSR
{
    public class ServiceRequest
    {
        public string ServiceTypeCode { get; set; }
        public string ServiceTypeName { get; set; }
        public string ServiceStatus { get; set; }
        public bool IsSelected { get; set; }
        public bool IsEligible { get; set; }
        public string SelectedChoice { get; set; }
        public string ServiceCategory { get; set; }
        public bool CanModify { get; set; }
        public IEnumerable<string> AvailableChoices { get; set; }

        public static explicit operator ServiceRequest(SpecialServiceRequest sourceRequest)
        {
            if (sourceRequest == null)
                return null;

            return new ServiceRequest
            {
                ServiceTypeCode = sourceRequest.Code,
                ServiceTypeName = sourceRequest.Name,
                IsSelected = sourceRequest.Selected,
                IsEligible = sourceRequest.Eligible,
                CanModify = sourceRequest.AllowChanges,
                SelectedChoice = sourceRequest.Remarks != null && sourceRequest.Remarks.Any() ? sourceRequest.Remarks.First() : string.Empty,
                ServiceCategory = sourceRequest.SsrGroupCode,
                AvailableChoices = sourceRequest.AvailableOptions ?? Enumerable.Empty<string>()
            };
        }
    }
}