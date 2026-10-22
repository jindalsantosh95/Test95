using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query.CPD
{
    public class CreatePaymentDocumentQueryHandler : ICreatePaymentDocumentQuery
    {

        private readonly ILogger<CreatePaymentDocumentQueryHandler> _logger;
        private readonly ServiceResponseQueryHandler _serviceResponseQueryHandler;

        public CreatePaymentDocumentQueryHandler(ILogger<CreatePaymentDocumentQueryHandler> logger, ServiceResponseQueryHandler serviceResponseQueryHandler)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _serviceResponseQueryHandler = serviceResponseQueryHandler ?? throw new ArgumentNullException(nameof(serviceResponseQueryHandler));
        }

        public async Task<Result<Response<PaymentResponse>>> Handle(string CorrelationId, string Type, CancellationToken cancellationToken)
        {
            Response<PaymentResponse> createpaymentDocResponse = new();
            try
            {
                int retryCounter = 1;
                bool responseProcessed = false;
                int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);

                while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
                {
                    var result = await _serviceResponseQueryHandler.Handle(new ServiceResponseQuery { CorrelationId = CorrelationId, EventType = Type }, cancellationToken);
                    var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                    switch (result.Status)
                    {

                        case ServiceResponseProcessStatus.UpdatePending:
                        case ServiceResponseProcessStatus.Processing:

                            await Task.Delay(timespan);

                            break;
                        case ServiceResponseProcessStatus.Completed:

                            createpaymentDocResponse = CreatePaymentQueryResponseMapper.QueryResponseMapper(result);
                            responseProcessed = true;

                            break;
                        case ServiceResponseProcessStatus.Error:

                            responseProcessed = true;

                            return Result.Failure<Response<PaymentResponse>>(result.JsonResponseData);
                    }

                    retryCounter++;
                }

                return createpaymentDocResponse.Value != null ? Result.Success(createpaymentDocResponse) : Result.Failure<Response<PaymentResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
            catch
            {
                return Result.Failure<Response<PaymentResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
