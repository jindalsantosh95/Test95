using CSharpFunctionalExtensions;
using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Query
{
    public class AddressLookupQuery : AddressDetailsRequest, IRequest<Result<Response<List<AddressDetail>>>>
    {
        public AddressLookupQuery(string countryCode, string postCode, string addressLine1)
        {
            CountryCode = countryCode;
            PostCode = postCode;
            AddressLine1 = addressLine1;
        }
    }
}
