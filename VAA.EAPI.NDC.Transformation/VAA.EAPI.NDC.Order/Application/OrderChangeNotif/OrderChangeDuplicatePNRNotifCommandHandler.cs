using CSharpFunctionalExtensions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.NdcQueueMgmt;

namespace VAA.EAPI.NDC.Order.Application.OrderChangeNotif
{
    public class OrderChangeDuplicatePNRNotifCommandHandler : IOrderChangeDuplicatePNRNotifCommand
    {
        private readonly ILogger<OrderChangeDuplicatePNRNotifCommandHandler> _logger;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly ValidationMSAgencyOCNDetailsQueryHandler _agencyOCNDetailsQueryHandler;
        private readonly OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly IOrderNotificationMSCommand _orderNotificationMSCommand;
        private readonly INdcQueueMgmtFetchDuplicatePnrsCommand _ndcQueueMgmtFetchDuplicatePnrsCommand;
        private readonly INdcQueueMgmtRemoveDuplicatePnrsCommand _ndcQueueMgmtRemoveDuplicatePnrsCommand;

        public OrderChangeDuplicatePNRNotifCommandHandler(
            ILogger<OrderChangeDuplicatePNRNotifCommandHandler> logger,
            ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler,
            OrderDataMSQueryHandler orderDataMSQueryHandler,
            ValidationMSCommandHandler validationMSCommandHandler,
            ValidationMSAgencyOCNDetailsQueryHandler agencyOCNDetailsQueryHandler,
            OrderSearchMSCommandHandler orderSearchMSCommandHandler,
            IOrderNotificationMSCommand orderNotificationMSCommand,
            INdcQueueMgmtFetchDuplicatePnrsCommand ndcQueueMgmtFetchDuplicatePnrsCommand,
            INdcQueueMgmtRemoveDuplicatePnrsCommand ndcQueueMgmtRemoveDuplicatePnrsCommand)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
            _orderDataMSQueryHandler = orderDataMSQueryHandler ?? throw new ArgumentNullException(nameof(orderDataMSQueryHandler));
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler ?? throw new ArgumentNullException(nameof(orderSearchMSCommandHandler));
            _orderNotificationMSCommand = orderNotificationMSCommand ?? throw new ArgumentNullException(nameof(orderNotificationMSCommand));
            _validationMSCommandHandler = validationMSCommandHandler ?? throw new ArgumentNullException(nameof(validationMSCommandHandler));
            _agencyOCNDetailsQueryHandler = agencyOCNDetailsQueryHandler ?? throw new ArgumentNullException(nameof(agencyOCNDetailsQueryHandler));
            _ndcQueueMgmtFetchDuplicatePnrsCommand = ndcQueueMgmtFetchDuplicatePnrsCommand ?? throw new ArgumentNullException(nameof(ndcQueueMgmtFetchDuplicatePnrsCommand));
            _ndcQueueMgmtRemoveDuplicatePnrsCommand = ndcQueueMgmtRemoveDuplicatePnrsCommand ?? throw new ArgumentNullException(nameof(ndcQueueMgmtRemoveDuplicatePnrsCommand));
        }

