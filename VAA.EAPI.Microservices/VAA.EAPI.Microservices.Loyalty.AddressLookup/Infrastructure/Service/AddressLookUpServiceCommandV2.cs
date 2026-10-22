using MediatR;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service
{
    public class AddressLookUpServiceCommandV2 : IRequest<CompleteAddressLookupResponse>
    {
        public string CompleteAddressLookUpRequest { get; set; }
        public AddressLookUpServiceCommandV2(string completeAddressLookUpRequest)
        {
            CompleteAddressLookUpRequest = completeAddressLookUpRequest;
        }
    }
}
