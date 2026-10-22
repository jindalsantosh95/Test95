using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using VAA.EAPI.Domain.Entities.Common;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Domain.TicketFeedData;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Ticket.Constants;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Retail.Order.Ticket.Application.Queries.RetrieveTicket
{
    public class RetriveTicketQueryHandler : IRequestHandler<RetriveTicketQuery, Result<Response<TicketData>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RetriveTicketQueryHandler> _logger;

        public RetriveTicketQueryHandler(IMediator mediator, ILogger<RetriveTicketQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<TicketData>>> Handle(RetriveTicketQuery request, CancellationToken cancellationToken)
        {
            var ticketDoc = await _mediator.Send(new RetriveTicketRepoQuery(request.RetriveTicketRequest), cancellationToken);
            if (ticketDoc == null)
            {

               return Result.Success
                    (
                        new Response<TicketData>
                        {
                            Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.InvalidTicketNumber, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath),
                            Status = Status.BusinessException
                        }
                    );
            }

            return Result.Success(new Response<TicketData>
            {
                Status = Status.OK,
                Value = ticketDoc
            });
        }
    }
}



