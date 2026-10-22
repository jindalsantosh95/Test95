using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Net;
using System.Net.Http;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Insights.Domain
{
    public sealed class Endpoint
    {
        public static explicit operator Endpoint(HttpResponseMessage source) => new()
        {
            Source = source.RequestMessage.RequestUri,
            StatusCode = source.StatusCode,
            Result = Json.Deserialize<dynamic>(source.Content.ReadAsStringAsync().GetAwaiter().GetResult())
        };

        public Uri Source { get; private set; }
        public HttpStatusCode StatusCode { get; private set; }
        public dynamic Result { get; private set; }
    }
}
