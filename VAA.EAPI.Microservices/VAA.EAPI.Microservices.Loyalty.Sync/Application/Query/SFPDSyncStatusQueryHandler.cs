using MediatR;
using System.Threading;
using System.Threading.Tasks;
using CSharpFunctionalExtensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.SFPD;
using VAA.EAPI.Microservices.Loyalty.Sync.Application.Query.BaseClasses;
using Microsoft.Extensions.Logging;
using VAA.EAPI.Infrastructure.Helper;
using System;
using System.Collections.Generic;
using System.Linq;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Application.Query
{
    public class SFPDSyncStatusQueryHandler : EventStatusBaseQueryHandler<SFPDSyncStatusQueryResponse>, IRequestHandler<SFPDSyncStatusQuery, Result<Response<SFPDSyncStatusQueryResponse>>>
    {
        public SFPDSyncStatusQueryHandler(IMediator mediator, ILogger<EventStatusBaseQueryHandler<SFPDSyncStatusQueryResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<SFPDSyncStatusQueryResponse>>> Handle(SFPDSyncStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request);
        }

        public override Response<SFPDSyncStatusQueryResponse> CreateResponseFromEvent(string requestId, string eventOutcome)
        {
            var updateEvent = Json.Deserialize<Result<Response<string>>>(eventOutcome);

            return new Response<SFPDSyncStatusQueryResponse>()
            {
                Status = Status.OK,
                Value = new SFPDSyncStatusQueryResponse()
                {
                    RequestId = requestId,
                    CommandStatus = updateEvent.Value is not null && updateEvent.Value.Status != Status.OK ? Status.BusinessException : Status.OK,
                    PsdsSyncStatus = updateEvent.Value is not null ? updateEvent.Value.Value.ToString() : null,
                    Errors = updateEvent.IsFailure == true && updateEvent.Error != null ? GetError(Constants.ErrorCodes.UpdateFailureError) : null
                }
            };
        }

        private List<ErrorDetails> GetError(string errorCode)
        {
            return new List<ErrorDetails>
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                };
        }
    }

}