using MediatR;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository
{
    public class AddressLookUpRepoCommand : IRequest<bool>
    {
        public AddressLookUpRequest Request { get; set; }
        public string Country { get; set; }
        public string PostalCode { get; set; }
        public string Street { get; set; }
        public AddressLookUpRepoCommand(AddressLookUpRequest request, string country, string postalCode, string street)
        {
            Request = request;
            Country = country;
            PostalCode = postalCode;
            Street = street;
        }
    }
}
