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
using System.Net.Http;
using System.Text;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Helper.Interface;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.Common;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Air4Payment
{
    public class Air4PaymentDetailsQueryHandler : IRequestHandler<Air4PaymentDetailsQuery, Result<Response<PaymentTransactionDetails>>>
    {
        private readonly ILogger<Air4PaymentDetailsQueryHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;
        private readonly ICachedChannel _cachedChannel;

        public Air4PaymentDetailsQueryHandler(ILogger<Air4PaymentDetailsQueryHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator, ICachedChannel cachedChannel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
        }
        public async Task<Result<Response<PaymentTransactionDetails>>> Handle(Air4PaymentDetailsQuery request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>();
            Channel channelEntity = null;

            if (!string.IsNullOrEmpty(request.Channel))
                channelEntity = await _cachedChannel.GetChannelEntity(request.Channel);

            if (!channelEntity.IsNullOrDefault())
            {
                headers = new Dictionary<string, string>
                    {                        
                        {"appID", channelEntity.DlApplicationId},
                        {"channelId", channelEntity.ChannelId},
                        {"purchasePath", channelEntity.DlPaymentPurchasePath},
                        {"objectName", ConfigReader.Try(Constants.Environment.DlPaymentobjectName)},
                    };
            }
            else
            {
                headers = new Dictionary<string, string>
                    {
                        {"appID", ConfigReader.Get(Constants.Environment.DlPaymentApiApplicationId)},
                        {"channelId", ConfigReader.Get(Constants.Environment.DlPaymentApiChannelId)},
                        {"purchasePath", ConfigReader.Get(Constants.Environment.DlPaymentPurchasePath)},
                        {"objectName", ConfigReader.Get(Constants.Environment.DlPaymentobjectName)},
                    };
            }           

            var paymentDetails = await _apiConnector.SendAsync<Air4PaymentDetailsResponse>(
                ConfigReader.Get(Constants.Environment.DlPaymentApiGetPaymentDetailsUrl).Replace("{paymentReferenceId}", request.PaymentReferenceId),
                headers: headers,
                bearerToken: await _air4TokenGenerator.GetToken(
                    ConfigReader.Get(Constants.Environment.DlPaymentClientId),
                    ConfigReader.Get(Constants.Environment.DlPaymentClientSecret)
                ),
                verb: HttpMethod.Get,
                content: new StringContent("", Encoding.UTF8, "text/plain")
            );

            var result = DeltaPaymentMapper.PaymentDetailsResponseMapper(Json.Deserialize<Air4PaymentDetails>(paymentDetails.ObjectContentText));

            return new Response<PaymentTransactionDetails>
            {
                Status = Status.OK,
                Value = result,
                IgnoreNullValues = false,
            };
        }
    }
}
