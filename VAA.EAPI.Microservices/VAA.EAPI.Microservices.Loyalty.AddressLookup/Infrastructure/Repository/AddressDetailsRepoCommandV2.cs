using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity.VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository
{
    public class AddressDetailsRepoCommandV2 : AddressDetailsRequest, IRequest<List<CompleteAddressDetails>>
    {
        public AddressDetailsRepoCommandV2(string countryCode, string postCode, string addressLine1)
        {
            CountryCode = countryCode;
            PostCode = postCode;
            AddressLine1 = addressLine1;
        }
    }
}
