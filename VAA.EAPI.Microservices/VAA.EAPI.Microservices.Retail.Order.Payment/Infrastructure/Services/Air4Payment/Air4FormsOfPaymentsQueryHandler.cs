using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
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
    public class Air4FormsOfPaymentsQueryHandler : IRequestHandler<Air4FormsOfPaymentsQuery, Result<Response<PaymentTransactionDetails>>>
    {
        private readonly ILogger<Air4FormsOfPaymentsQueryHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;
        private readonly ICachedChannel _cachedChannel;

        public Air4FormsOfPaymentsQueryHandler(ILogger<Air4FormsOfPaymentsQueryHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator, ICachedChannel cachedChannel)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
            _cachedChannel = cachedChannel ?? throw new ArgumentNullException(nameof(cachedChannel));
        }

        public async Task<Result<Response<PaymentTransactionDetails>>> Handle(Air4FormsOfPaymentsQuery request, CancellationToken cancellationToken)
        {
            var _headers = new Dictionary<string, string>();
            Channel channelEntity = null;

            if (!string.IsNullOrEmpty(request.Channel))
                channelEntity = await _cachedChannel.GetChannelEntity(request.Channel);

            if (!channelEntity.IsNullOrDefault())
            {
                _headers = new Dictionary<string, string>
                    {
                        { "paymentReferenceId", request.PaymentReferenceId },
                        { "appID", channelEntity.DlApplicationId },
                        { "channelId", channelEntity.ChannelId }                         
                    };
            }
            else
            {
                _headers = new() 
                { 
                    { "paymentReferenceId", request.PaymentReferenceId },
                    { "appID", ConfigReader.Get(Constants.Environment.DlPaymentApiApplicationId) },
                    { "channelId", ConfigReader.Get(Constants.Environment.DlPaymentApiChannelId) }
                };
            }
            
            var apiResponse = await _apiConnector.SendAsync<Air4formsOfPaymentsResponse>(
                ConfigReader.Get(Constants.Environment.DlPaymentApiFormsOfPaymentsUrl),
                headers: _headers,
                bearerToken: await _air4TokenGenerator.GetToken(
                    ConfigReader.Get(Constants.Environment.DlPaymentClientId),
                    ConfigReader.Get(Constants.Environment.DlPaymentClientSecret)
                ),
                verb: HttpMethod.Get,
                content: new StringContent("", Encoding.UTF8, "text/plain")
            );
            var result = DeltaPaymentMapper.FormsOfPaymentsResponseMapper(apiResponse);

            return new Response<PaymentTransactionDetails>
            {
                Status = Status.OK,
                Value = result,
                IgnoreNullValues = false,
            };
        }
    }
}
