using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.MPI;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.MPI;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories
{
    public class SaveMPIRepoCommandHandler : IRequestHandler<SaveMPIRepoCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<SaveMPIRepoCommandHandler> _logger;

        public SaveMPIRepoCommandHandler(IMediator mediator, ILogger<SaveMPIRepoCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(SaveMPIRepoCommand request, CancellationToken cancellationToken)
        {

            var authenticationRequest = MPIPaymentAuthenticationRequestMapper.MPIDataRQMapper(request.AuthenticationRequest);

            var response = await _mediator.Send(new SaveMPIRepoSvcCommand(authenticationRequest), cancellationToken);

            var authenticationResponse = Json.Deserialize<PaymentCardAuthenticationResponse>(response.Value);

            var paymentReference = authenticationResponse.FormOfPayment.PaymentReferenceId;

            return new Response<string> { Status = Status.OK, Value = paymentReference };
        }
    }
}
