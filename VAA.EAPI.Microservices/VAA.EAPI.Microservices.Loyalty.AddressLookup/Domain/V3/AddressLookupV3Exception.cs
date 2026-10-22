using System;
using System.Net;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.V3
{
    public sealed class AddressLookupV3Exception : Exception
    {
        public AddressLookupV3Exception(HttpStatusCode statusCode, string code, string message)
            : base(message)
        {
            StatusCode = statusCode;
            Code = code;
        }

        public HttpStatusCode StatusCode { get; }
        public string Code { get; }
    }
}
