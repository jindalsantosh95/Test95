using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.MPI;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class SaveMPIRepoCommand : IRequest<Result<Response<string>>>
    {
        public AuthenticationRequest AuthenticationRequest { get; set; }

        public SaveMPIRepoCommand(AuthenticationRequest authenticationRequest)
        {
            AuthenticationRequest = authenticationRequest;
        }
    }
}
