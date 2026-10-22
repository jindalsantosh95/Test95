using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Services.Analytics;
using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Mapper.ValidationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.BrandMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderNotificationMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.OrderSearchMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.PublishOrderDocMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.ValidationMS;
using VAA.EAPI.NDC.Order.Mapper.Order;

namespace VAA.EAPI.NDC.Order.Application.OrderChangeNotif
{
    public class TtlExpiryCommandHandler : ITtlExpiryCommand
    {
        private readonly ILogger<TtlExpiryCommandHandler> _logger;
        private readonly PublishOrderDocMSCommandHandler _publishOrderDocMSCommandHandler;
        private readonly OrderDataMSQueryHandler _orderDataMSQueryHandler;
        private readonly ValidationMSAgencyOCNDetailsQueryHandler _agencyOCNDetailsQueryHandler;
        private readonly OrderSearchMSCommandHandler _orderSearchMSCommandHandler;
        private readonly ValidationMSCommandHandler _validationMSCommandHandler;
        private readonly IOrderNotificationMSCommand _orderNotificationMSCommand;

        private readonly IBrandMSCommand _brandMSCommand;
        private readonly IBrandMSPTCCommand _brandMSPTCCommand;
        private readonly ITrackAnalyticsEventCommandHandler _trackAnalyticsEventCommandHandler;

