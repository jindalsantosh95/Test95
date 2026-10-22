
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.CustomerDetail;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Repository.Services;
using VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Vred.Mapper;
namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.ManageAccount
{
    public class UpdateVredCustomerDetailsResponseQueryHandler : IVredCustomerDetailsResponseQuery
    {


        private readonly IServiceResponseQuery _serviceResponseQuery;
        public UpdateVredCustomerDetailsResponseQueryHandler(IServiceResponseQuery serviceResponseQuery)
        {

            _serviceResponseQuery = serviceResponseQuery ?? throw new ArgumentNullException(nameof(serviceResponseQuery));
        }

        public async Task<Response<UpdateVredCustomerDetailsQueryResponse>> Handle(UpdateVredCustomerDetailsResponseQuery request, CancellationToken cancellationToken)
        {
            Response<UpdateVredCustomerDetailsQueryResponse> response = new();

            int retryCounter = 1;
            bool responseProcessed = false;
            int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);


            while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
            {
                ServiceResponse result = await _serviceResponseQuery.Handle(new ServiceResponseQueryRequest
                {
                    ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey),
                    TableName = ConfigReader.Get(Constants.Environment.MicroServiceResponseTableName),
                    CorrelationId = request.CorrelationId,
                    EventType = request.Type
                }, cancellationToken);

                var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                switch (result.Status)
                {
                    case ServiceResponseProcessStatus.UpdatePending:
                    case ServiceResponseProcessStatus.Processing:
                        await Task.Delay(timespan, cancellationToken);
                        break;
                    case ServiceResponseProcessStatus.Completed:
                        responseProcessed = true;
                        response = UpdateVredCustomerDetailsQueryMapper.ResponseMapper(result);
                        break;
                    case ServiceResponseProcessStatus.Error:

                        var error = new ErrorDetails
                        {
                            Code = result.Status.ToString(),
                            Message = !string.IsNullOrWhiteSpace(result.JsonResponseData)
                            ? result.JsonResponseData
                            : result.Status.ToString(),
                            Id = result.Id
                        };

                        return new Response<UpdateVredCustomerDetailsQueryResponse>
                        {
                            Status = Status.BusinessException,
                            Errors = new List<ErrorDetails> { error }
                        };
                }
                retryCounter++;
            }


            return response.Value != null ? response : new Response<UpdateVredCustomerDetailsQueryResponse> { ErrorMessage = Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)) };
        }
    }
}
