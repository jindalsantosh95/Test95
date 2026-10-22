using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.Interfaces;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.InsurancePurchase;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.Insurance;


namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services.InsurancePurchase
{
    public class InsurancePurchaseServiceCommandHandler : IRequestHandler<InsurancePurchaseServiceCommand, Result<Response<PurchaseResponse>>>
    {
        private readonly ILogger<InsurancePurchaseServiceCommandHandler> _logger;
        private readonly IHttpConnector _apiConnector;
        private readonly IAir4TokenGenerator _air4TokenGenerator;

        public InsurancePurchaseServiceCommandHandler(ILogger<InsurancePurchaseServiceCommandHandler> logger, IHttpConnector apiConnector, IAir4TokenGenerator air4TokenGenerator)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _apiConnector = apiConnector ?? throw new ArgumentNullException(nameof(apiConnector));
            _air4TokenGenerator = air4TokenGenerator ?? throw new ArgumentNullException(nameof(air4TokenGenerator));
        }

        public async Task<Result<Response<PurchaseResponse>>> Handle(InsurancePurchaseServiceCommand request, CancellationToken cancellationToken)
        {
            InsurancePurchaseRequest dlInsurancePurchaseRequest = null;
            try
            {
                dlInsurancePurchaseRequest = InsurancePurchaseMapper.PurchaseRequestMapper(request.InsurancePurchaseRequest);
               
                var headers = new Dictionary<string, string>
                    {
                         {"transactionId", request.InsurancePurchaseRequest.TransactionID},
                         {"channelId", ConfigReader.Get(Constants.Environment.DlPaymentApiChannelId)},
                         {"paymentReferenceId", request.InsurancePurchaseRequest.PaymentReferenceId},
                         {"Accept", "application/json"},
                    };

                var insuranceResponse = await _apiConnector.SendAsync<InsurancePurchaseResponse>(ConfigReader.Get(Constants.Environment.DlPaymentApiPurchaseInsuranceUri), headers: headers, bearerToken: await _air4TokenGenerator.GetToken(ConfigReader.Get(Constants.Environment.DlPaymentClientId), ConfigReader.Get(Constants.Environment.DlPaymentClientSecret)), verb: System.Net.Http.HttpMethod.Post, content: Json.Serialize(dlInsurancePurchaseRequest, false));

                _logger.LogInformation($"DL RS INSURANCE PURCHASE Response: {Json.Serialize(insuranceResponse)}");

                return Result.Success(new Response<PurchaseResponse>
                {
                    Status = EAPI.Domain.Entities.Common.Status.OK,
                    Value = InsurancePurchaseMapper.PurchaseResponseMapper(insuranceResponse.ProductResponseXml)
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} InsurancePurchase MS RQ-{Json.Serialize(InsurancePurchaseMapper.MaskInfo(request.InsurancePurchaseRequest), false)} Error in InsurancePurchaseServiceCommandHandler with DL Request: - {Json.Serialize(InsurancePurchaseMapper.MaskInfo(dlInsurancePurchaseRequest, request.InsurancePurchaseRequest), false)} - DL Response {ex.Data["ResponseData"]?.ToString()}");
                return Result.Failure<Response<PurchaseResponse>>((ex.Data["ResponseData"] ?? ex).ToString());
            }
        }
    }
}
