using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Command.PaymentMPI
{
    public class SaveMPICommand : IRequest<Result<Response<string>>>
    {
        public AuthenticationRequest AuthenticationRequest { get; set; }

        public SaveMPICommand(AuthenticationRequest authenticationRequest)
        {
            AuthenticationRequest = authenticationRequest;
        }
    }
}
