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
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Retail.Offer.Constants;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.CancelReshop
{
    public class CancelReshopQueryHandler : IRequestHandler<CancelReshopQuery, Result<Response<CancelReshopResponse>>>
    {
        public readonly IMediator _mediator;
        public readonly ILogger<CancelReshopQueryHandler> _logger;

        public CancelReshopQueryHandler(IMediator mediator, ILogger<CancelReshopQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }


        public async Task<Result<Response<CancelReshopResponse>>> Handle(CancelReshopQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var pnrRetrieveResponse = await _mediator.Send(new PNRRetrieveHistoryRepoQuery(request.CancelOrderRequest?.ReshopActions?.DeleteOrder?.BookingReference?.RLOC), cancellationToken);
                var ticketNumbers = pnrRetrieveResponse?.PnrType?.TicketInformation?.Select(x => x.TicketNumber);


                if (ticketNumbers == null || pnrRetrieveResponse != null && !ValidateRFC(pnrRetrieveResponse))
                {
                    return Result.Success(new Response<CancelReshopResponse>
                    {
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException
                        (
                            ErrorCodes.RFCErrorCode,
                            Guid.NewGuid().ToString(),
                            EAPI.Infrastructure.Constants.Constants.ErrorCodesPath

                        ),
                        Status = Status.BusinessException
                    });
                }

                //Call Ticketing Process V3 Delta service 
                var ticketingV3RetrieveResponse = await _mediator.Send(new TicketingV3RetrieveRepoQuery(TicketingV3RetrieveRQMapper.RetrieveRequestMapper(ticketNumbers)), cancellationToken);

                //Map to OrderCancelReshop Response 
                CancelReshopResponse cancelReshopResponse = CancelReshopMapper.RfcReshopResponse(pnrRetrieveResponse.PnrType, ticketingV3RetrieveResponse);

                return Result.Success(new Response<CancelReshopResponse>
                {
                    Status = Status.OK,
                    Value = cancelReshopResponse
                });
            }
            catch (ExternalServiceIntegrationException ex)
            {
                _logger.LogError(ex, $"Error occured in Delta service");

                if (ex.InnerException.Message.Contains(Constants.Application.InvalidPnrErrorMessage))
                    return Result.Failure<Response<CancelReshopResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.PNRExpiredErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                else
                    return Result.Failure<Response<CancelReshopResponse>>(Json.Serialize(ErrorMapper.MapDeltaExceptionToEAPIException(ErrorCodes.ExceptionErrorCode, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath, ex.InnerException.Message)));
            }
        }

        private static bool ValidateRFC(PNRCommonV6RetrieveResponse pNRCommonV6RetrieveResponse)
        {
            var pnrCreationDate = new System.Text.RegularExpressions.Regex(Constants.Application.CreditTextRegex).Match(pNRCommonV6RetrieveResponse.PnrType.HistoryCreditOrSignatures.FirstOrDefault().CreditText).ToString();
            return !string.IsNullOrEmpty(pnrCreationDate) && DateTime.TryParseExact(pnrCreationDate, Constants.Application.PnrCreationDateTimeFormat, System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime pnrCreationDateTime) && (DateTime.UtcNow - pnrCreationDateTime).TotalHours <= ConfigReader.Get<double>(Constants.Environment.RFCWindowInHours);

        }
    }
}
