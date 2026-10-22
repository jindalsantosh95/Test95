using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.GetReservation;
using VAA.EAPI.Infrastructure.NDC.Sabre.Repositories;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Repositories
{
    public class SplitPnrRepoCommandHandler : ISplitPnrRepoCommand
    {
        private readonly ILogger<SplitPnrRepoCommandHandler> _logger;
        private readonly CreateSessionCommandHandler _createSessionCommandHandler;
        private readonly GetReservationQueryHandler _getReservationQueryHandler;
        private readonly SessionCloseCommandHandler _sessionCloseCommandHandler;
        private readonly ITravelItineraryDivideLLSCommand _travelItineraryDivideLLSCommand;
        private readonly IEndTransactionCommand _endTransactionCommand;
        private readonly EnhancedEndTransactionCommandHandler _enhancedEndTransactionCommand;
        private readonly UpdatePassengerNameRecordCommandHandler _updatePassengerNameRecordCommandHandler;

        public SplitPnrRepoCommandHandler(CreateSessionCommandHandler createSessionCommandHandler, SessionCloseCommandHandler sessionCloseCommandHandler, GetReservationQueryHandler getReservationQueryHandler, ILogger<SplitPnrRepoCommandHandler> logger, ITravelItineraryDivideLLSCommand travelItineraryDivideLLSCommand, IEndTransactionCommand endTransactionCommand, EnhancedEndTransactionCommandHandler enhancedEndTransactionCommand, UpdatePassengerNameRecordCommandHandler updatePassengerNameRecordCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _createSessionCommandHandler = createSessionCommandHandler ?? throw new ArgumentNullException(nameof(createSessionCommandHandler));
            _getReservationQueryHandler = getReservationQueryHandler ?? throw new ArgumentNullException(nameof(getReservationQueryHandler));
            _sessionCloseCommandHandler = sessionCloseCommandHandler ?? throw new ArgumentNullException(nameof(sessionCloseCommandHandler));
            _travelItineraryDivideLLSCommand = travelItineraryDivideLLSCommand ?? throw new ArgumentNullException(nameof(travelItineraryDivideLLSCommand));
            _endTransactionCommand = endTransactionCommand ?? throw new ArgumentNullException(nameof(endTransactionCommand));
            _enhancedEndTransactionCommand = enhancedEndTransactionCommand ?? throw new ArgumentNullException(nameof(enhancedEndTransactionCommand));
            _updatePassengerNameRecordCommandHandler = updatePassengerNameRecordCommandHandler ?? throw new ArgumentNullException(nameof(updatePassengerNameRecordCommandHandler));
        }

        public async Task<Result<Response<GetReservationRS>>> Handle(SplitPnrRequest request, CancellationToken cancellationToken)
        {
            try
            {
                int responseDelayInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);
                var delay = TimeSpan.FromMilliseconds(responseDelayInMS);

                var token = await _createSessionCommandHandler.Handle(new CreateSessionCommand() { PCC = request.OrderDocument.Parties.Participants.First().PccCode }, cancellationToken);

                if (token != null)
                {
                    var getReservationRQ = GetReservationRequestMapper.GetReservationRequest(token, request.OrderDocument.BookingReference.RLOC);
                    var getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);

                    var childPNRGetReservationRS = new GetReservationRS();

                    if (getReservationRS != null || getReservationRS?.Errors == null)
                    {
                        var customers = request.OrderDocument.DataList.CustomerDetails.Where(cd => request.splitPaxId.Contains(cd.CustId));

                        var divideRQ = SplitPNRRequestMapper.DivideRQRequest(token, customers);
                        var divideRS = await _travelItineraryDivideLLSCommand.Handle(divideRQ, cancellationToken);

                        if (divideRS.TravelItineraryDivideRS?.ApplicationResults.Error == null || divideRS.TravelItineraryDivideRS?.ApplicationResults.Warning == null)
                        {
                            var endTransactionRQ = SplitPNREndTransactionRQMapper.EndTransationRequest(token);
                            var endTransactionRS = await _endTransactionCommand.Handle(endTransactionRQ, cancellationToken);

                            if (endTransactionRS?.ApplicationResults?.Error == null || endTransactionRS?.ApplicationResults?.Warning == null)
                            {
                                var filePNRRQ = SplitPNRRequestMapper.FileSplitPNRRQ(token);
                                var filePNRRS = await _travelItineraryDivideLLSCommand.Handle(filePNRRQ, cancellationToken);

                                if (filePNRRS?.TravelItineraryDivideRS?.ApplicationResults?.Error == null || filePNRRS?.TravelItineraryDivideRS?.ApplicationResults?.Warning == null)
                                {
                                    var enhancedTransactionRQ = EnhancedEndTransactionRequestMapper.GetEnhancedEndTransactionRequest(token);
                                    var enhancedTransactionRS = await _enhancedEndTransactionCommand.Handle(new EnhancedEndTransactionCommand() { EnhancedEndTransactionRQ = enhancedTransactionRQ }, cancellationToken);
                                }
                            }

                            else
                            {
                                _logger.LogError("Sabre EndTransaction returned an error response");
                            }
                        }
                        
                        else
                        {
                            _logger.LogError("Sabre TravelItineraryDivide returned an error response");
                        }
                        await Task.Delay(delay, cancellationToken);

                        getReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(getReservationRQ), cancellationToken);

                        if (request.OrderDocument.DataList.CustomerDetails.Any(x => x.IsEmailUpdates))
                        {
                            var response = (getReservationRS.Item as ReservationPNRB).BookingDetails;
                            var childRloc = (response.DivideSplitDetails.Items.FirstOrDefault().RecordLocator);

                            var childPnrGetReservationReq = GetReservationRequestMapper.GetReservationRequest(token, ((getReservationRS.Item as ReservationPNRB).BookingDetails.DivideSplitDetails as BookingDetailsPNRBDivideSplitDetails).Items.FirstOrDefault().RecordLocator);
                            childPNRGetReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(childPnrGetReservationReq), cancellationToken);

                            var updatePassengerNameRecordRQ = SplitPNRUpdatePassengerNameRecordMapper.UpdatePnrRequestMapper(token, request.OrderDocument.DataList.CustomerDetails, childPNRGetReservationRS, childRloc);
                            var updatePassengerNameRecordRS = await _updatePassengerNameRecordCommandHandler.Handle(new UpdatePassengerNameRecordCommand() { UpdatePassengerNameRecordRQRequest = updatePassengerNameRecordRQ }, cancellationToken);

                            if (updatePassengerNameRecordRS?.UpdatePassengerNameRecordRS?.ApplicationResults?.Error != null || updatePassengerNameRecordRS?.UpdatePassengerNameRecordRS?.ApplicationResults?.Warning == null)
                            {
                                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                            }
                        }

                        var childPnrGetReservationRQ = GetReservationRequestMapper.GetReservationRequest(token, ((getReservationRS.Item as ReservationPNRB).BookingDetails.DivideSplitDetails as BookingDetailsPNRBDivideSplitDetails).Items.LastOrDefault().RecordLocator);
                        childPNRGetReservationRS = await _getReservationQueryHandler.Handle(new GetReservationQuery(childPnrGetReservationRQ), cancellationToken);
                    }
                    
                    else
                    {
                        _logger.LogError("Sabre GetReservation returned an error response");
                    }

                    await _sessionCloseCommandHandler.Handle(new SessionCloseCommand() { BinaryToken = token, PCC = request.OrderDocument.Parties.Participants.First().PccCode }, cancellationToken);

                    return Result.Success(new Response<GetReservationRS>
                    {
                        Status = Status.OK,
                        Value = childPNRGetReservationRS
                    });
                }

                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Split PNR Sabre workflow failed");
                return Result.Failure<Response<GetReservationRS>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
