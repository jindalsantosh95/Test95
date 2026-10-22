using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment
{
    public class Air4PaymentCaptureCommandHandler : IRequestHandler<Air4PaymentCaptureCommand, Result<Response<PaymentTransactionDetails>>>
    {
        private readonly ILogger<Air4PaymentCaptureCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public Air4PaymentCaptureCommandHandler(ILogger<Air4PaymentCaptureCommandHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<Result<Response<PaymentTransactionDetails>>> Handle(Air4PaymentCaptureCommand request, CancellationToken cancellationToken)
        {
            Dictionary<string, string> headers = new() { { "paymentReferenceId", request.PaymentReferenceId }, { "appID", ConfigReader.Get(Constants.Environment.DlPaymentApiApplicationId) }, { "channelId", ConfigReader.Get(Constants.Environment.DlPaymentApiChannelId) } };

            Air4CapturePaymentRequest air4CapturePaymentRequest = new() { formsOfPayments = new FormsOfPayment[] { new FormsOfPayment { PaymentMethod = new PaymentMethod { PaymentMethodTypeName = request.PaymentType == Domain.Enums.PaymentType.PayPal ? Constants.Application.DlPayPal : request.PaymentType == Domain.Enums.PaymentType.GooglePay ? ConfigReader.Get(Constants.Environment.DlGooglePay) : Constants.Application.DlApplePay }, PaymentReferenceId = request.PaymentReferenceId, VendorId = request.VendorId, PurchaseInfo = new PurchaseInfo { ElectronicDocumentNum = request.TicketNumber, RecordLocatorId = request.RLOC } } } };

            var captureResponse = await _apiConnector.SendAsync<Air4formsOfPaymentsResponse>(ConfigReader.Get(Constants.Environment.DlPaymentApiCaptureUrl), headers: headers, bearerToken: await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlPaymentClientId), ConfigReader.Get(Constants.Environment.DlPaymentClientSecret)), verb: System.Net.Http.HttpMethod.Put, content: Json.Serialize(air4CapturePaymentRequest));

            var result = DeltaPaymentMapper.FormsOfPaymentsResponseMapper(captureResponse);

            return new Response<PaymentTransactionDetails>
            {
                Status = Status.OK,
                Value = result,
                IgnoreNullValues = false,
            };
        }
    }
}