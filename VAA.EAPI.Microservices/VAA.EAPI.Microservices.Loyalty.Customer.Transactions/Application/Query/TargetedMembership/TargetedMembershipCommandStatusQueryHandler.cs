using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.TargetedMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.TargetedMembership
{
    public class TargetedMembershipCommandStatusQuery : CommandStatusQueryBase, IRequest<Result<Response<TargetedMembershipCommandQueryResponse>>>
    {
    }

    public class TargetedMembershipCommandStatusQueryHandler : CommandStatusBaseQueryHandler<TargetedMembershipCommandQueryResponse>, IRequestHandler<TargetedMembershipCommandStatusQuery, Result<Response<TargetedMembershipCommandQueryResponse>>>
    {
        public TargetedMembershipCommandStatusQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<TargetedMembershipCommandQueryResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<TargetedMembershipCommandQueryResponse>>> Handle(TargetedMembershipCommandStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override Response<TargetedMembershipCommandQueryResponse> CreateResponseFromEvent(string requestId, string commandEventOutcome)
        {
            var documentEvent = Json.Deserialize<Result<Response<TargetedMembershipCosmosDocumentEvent>>>(commandEventOutcome);


            // *** CURRENTLY NON FUNCTIONAL IF THE EVENT HANDLER HAD AN ERROR *** //

            //if (documentEvent.Value.Value.Document.DocType != ConfigReader.Get(Constants.EnvironmentSettings.TargetedMembershipDocType))
            //    return NotFoundResponse(requestId);

            if (documentEvent.Value.Status == Status.OK)
                return new Response<TargetedMembershipCommandQueryResponse>()
                {
                    Status = Status.OK,
                    Value = new TargetedMembershipCommandQueryResponse()
                    {
                        CommandStatus = Status.OK,
                        RequestId = requestId,
                        TargetedMembershipDocument = documentEvent.Value.Value is null ? null     // Document could be null if the request was a delete
                                                                        : new(documentEvent.Value.Value.Document)
                    }
                };
            else
                return new Response<TargetedMembershipCommandQueryResponse>()
                {
                    Status = Status.OK,
                    Value = new TargetedMembershipCommandQueryResponse()
                    {
                        CommandStatus = Status.BusinessException,
                        RequestId = requestId,
                        Errors = documentEvent.Value.Errors
                    }
                };
        }
    }
}
