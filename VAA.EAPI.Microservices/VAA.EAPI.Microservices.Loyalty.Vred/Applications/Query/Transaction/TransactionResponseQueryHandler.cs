using System;
using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Vred.Mapper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Applications.Query.Transaction
{
    public class TransactionResponseQueryHandler : IRequestHandler<TransactionResponseQuery, Result<Response<TransactionQueryResponse>>>
    {
        private readonly IMediator _mediator;

        public TransactionResponseQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }
        
        public async Task<Result<Response<TransactionQueryResponse>>> Handle(TransactionResponseQuery request, CancellationToken cancellationToken)
        {
            Response<TransactionQueryResponse> response = new();

            int retryCounter = 1;
            bool responseProcessed = false;
            int responseDelayTimeInMS = ConfigReader.Get<int>(Constants.Environment.ResponseDelayTimeInMS);
           
            while (!responseProcessed && retryCounter <= ConfigReader.Get<int>(Constants.Environment.ResponsePollingCount))
            {
                ServiceResponse result = await _mediator.Send(new ServiceResponseQuery
                {
                    ConnectionString = ConfigReader.Get(Constants.Environment.MicroServiceDBKey),
                    TableName = ConfigReader.Get(Constants.Environment.MicroServiceResponseTableName),
                    CorrelationId = request.CorrelationId,
                    EventType = request.Type
                });
               
                var timespan = TimeSpan.FromMilliseconds(responseDelayTimeInMS * retryCounter);

                switch (result.Status)
                {
                    case ServiceResponseProcessStatus.UpdatePending:
                    case ServiceResponseProcessStatus.Processing:
                        await Task.Delay(timespan);
                        break;
                    case ServiceResponseProcessStatus.Completed:
                        responseProcessed = true;
                        response = TransactionResponseQueryMapper.ResponseMapper(result);
                        break;
                    case ServiceResponseProcessStatus.Error:
                        var vredError = Json.Deserialize<TransactionErrorResponse>(Json.Deserialize<TransactionQueryResponse>(result.JsonResponseData).Value);
                        ErrorDetails error = new() { Code = vredError.Status, Message = vredError.Message, ErrorSource = vredError.Url };

                        return Result.Success(new Response<TransactionQueryResponse>
                        {
                            Errors = new List<ErrorDetails>() { error },
                            Status = Status.BusinessException
                        });
                }
                retryCounter++;
            }
            
            return response.Value != null ? Result.Success(response) : Result.Failure<Response<TransactionQueryResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));           
        }
    }
}