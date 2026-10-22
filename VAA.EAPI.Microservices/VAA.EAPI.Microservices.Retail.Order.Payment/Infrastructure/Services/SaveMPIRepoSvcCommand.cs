using MediatR;
using System.Collections.Generic;
using System.Net;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services
{
    public class SaveMPIRepoSvcCommand : IRequest<KeyValuePair<HttpStatusCode, string>>
    {
        public PaymentCardAuthenticationRequest AuthenticationRequest { get; set; }

        public SaveMPIRepoSvcCommand(PaymentCardAuthenticationRequest authenticationRequest)
        {
            AuthenticationRequest = authenticationRequest;
        }
    }
}
