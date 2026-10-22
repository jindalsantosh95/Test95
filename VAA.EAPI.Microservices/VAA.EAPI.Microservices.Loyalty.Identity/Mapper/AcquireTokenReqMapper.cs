using Microsoft.Azure.Functions.Worker.Http;
using System;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Identity.Domain.AcquireToken;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Mapper
{
    public class AcquireTokenReqMapper
    {
        public static AcquireTokenRequest Mapper(HttpRequestData req)
        {
            if (req == null) throw new ArgumentNullException(nameof(req));

            var error = "One of the headers is missing";

            return new AcquireTokenRequest
            {
                RLOC = req.Headers.Contains("Pnr") ? req.Headers.GetValues("Pnr").First() : throw new ArgumentNullException(error),
                FirstName = req.Headers.Contains("FirstName") ? req.Headers.GetValues("FirstName").First() : throw new ArgumentNullException(error),
                LastName = req.Headers.Contains("LastName") ? req.Headers.GetValues("LastName").First() : throw new ArgumentNullException(error),
                Type = req.Headers.Contains("Type") ? req.Headers.GetValues("Type").First() : throw new ArgumentNullException(error),
                TicketRef = req.Headers.Contains("TicketRef") ? req.Headers.GetValues("TicketRef").First() : throw new ArgumentNullException(error)
            };
        }
    }
}
