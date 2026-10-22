using System;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Mapper
{
    public static class AddressLookupRequestMapper
    {
        public static string AddressLookupServiceRequestMapper(string country, string postCode, string street)
        {
            return Xml.Serialize(new CompleteAddressLookup_RQ
            {
                Message = new Message
                {
                    MessageId = Guid.NewGuid().ToString(),
                    Request = new Request
                    {
                        Country = country,
                        PostOrZipCode = postCode,
                        StreetDetails = street
                    }
                }
            });
        }
    }
}
