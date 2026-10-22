using System.Collections.Generic;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command.GbgService
{
    public interface IAddressLookupService
    {
        Task<Response<IEnumerable<AddressDetail>>> GetAllAddresses(string countryCode, string postalCode, string AddressLine);
        Task<Response<IEnumerable<AddressDetail>>> FindAddresses(string addressText, string countryCode);
        Task<Response<IEnumerable<AddressDetail>>> RetrieveAddress(string addressId);
    }
}