        public async Task<Result<Response<string>>> Handle(OrderChangeDuplicatePNRNotifRQ request, CancellationToken cancellationToken)
        {
            try
            {
                var queuePnrs = await RetrievePnrsFromQueueAsync(request, cancellationToken) ?? Array.Empty<string>();
                var pnrs = queuePnrs.Count > 0 ? queuePnrs : new[] { request.UniqueId };

                foreach (var pnrItem in pnrs)
                {
                    TrackAnalyticsEventCommand analyticsEventCommand = new()
                    {
                        Type = nameof(TrackingAnalyticsType.Microservice),
                        Subject = Constants.Application.OrderChangeDuplicatePNRNotifEventSubject
                    };

                    analyticsEventCommand.Track("NDCVersion", Constants.Application.NDCVersion);
                    analyticsEventCommand.Track("DateofRequest", DateTime.UtcNow);

                    var statusCode = StatusCodes.Status200OK;
                    string orderId = pnrItem;
                    string rloc = null;
                    var ocnStatus = "Skipped";
                    var skipReason = "None";
                    var queueRemovalStatus = "NotRequested";
                    var agentAcknowledgementCount = 0;

                    try
                    {
                        analyticsEventCommand.Track("RequestType", "OCNDuplicatePNR");

                        analyticsEventCommand.Track("Stage", "OrderDataMS");

                        var orderRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery { OrderID = orderId }, cancellationToken);

                        if (orderRS?.OrderDocument is null)
                        {
                            analyticsEventCommand.Track("SkipReason", "OrderDocumentNotFound");
                            skipReason = "OrderDocumentNotFound";
                            _logger.LogWarning("DuplicatePNR OCN skipped. Unable to locate order document. PNR={PNR}", pnrItem);
                            continue;
                        }

                        var orderDocument = orderRS.OrderDocument;

                        rloc = orderDocument?.BookingReference?.RLOC ?? orderId?.Trim();

                        if (rloc.IsNullOrWhiteSpace())
                        {
                            analyticsEventCommand.Track("SkipReason", "MissingRloc");
                            skipReason = "MissingRloc";
                            _logger.LogWarning("DuplicatePNR OCN skipped. Missing PNR/RLOC. PNR={PNR}", pnrItem);
                            continue;
                        }

                        analyticsEventCommand.Track("PNR", rloc);

                        analyticsEventCommand.Track("Stage", "OrderSearchMS");

                        var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderDocument, false);
                        var getReservationOrderDoc = await _orderSearchMSCommandHandler.Handle(new OrderSearchMSCommand(getReservationReq), cancellationToken);
                        if (getReservationOrderDoc?.Errors?.Any() == true)
                        {
                            analyticsEventCommand.Track("SkipReason", "OrderSearchMSFailed");
                            skipReason = "OrderSearchMSFailed";
                            _logger.LogWarning("DuplicatePNR OCN skipped. GetReservation failed. PNR={PNR}", rloc);
                            continue;
                        }

                        var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(getReservationOrderDoc.Parties, getReservationOrderDoc.Parties?.CorporateAccount, false, true);

                        analyticsEventCommand.Track("Stage", "ValidationMS");
                        var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand { AgencyValidateRequest = validationMSRequest }, cancellationToken);
                        if (validationMSResponse?.Error is not null)
                        {
                            analyticsEventCommand.Track("SkipReason", "ValidationFailed");
                            skipReason = "ValidationFailed";
                            _logger.LogWarning("DuplicatePNR OCN skipped. Validation failed. PNR={PNR}", rloc);
                            continue;
                        }

                        if (validationMSResponse?.ValidateResponse?.OCNFlag == true)
                        {
                            var hasPossibleDupeBooking = getReservationOrderDoc?.OrderRemarks?.Any(r =>
                                (r?.Text).IsNullOrWhiteSpace() == false &&
                                r.Text.Contains("POSSIBLE DUPE BOOKING", StringComparison.OrdinalIgnoreCase)) == true;

                            analyticsEventCommand.Track("HasPossibleDupeBooking", hasPossibleDupeBooking);

                            if (!hasPossibleDupeBooking)
                            {
                                analyticsEventCommand.Track("SkipReason", "MissingPossibleDupeBookingRemark");
                                skipReason = "MissingPossibleDupeBookingRemark";
                                continue;
                            }

                            var duplicatePnrs = GetDuplicatePnrsFromRemarks(getReservationOrderDoc, rloc);
                            analyticsEventCommand.Track("DuplicatePnrGroupCount", duplicatePnrs.Count);

                            var agentIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                            var skipOcn = false;

                            foreach (var pnr in duplicatePnrs)
                            {
                                var dupDoc = pnr.Is(rloc)
                                    ? getReservationOrderDoc
                                    : await GetReservationByPnrAsync(pnr, analyticsEventCommand, cancellationToken);

                                if (dupDoc is null || dupDoc?.Errors?.Any() == true)
                                {
                                    analyticsEventCommand.Track("SkipReason", "GetReservationFailedForDuplicatePnr");
                                    skipReason = "GetReservationFailedForDuplicatePnr";
                                    _logger.LogWarning("DuplicatePNR OCN skipped because a related PNR could not be loaded.");
                                    skipOcn = true;
                                    break;
                                }

                                var sellerAgencyId = GetSellerAgencyId(dupDoc);

                                if (sellerAgencyId.IsNullOrWhiteSpace())
                                {
                                    analyticsEventCommand.Track("SkipReason", "MissingSellerAgencyIdForDuplicatePnr");
                                    skipReason = "MissingSellerAgencyIdForDuplicatePnr";
                                    _logger.LogWarning("DuplicatePNR OCN skipped. Missing Seller AgencyId for PNR {PNR}.", pnr);
                                    skipOcn = true;
                                    break;
                                }

                                agentIds.Add(sellerAgencyId);

                                if (agentIds.Count > 1)
                                {
                                    analyticsEventCommand.Track("SkipReason", "DifferentSellerAgencyIds");
                                    skipReason = "DifferentSellerAgencyIds";
                                    _logger.LogInformation("DuplicatePNR OCN skipped. Different Seller AgencyIds found for PNR group. MainPNR={MainPNR}", rloc);
                                    skipOcn = true;
                                    break;
                                }
                            }

                            if (skipOcn)
                            {
                                continue;
                            }

                            if (agentIds.Count != 1)
                            {
                                analyticsEventCommand.Track("SkipReason", "NoCommonSellerAgencyId");
                                skipReason = "NoCommonSellerAgencyId";
                                _logger.LogInformation("DuplicatePNR OCN skipped. Unable to determine a single common Seller AgencyId. MainPNR={MainPNR}", rloc);
                                continue;
                            }

                            var commonAgentId = agentIds.Single();

                            if (commonAgentId.IsNullOrWhiteSpace())
                            {
                                analyticsEventCommand.Track("SkipReason", "MissingSellerAgencyId");
                                skipReason = "MissingSellerAgencyId";
                                _logger.LogWarning("DuplicatePNR OCN skipped. Missing Seller AgencyId. PNR={PNR}", rloc ?? string.Empty);
                                continue;
                            }

                            analyticsEventCommand.Track("Stage", "ValidationMSAgencyOCNDetails");
                            var agencyOCNDetails = await _agencyOCNDetailsQueryHandler.Handle(
                                new ValidationMSAgencyOCNDetailsQuery
                                {
                                    AgencyId = commonAgentId,
                                    AggregatorId = GetAggregatorAgencyId(getReservationOrderDoc),
                                    Environment = ConfigReader.Get(Constants.Environment.AgentRegistrationEnvironment)
                                },
                                cancellationToken);

                            var agentEndpointDetails =
                                (agencyOCNDetails ?? Enumerable.Empty<Infrastructure.Model.ValidationMS.AgencyOCNDetails>())
                                .Where(x => x?.AgencyDetails != null
                                            && x.EndpointDetails != null
                                            && string.Equals(x.AgencyDetails.RequestAgencyId, commonAgentId, StringComparison.OrdinalIgnoreCase))
                                .GroupBy(x => x.AgencyDetails.RequestAgencyId)
                                .Select(g => new Infrastructure.Model.OrderNotificationMS.AgencyOCNDetails
                                {
                                    AgencyDetails = g.First().AgencyDetails,
                                    EndpointDetails = g.First().EndpointDetails
                                })
                                .ToList();

                            if (agentEndpointDetails?.Any() != true)
                            {
                                analyticsEventCommand.Track("SkipReason", "NoAgentEndpointDetails");
                                skipReason = "NoAgentEndpointDetails";
                                _logger.LogWarning("DuplicatePNR OCN skipped. No agent endpoint details found for AgentId {AgentId}.", commonAgentId);
                                continue;
                            }

                            analyticsEventCommand.Track("Stage", "OrderNotificationMS");

                            var agentResponse = await _orderNotificationMSCommand.Handle(new()
                            {
                                OrderDocument = getReservationOrderDoc,
                                AgentEndpointDetails = agentEndpointDetails,
                                TTLExpiryType = "DuplicatePNR",
                                IsDuplicatePNR = true
                            });

                            agentAcknowledgementCount = GetAgentAcknowledgementCount(agentResponse);

                            if (agentResponse?.Errors?.Any() == true)
                            {
                                analyticsEventCommand.Track("SkipReason", "OrderNotificationMSFailed");
                                ocnStatus = "Failed";
                                skipReason = "OrderNotificationMSFailed";
                                _logger.LogWarning("DuplicatePNR OCN skipped. OrderNotificationMS returned errors. PNR={PNR}", rloc ?? string.Empty);
                                continue;
                            }

                            if (agentResponse?.OrderNotificationAgentResponse?.AgentResponse != null)
                            {
                                foreach (var (x, index) in agentResponse.OrderNotificationAgentResponse.AgentResponse.Select((x, index) => (x, index)))
                                {
                                    int i = index + 1;
                                    analyticsEventCommand.Track($"Agent{i}", x?.AgentId ?? string.Empty);
                                    analyticsEventCommand.Track($"Agent{i}AckStatus", x?.AcknowledgementRcvd == true ? "Y" : "N");
                                }
                            }

                            ocnStatus = GetOcnStatus(agentResponse);
                            skipReason = "None";
                            _logger.LogInformation("DuplicatePNR OCN sent. PNR={PNR}", rloc);
                        }
                        else
                        {
                            analyticsEventCommand.Track("SkipReason", "OCNFlagFalse");
                            skipReason = "OCNFlagFalse";
                        }
                    }
                    catch (Exception ex)
                    {
                        statusCode = StatusCodes.Status500InternalServerError;
                        ocnStatus = "Failed";
                        skipReason = "UnhandledException";
                        analyticsEventCommand.Track("ExceptionType", ex.GetType().Name);
                        analyticsEventCommand.Track("FailureReason", "OcnItemProcessingFailed");
                        _logger.LogError(
                            ex,
                            "DuplicatePNR OCN item processing failed. PNR={PNR}",
                            rloc ?? string.Empty);
                    }
                    finally
                    {
                        if (queuePnrs.Contains(pnrItem, StringComparer.OrdinalIgnoreCase))
                        {
                            analyticsEventCommand.Track("Stage", "QueueMgmtRemoveDuplicatePnr");
                            var removed = await TryClearQueueItemAsync(request, pnrItem, cancellationToken);
                            analyticsEventCommand.Track("QueueItemRemoved", removed);
                            queueRemovalStatus = removed ? "Removed" : "Failed";

                            if (!removed && skipReason.Is("None"))
                                skipReason = "QueueRemovalFailed";
                        }

                        analyticsEventCommand.Track("OcnStatus", ocnStatus);
                        analyticsEventCommand.Track("AgentAcknowledgementCount", agentAcknowledgementCount);
                        analyticsEventCommand.Track("QueueRemovalStatus", queueRemovalStatus);
                        analyticsEventCommand.Track("StatusCode", statusCode);
                        await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
                    }
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DuplicatePNR OCN processing failed before queue items could be processed. UniqueId={UniqueId}", request?.UniqueId ?? string.Empty);

