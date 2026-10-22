using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.FlightInformation.Domain.Airportdetails.Enums;
using VAA.EAPI.Microservices.FlightInformation.Mapper;

namespace VAA.EAPI.Microservices.FlightInformation.Application.Command.Airportdetails
{
    public class AirportDetailsMergeEventCommandHandler : IRequestHandler<AirportDetailsMergeEventCommand, Result<Response<string>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AirportDetailsMergeEventCommandHandler> _logger;
        public AirportDetailsMergeEventCommandHandler(IMediator mediator, ILogger<AirportDetailsMergeEventCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<string>>> Handle(AirportDetailsMergeEventCommand airportdataRequest, CancellationToken cancellationToken)
        {
            StringBuilder sb = new StringBuilder();

            if (airportdataRequest == null) throw new ArgumentNullException(nameof(airportdataRequest));


            foreach (var item in airportdataRequest.PayLoad)
            {
                try
                {
                    var cloudEvent = AirportDetailsToEventMapper<Domain.Airportdetails.Model.Airportdetail>.CommandToEventMapper(item, string.Empty);
                    await _mediator.Send(new EventRequestInsertCommand { Id = cloudEvent.Id, Type = cloudEvent.Type, Data = cloudEvent.Data, Subject = cloudEvent.Subject }, cancellationToken);
                    try
                    {

                        await _mediator.Publish(cloudEvent, cancellationToken);
                    }
                    catch (Exception)
                    {
                        _logger.LogTrace($"Rollback from event store");
                        await _mediator.Send(new EventStoreRequestDeleteCommand { Id = cloudEvent.Id, Type = AirportDetailEventType.DeleteAirportDetailsEvent.ToString(), Data = airportdataRequest, Subject = Constants.Environment.EventSubject }, cancellationToken);
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Airport details merge event processing failed.");

                    sb.Append(ex.Message + " ");
                }
            }

            var airportdataResponse = AirportDetailsResponseMapper.ResponseMapper();

            return sb.Length == 0 ? Result.Success<Response<string>>(airportdataResponse) : Result.Failure<Response<string>>(sb.ToString());
        }
    }
}
