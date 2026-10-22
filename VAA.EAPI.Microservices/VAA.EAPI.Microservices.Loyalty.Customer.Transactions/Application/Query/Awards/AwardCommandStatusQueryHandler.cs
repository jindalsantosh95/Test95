using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Awards;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Awards
{
    public class AwardCommandStatusQuery : CommandStatusQueryBase, IRequest<Result<Response<AwardCommandQueryResponse>>>
    {
    }

    public class AwardCommandStatusQueryHandler : CommandStatusBaseQueryHandler<AwardCommandQueryResponse>, IRequestHandler<AwardCommandStatusQuery, Result<Response<AwardCommandQueryResponse>>>
    {
        public AwardCommandStatusQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<AwardCommandQueryResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<AwardCommandQueryResponse>>> Handle(AwardCommandStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override Response<AwardCommandQueryResponse> CreateResponseFromEvent(string requestId, string commandEventOutcome)
        {
            var documentEvent = Json.Deserialize<Result<Response<AwardCosmosDocumentEvent>>>(commandEventOutcome);

            // *** CURRENTLY NON FUNCTIONAL IF THE EVENT HANDLER HAD AN ERROR *** //

            //if (documentEvent.Value.Value.Document.DocType != ConfigReader.Get(Constants.EnvironmentSettings.AwardDocType))
            //    return NotFoundResponse(requestId);

            if (documentEvent.Value.Status == Status.OK)
                return new Response<AwardCommandQueryResponse>()
                {
                    Status = Status.OK,
                    Value = new AwardCommandQueryResponse()
                    {
                        CommandStatus = Status.OK,
                        RequestId = requestId,
                        Award = documentEvent.Value.Value is null ? null     // Document could be null if the request was a delete
                                                                  : new Award(documentEvent.Value.Value.Document)
                    }
                };
            else
                return new Response<AwardCommandQueryResponse>()
                {
                    Status = Status.OK,
                    Value = new AwardCommandQueryResponse()
                    {
                        CommandStatus = Status.BusinessException,
                        RequestId = requestId,
                        Errors = documentEvent.Value.Errors
                    }
                };
        }
    }
}