        public TtlExpiryCommandHandler(ILogger<TtlExpiryCommandHandler> logger, PublishOrderDocMSCommandHandler publishOrderDocMSCommandHandler, OrderDataMSQueryHandler orderDataMSQueryHandler, ValidationMSAgencyOCNDetailsQueryHandler agencyOCNDetailsQueryHandler, OrderSearchMSCommandHandler orderSearchMSCommandHandler, ValidationMSCommandHandler validationMSCommandHandler, IOrderNotificationMSCommand orderNotificationMSCommand, IBrandMSCommand brandMSCommand, IBrandMSPTCCommand brandMSPTCCommand, ITrackAnalyticsEventCommandHandler trackAnalyticsEventCommandHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _publishOrderDocMSCommandHandler = publishOrderDocMSCommandHandler;
            _orderDataMSQueryHandler = orderDataMSQueryHandler;
            _agencyOCNDetailsQueryHandler = agencyOCNDetailsQueryHandler;
            _orderSearchMSCommandHandler = orderSearchMSCommandHandler;
            _validationMSCommandHandler = validationMSCommandHandler;
            _orderNotificationMSCommand = orderNotificationMSCommand;
            _brandMSCommand = brandMSCommand ?? throw new ArgumentNullException(nameof(brandMSCommand));
            _brandMSPTCCommand = brandMSPTCCommand ?? throw new ArgumentNullException(nameof(brandMSPTCCommand));
            _trackAnalyticsEventCommandHandler = trackAnalyticsEventCommandHandler ?? throw new ArgumentNullException(nameof(trackAnalyticsEventCommandHandler));
        }
        public async Task Handle(OcnTtlExpiryRQ request, string requestType, CancellationToken cancellationToken)
        {
            TrackAnalyticsEventCommand analyticsEventCommand = new()
            {
                Type = nameof(TrackingAnalyticsType.Microservice),
                Subject = Constants.Application.OrderChangeNotifTtlExpiryEventSubject
            };

            try
            {
                analyticsEventCommand.Track("NDCVersion", Constants.Application.NDCVersion);

                DateTime? dateOfRequest = DateTime.UtcNow;
                analyticsEventCommand.Track("DateofRequest", dateOfRequest);

                analyticsEventCommand.Track("RequestType", string.IsNullOrWhiteSpace(requestType)
                    ? Constants.Application.OrderChangeNotifTtlExpiryEventSubject
                    : (requestType.StartsWith("OrderChangeNotif", StringComparison.OrdinalIgnoreCase)
                        ? requestType
                        : $"OrderChangeNotif-{requestType}"));
                analyticsEventCommand.Track("PNR", request.BookingReference.RLOC);

                analyticsEventCommand.Track("OrderID", $"VS932{request.BookingReference.RLOC}{DateTime.UtcNow:yy}");

                var orderRS = await _orderDataMSQueryHandler.Handle(new OrderDataMSQuery() { OrderID = $"VS932{request.BookingReference.RLOC}{DateTime.UtcNow:yy}" }, cancellationToken);

                if (orderRS?.OrderDocument is null) throw new InvalidOperationException("Unable to locate order document");

                var validationMSRequest = ValidationMSRequestMapper.MapValidationRequest(orderRS.OrderDocument.Parties, orderRS.OrderDocument.Parties.CorporateAccount, false, true);
                var validationMSResponse = await _validationMSCommandHandler.Handle(new ValidationMSCommand() { AgencyValidateRequest = validationMSRequest }, cancellationToken);

                if (validationMSResponse?.Error is not null)
                {
                    var exception = new InvalidOperationException($"Validation failed: {string.Join(Environment.NewLine, validationMSResponse.Error.Select(x => $"{x.Code}: {x.Message}"))}");

                    exception.Data.Add("Errors", validationMSResponse.Error);

                    throw exception;
                }

                var getReservationReq = OrderSearchMSRequestMapper.Mapper(orderRS.OrderDocument, false);
                var getReservationOrderDoc = await _orderSearchMSCommandHandler.Handle(new OrderSearchMSCommand(getReservationReq), cancellationToken);

                if (getReservationOrderDoc != null && getReservationOrderDoc.OrderItems.First(a => a.Services.First().Product.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum).AccountableDocumentRefs == null)
                {
                    if (validationMSResponse.ValidateResponse.OCNFlag)
                    {
                        var agencyOCNDetails = await _agencyOCNDetailsQueryHandler.Handle(new ValidationMSAgencyOCNDetailsQuery()
                        {
                            AgencyId = orderRS.OrderDocument.Parties.Participants.FirstOrDefault(p => p.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Seller)?.AgencyID,
                            AggregatorId = orderRS.OrderDocument.Parties.Participants.FirstOrDefault(p => p.PartyType == Infrastructure.Model.Participant.PartyTypeEnum.Distributor)?.AgencyID,
                            Environment = ConfigReader.Get(Constants.Environment.AgentRegistrationEnvironment)
                        }, cancellationToken);

                        if (agencyOCNDetails != null)
                        {
                            var agentResponse = await _orderNotificationMSCommand.Handle(new()
                            {
                                OrderDocument = getReservationOrderDoc,
                                AgentEndpointDetails = agencyOCNDetails.Select(x =>
                                {
                                    return new Infrastructure.Model.OrderNotificationMS.AgencyOCNDetails()
                                    {
                                        AgencyDetails = x.AgencyDetails,
                                        EndpointDetails = x.EndpointDetails
                                    };
                                }),
                                TTLExpiryType = requestType
                            });

                            if (agentResponse.OrderNotificationAgentResponse != null)
                            {
                                foreach (var (x, index) in agentResponse.OrderNotificationAgentResponse.AgentResponse.Select((x, index) => (x, index)))
                                {
                                    int i = index;
                                    i++;
                                    analyticsEventCommand.Track($"Agent{i}", x.AgentId);
                                    analyticsEventCommand.Track($"Agent{i}AckStatus", x.AcknowledgementRcvd ? "Y" : "N");
                                }
                            }
                        }
                    }

                    await RetainDetailsFromOldOrderDoc(orderRS, getReservationOrderDoc);

                    var publishOrderDocRequest = new UpdateOrderRequest { OrderDocument = getReservationOrderDoc };

                    await _publishOrderDocMSCommandHandler.Handle(new PublishOrderDocMSCommand(publishOrderDocRequest), cancellationToken);

                }
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status200OK);
            }
            catch (Exception ex)
            {
                var errorMessages = string.Empty;

                if (ex.Data.Contains("Errors"))
                {
                    IEnumerable<ErrorDetails> errors = (IEnumerable<ErrorDetails>)ex.Data["Errors"];

                    errorMessages = $", Errors{string.Join(Environment.NewLine, errors.Select(x => $"{x.Code}: {x.Message}"))}";
                }

                _logger.LogError(ex, $"Error occurred - {ex.Message}{errorMessages}");
                analyticsEventCommand.Track("StatusCode", StatusCodes.Status500InternalServerError);
            }
            finally
            {
                await _trackAnalyticsEventCommandHandler.Handle(analyticsEventCommand, cancellationToken);
            }
        }

