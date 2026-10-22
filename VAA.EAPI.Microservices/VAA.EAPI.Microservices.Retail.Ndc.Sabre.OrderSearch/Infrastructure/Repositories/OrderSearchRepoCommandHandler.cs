using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.OrderSearch.Infrastructure.Repositories
{
    public class OrderSearchRepoCommandHandler : IRequestHandler<OrderSearchRepoCommand, Result<Response<GetReservationRS>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<OrderSearchRepoCommandHandler> _logger;
        private readonly ICreateTokenCommand _createTokenCommand;

        public OrderSearchRepoCommandHandler(IMediator mediator, ILogger<OrderSearchRepoCommandHandler> logger, ICreateTokenCommand createTokenCommand)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createTokenCommand = createTokenCommand ?? throw new ArgumentNullException(nameof(createTokenCommand));
        }

        public async Task<Result<Response<GetReservationRS>>> Handle(OrderSearchRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                if (!request.OrderSearchRequest.SearchFilterCriteria.HistoryFlag)
                {
                    var token = await _mediator.Send(new CreateSessionCommand() { PCC = request.OrderSearchRequest.SearchFilterCriteria.Parties.Participants.First().PccCode }, cancellationToken);

                    if (token != null)
                    {
                        var getReservationRequest = GetReservationRQMapper.GetReservationRequest(token, request.OrderSearchRequest);

                        var response = await _mediator.Send(new GetReservationQuery(getReservationRequest), cancellationToken);

                        await _mediator.Send(new SessionCloseCommand() { BinaryToken = token, PCC = request.OrderSearchRequest.SearchFilterCriteria.Parties.Participants.First().PccCode }, cancellationToken);

                        if (response != null)
                        {
                            return Result.Success(new Response<GetReservationRS>
                            {
                                Status = response.Errors is null ? Status.OK : Status.Error,
                                Value = response
                            });
                        }
                        else
                        {
                            _logger.LogError("Sabre operation returned no reservation. {Operation}", "GetReservation");
                        }
                    }
                }
                else
                {
                    var token = await _createTokenCommand.Handle(request.OrderSearchRequest.SearchFilterCriteria.Parties.Participants.First().PccCode);

                    if (token != null)
                    {
                        var getReservationRequest = GetReservationRQMapper.GetReservationRequest(token, request.OrderSearchRequest);

                        var response = await _mediator.Send(new GetReservationQuery(getReservationRequest), cancellationToken);

                        if (response != null)
                        {
                            return Result.Success(new Response<GetReservationRS>
                            {
                                Status = response.Errors is null ? Status.OK : Status.Error,
                                Value = response
                            });
                        }
                        else
                        {
                            _logger.LogError("Sabre operation returned no reservation. {Operation}", "GetReservation");
                        }
                    }
                }

                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            
            }
            catch (ExternalServiceIntegrationException)
            {
                _logger.LogError("Order search reservation retrieval failed. {Operation}", "GetReservation");
                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Order search reservation retrieval failed. {Operation}", "GetReservation");
                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