                TrackAnalyticsEventCommand analyticsEventCommand = new()
                {
                    Type = nameof(TrackingAnalyticsType.Microservice),
                    Subject = Constants.Application.OrderChangeDuplicatePNRNotifEventSubject
                };

                analyticsEventCommand.Track("NDCVersion", Constants.Application.NDCVersion);
                analyticsEventCommand.Track("DateofRequest", DateTime.UtcNow);
                analyticsEventCommand.Track("ExceptionType", ex.GetType().Name);
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);

                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }

            return Result.Success(new Response<string>() { Status = Status.Accepted });
        }

        private static string GetOcnStatus(Infrastructure.Model.OrderNotificationMS.OrderNotificationResponse agentResponse)
        {
            var agentResponses = agentResponse?.OrderNotificationAgentResponse?.AgentResponse;

            if (agentResponses?.Any() != true)
                return "Submitted";

            return agentResponses.All(agent => agent?.AcknowledgementRcvd == true)
                ? "Acknowledged"
                : "SubmittedWithoutAcknowledgement";
        }

        private static int GetAgentAcknowledgementCount(Infrastructure.Model.OrderNotificationMS.OrderNotificationResponse agentResponse)
        {
            return agentResponse?.OrderNotificationAgentResponse?.AgentResponse?
                .Count(agent => agent?.AcknowledgementRcvd == true) ?? 0;
        }

        private async Task<bool> TryClearQueueItemAsync(OrderChangeDuplicatePNRNotifRQ request, string pnr, CancellationToken cancellationToken)
        {
            try
            {
                return await _ndcQueueMgmtRemoveDuplicatePnrsCommand.Handle(request, pnr, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "DuplicatePNR queue removal request failed. PCC={PCC}, PNR={PNR}",
                    request?.PCC ?? string.Empty,
                    pnr);
                return false;
            }
        }

        private async Task<Infrastructure.Model.OrderMS.OrderDocument> GetReservationByPnrAsync(string pnr, TrackAnalyticsEventCommand analyticsEventCommand, CancellationToken cancellationToken)
        {
            analyticsEventCommand?.Track("Stage", "OrderDataMS-DuplicatePNR");

            var orderRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery { OrderID = pnr }, cancellationToken);

            if (orderRS?.OrderDocument is null)
                return null;

            analyticsEventCommand?.Track("Stage", "OrderSearchMS-DuplicatePNR");
            var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderRS.OrderDocument, false);
            return await _orderSearchMSCommandHandler.Handle(new OrderSearchMSCommand(getReservationReq), cancellationToken);
        }

        private static IReadOnlyCollection<string> GetDuplicatePnrsFromRemarks(Infrastructure.Model.OrderMS.OrderDocument getReservationOrderDoc, string mainPnr)
        {
            var pnrs = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            if (!mainPnr.IsNullOrWhiteSpace())
            {
                pnrs.Add(mainPnr.Trim());
            }

            var remarks = getReservationOrderDoc?.OrderRemarks?
                .Select(x => x?.Text)
                .Where(x => !x.IsNullOrWhiteSpace())
                .ToList();

            if (remarks == null)
            {
                return pnrs.ToList();
            }

            foreach (var text in remarks)
            {
                if (!text.Contains("POSSIBLE DUPE BOOKING", StringComparison.OrdinalIgnoreCase) &&
                    !text.Contains("SEE PNR", StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var tokens = text
                    .Replace(",", " ")
                    .Replace(".", " ")
                    .Replace(":", " ")
                    .Replace(";", " ")
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                foreach (var t in tokens)
                {
                    var token = t.Trim();
                    if (token.Length == 6 && token.All(char.IsLetterOrDigit))
                    {
                        pnrs.Add(token);
                    }
                }
            }

            return pnrs.ToList();
        }

        private static string GetSellerAgencyId(Infrastructure.Model.OrderMS.OrderDocument orderDocument)
        {
            return orderDocument?.Parties?.Participants?
                .FirstOrDefault(p => p.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller)
                ?.AgencyID;
        }

        private static string GetAggregatorAgencyId(Infrastructure.Model.OrderMS.OrderDocument orderDocument)
        {
            return orderDocument?.Parties?.Participants?
                .FirstOrDefault(p => p.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Distributor)
                ?.AgencyID;
        }

        private async Task<IReadOnlyCollection<string>> RetrievePnrsFromQueueAsync(OrderChangeDuplicatePNRNotifRQ request, CancellationToken cancellationToken)
        {
            try
            {
                return await _ndcQueueMgmtFetchDuplicatePnrsCommand.Handle(request, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "QueueMgmt fetch duplicate PNRs failed.");
                return Array.Empty<string>();
            }
        }
    }
}