        private async Task RetainDetailsFromOldOrderDoc(OrderRS orderRS, OrderDocument getReservationOrderDoc)
        {
            var contact = orderRS.OrderDocument.DataList.CustomerDetails.First().Contacts;

            if (contact?.CountryDailingCode != null)
            {
                getReservationOrderDoc.DataList.CustomerDetails.First().Contacts.CountryDailingCode = contact.CountryDailingCode;
            }

            if (orderRS.OrderDocument.FareTypeRequest != null) getReservationOrderDoc.FareTypeRequest = orderRS.OrderDocument.FareTypeRequest;

            foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails.Where(a => a.IsNameChanged))
            {
                getReservationOrderDoc.DataList.CustomerDetails.First(x => x.CustId == customer.CustId).IsNameChanged = true;
            }

            foreach (var customer in orderRS.OrderDocument.DataList.CustomerDetails)
            {
                var getReservationOrderDocCustomer = getReservationOrderDoc.DataList.CustomerDetails.FirstOrDefault(x => x.CustId == customer.CustId);

                if (getReservationOrderDocCustomer != null)
                {
                    getReservationOrderDocCustomer.ItaPtc = customer.ItaPtc;
                }
            }

            foreach (var orderItem in orderRS.OrderDocument.OrderItems.Where(a => a.OCNTimestamp != null))
            {
                getReservationOrderDoc.OrderItems.First(x => x.OrderItemId == orderItem.OrderItemId).OCNTimestamp = orderItem.OCNTimestamp;
            }

            if (orderRS.OrderDocument.OrderItems.Any(x => !string.IsNullOrEmpty(x.OrderItemId) && char.IsLetter(x.OrderItemId.Last())))
            {
                OrderDocumentMapper.MapOrderItems(orderRS.OrderDocument, getReservationOrderDoc);
            }

            getReservationOrderDoc = BrandMSRequestMapper.UpdateFares(orderRS.OrderDocument, getReservationOrderDoc);
            IEnumerable<Infrastructure.Model.BrandMS.BrandRS> brandResponse;

            if (getReservationOrderDoc.DataList.CustomerDetails.Any(x => x.Ptc == Constants.Application.INF))
            {
                var brandMsReq = BrandMSRequestMapper.MapBrandMSPTCOrderRequest(getReservationOrderDoc);
                brandResponse = await _brandMSPTCCommand.Handle(brandMsReq, CancellationToken.None);

            }
            else
            {
                var brandMsReq = BrandMSRequestMapper.MapBrandMSOrderRequest(getReservationOrderDoc);
                brandResponse = await _brandMSCommand.Handle(brandMsReq, CancellationToken.None);
            }

            getReservationOrderDoc = BrandMSResponseMapper.MapFareComponents(getReservationOrderDoc, brandResponse, orderRS.OrderDocument);

            bool isCashPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum) ?? false;
            bool isEasyPayment = orderRS.OrderDocument.OrderItems.FirstOrDefault()?.PaymentDetails?.Any(x => x.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.EasyPay) ?? false;

            if (isCashPayment || isEasyPayment)
            {
                foreach (var orderItem in getReservationOrderDoc.OrderItems)
                {
                    orderItem.PaymentDetails = orderRS.OrderDocument.OrderItems.First().PaymentDetails;
                }
            }
        }

    }
}
