using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.NDC.Sabre.Models;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class SSRRequestMapper
    {
        public static SpecialServiceInfo SpecialServiceReqMapper(IEnumerable<SpecialServiceRequestInfo> ssrReq, IEnumerable<Customer> customers)
        {
            return new()
            {
                Service = GetSSRs(ssrReq),
                AdvancePassenger = GetAdvancePassengerInfo(customers),
                SecureFlight = null
            };
        }
        private static IEnumerable<Service> GetSSRs(IEnumerable<SpecialServiceRequestInfo> ssrReq)
        {
            return ssrReq.Select(x => new Service()
            {
                SSRCode = x.RequestCode,
                SegmentNumber = x.SegmentNumber,
                Text = x.FreeformText,
                PersonName = new() { NameNumber = $"{x.FirstNameNumber.TrimStart('0')}-{x.LastNameNumber.TrimStart('0')}", GivenName = x.FirstName, Surname = x.LastName }
            });
        }

        private static IEnumerable<AdvancePassenger> GetAdvancePassengerInfo(IEnumerable<Customer> customers)
        {
            return customers.Select(x => new AdvancePassenger()
            {
                ResidentDestinationAddress = new() { Country = x.Addresses?.Country, City = x.Addresses?.City, Zip = x.Addresses?.PostalCode, Street = x.Addresses?.AddressLine1 },
                PersonName = { NameNumber = $"{x.FirstNameNumber.TrimStart('0')}-{x.LastNameNumber.TrimStart('0')}", GivenName = x.FirstName, Surname = x.LastName, DateOfBirth = x.DateOfBirth, Gender = x.Gender, PassengerType = x.Ptc }
            });
        }

    }
}
