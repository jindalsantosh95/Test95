using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Infrastructure.Caching;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;
using VAA.EAPI.Microservices.Retail.Offer.Domain.Reshop;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Repositories.Ita;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita.ReshopForCancellation;
using VAA.EAPI.Microservices.Retail.Offer.Mapper.ITA.ReshopForCancellation;
using Status = VAA.EAPI.Domain.Entities.Common.Status;

namespace VAA.EAPI.Microservices.Retail.Offer.Application.Query.Ita.ReshopForCancellation
{
    public class ReshopForCancellationQueryHandler : IRequestHandler<ReshopForCancellationQuery, Result<Response<CancelReshopResponse>>>
    {
        public readonly IMediator _mediator;
        public readonly ILogger<ReshopForCancellationQueryHandler> _logger;
        private readonly IDistributedCacheService _distributedCacheService;

        public ReshopForCancellationQueryHandler(IMediator mediator, ILogger<ReshopForCancellationQueryHandler> logger, IDistributedCacheService distributedCacheService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _distributedCacheService = distributedCacheService ?? throw new ArgumentNullException(nameof(distributedCacheService));
        }

        public async Task<Result<Response<CancelReshopResponse>>> Handle(ReshopForCancellationQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var accountableDocument = request.ReshopOrder?.Select(order => order.AccountableDocumentRefs?.FirstOrDefault());
                string scheduledDeparture = request.SegmentDetails?.First().ScheduledDepartureTime.LocalDateTime;

                if ( request.IsCancelDisruption|| ValidateRFC(accountableDocument.FirstOrDefault(), request.BookingReferences, scheduledDeparture))
                {
                    DateTime expiration;
                    var offerExpiryMinutes = DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes));
                    if (request.IsCancelDisruption)
                    {
                        expiration = offerExpiryMinutes;
                    }
                    else
                    {
                        var accountableDocumentRefs = accountableDocument.First();
                        var issueDateTime = Convert.ToDateTime(accountableDocumentRefs.IssueDate + " " + accountableDocumentRefs.IssueTime);
                        var rfcWindowHours = Convert.ToDouble(Constants.Application.RFCWindowInHours);
                        var rfcWindowTime = issueDateTime.AddHours(rfcWindowHours);

                        expiration = offerExpiryMinutes > rfcWindowTime ? rfcWindowTime : offerExpiryMinutes;
                    }

                    CancelReshopResponse cancelReshopResponse = ReshopForCancellationResponseMapper.RfcReshopResponse(request, expiration.ToString(Constants.Application.ZuluDateTimeFormat));
                    cancelReshopResponse.IsRfc = true;

                    var cacheAbsoluteExpiration = TimeSpan.FromMinutes((expiration - DateTime.UtcNow).TotalMinutes);

                    await _distributedCacheService.Set(cancelReshopResponse.ShoppingResponseId, cancelReshopResponse, false, TimeSpan.FromSeconds(0), cacheAbsoluteExpiration);
                    
                    return new Response<CancelReshopResponse>
                    {
                        Status = EAPI.Domain.Entities.Common.Status.OK,
                        Value = cancelReshopResponse
                    };
                }
                else
                {
                    var searchResult = ReshopForCancellationRequestMapper.ITAAwrReqMapper(request);

                    var response = await _mediator.Send(new ItaRepoQuery { Search = searchResult });

                    var repriceSearchResult = ReshopForCancellationRequestMapper.ITARepriceReqMapper(request);

                    var repriceResponse = await _mediator.Send(new ItaRepoQuery { Search = repriceSearchResult });

                    var expiration = DateTime.UtcNow.AddMinutes(ConfigReader.Get<double>(Constants.Environment.OfferExpiryLimitInMinutes));

                    var cancelReshopResponse = ReshopForCancellationResponseMapper.NonRfcReshopResponse(request, response, expiration.ToString(Constants.Application.ZuluDateTimeFormat));
                    cancelReshopResponse.IsRfc = false;

                    var cacheAbsoluteExpiration = TimeSpan.FromMinutes((expiration - DateTime.UtcNow).TotalMinutes);

                    await _distributedCacheService.Set(cancelReshopResponse.ShoppingResponseId, cancelReshopResponse, false, TimeSpan.FromSeconds(0), cacheAbsoluteExpiration);
                    
                    return new Response<CancelReshopResponse>
                    {
                        Status = Status.OK,
                        Value = cancelReshopResponse
                    };
                }
            }
            catch (ExternalServiceIntegrationException ex)
            {
                return new Response<CancelReshopResponse>()
                {
                    Status = Status.BusinessException
                };
            }
        }

        private static bool ValidateRFC(AccountableDocumentRef accountableDocument, BookingReferences bookingReferences, string scheduledDeparture)
        {
            DateTime ticketPurchaseDate = DateTime.Now.AddDays(7);
            string rLOCCreateDate = bookingReferences.RLOCCreateDate?.ToString("yyyy-MM-dd");

            return accountableDocument.DocumentType == Constants.Application.TicketDocumentType &&
                rLOCCreateDate != null && !string.IsNullOrEmpty(bookingReferences.RLOCCreateDateTime) &&
                Convert.ToDateTime(scheduledDeparture) >= ticketPurchaseDate &
                DateTime.TryParseExact(rLOCCreateDate + " " + bookingReferences.RLOCCreateDateTime, "yyyy-MM-dd HH:mm:ss", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime ticketIssueDateTime) &&
                (DateTime.UtcNow - ticketIssueDateTime).TotalHours <= Convert.ToDouble(Constants.Application.RFCWindowInHours);
        }
    }
}
