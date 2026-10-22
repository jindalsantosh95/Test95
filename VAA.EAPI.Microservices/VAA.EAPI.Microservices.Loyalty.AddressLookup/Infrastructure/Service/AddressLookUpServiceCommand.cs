using MediatR;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service
{
    public class AddressLookUpServiceCommand : IRequest<CompleteAddressLookupResponse>
    {
        public string CompleteAddressLookUpRequest { get; set; }
        public AddressLookUpServiceCommand(string completeAddressLookUpRequest)
        {
            CompleteAddressLookUpRequest = completeAddressLookUpRequest;
        }
    }
}
