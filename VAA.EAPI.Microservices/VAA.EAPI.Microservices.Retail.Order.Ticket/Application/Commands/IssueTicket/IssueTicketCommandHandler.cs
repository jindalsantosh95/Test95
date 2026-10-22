using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.IssueTicket;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Commands.IssueTicket
{
    public class IssueTicketCommandHandler : IRequestHandler<IssueTicketCommand, Result<Response<IssueTicketResponse>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<IssueTicketCommandHandler> _logger;
        public IssueTicketCommandHandler(IMediator mediator, ILogger<IssueTicketCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IssueTicketResponse>>> Handle(IssueTicketCommand request, CancellationToken cancellationToken)
        {

            try
            {
                var response = await _mediator.Send(new IssueTicketRepoCommand { IssueTicketRequest = request.IssueTicketRequest });
                Result<Response<IssueTicketResponse>> commandOutput = new Response<IssueTicketResponse>
                {
                    Status = Status.OK,
                    Value = response
                };

                return commandOutput;
            }
            catch (ExternalServiceIntegrationException ex)
            {
                return Result.Success(new Response<IssueTicketResponse>
                {
                    Errors = ErrorMapper.MapDeltaExceptionToEAPIException(Constants.ErrorCodes.DeltaUnknownErrors, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message.ToString()),
                    Status = Status.Error
                });
            }

        }
    }
}
