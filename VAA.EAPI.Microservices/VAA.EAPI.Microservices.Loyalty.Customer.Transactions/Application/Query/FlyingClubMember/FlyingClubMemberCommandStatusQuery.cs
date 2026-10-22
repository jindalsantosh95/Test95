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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.FlyingClubMember
{
    public class FlyingClubMemberCommandStatusQuery : CommandStatusQueryBase, IRequest<Result<Response<FlyingClubMemberCommandQueryResponse>>>
    {
    }

    public class FlyingClubMemberCommandStatusQueryHandler : CommandStatusBaseQueryHandler<FlyingClubMemberCommandQueryResponse>, IRequestHandler<FlyingClubMemberCommandStatusQuery, Result<Response<FlyingClubMemberCommandQueryResponse>>>
    {
        public FlyingClubMemberCommandStatusQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<FlyingClubMemberCommandQueryResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<FlyingClubMemberCommandQueryResponse>>> Handle(FlyingClubMemberCommandStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override Response<FlyingClubMemberCommandQueryResponse> CreateResponseFromEvent(string requestId, string commandEventOutcome)
        {
            var documentEvent = Json.Deserialize<Result<Response<FlyingClubMemberCosmosDocumentEvent>>>(commandEventOutcome);

            return new Response<FlyingClubMemberCommandQueryResponse>()
            {
                Status = Status.OK,
                Value = new FlyingClubMemberCommandQueryResponse()
                {
                    RequestId = requestId,
                    CommandStatus = documentEvent.Value is not null && documentEvent.Value.Status != Status.OK ? Status.BusinessException : Status.OK,
                    FlyingClubMemberDocument = documentEvent.Value.Value is null ? null : new(documentEvent.Value.Value.Document),
                    Errors = documentEvent.Value is not null && documentEvent.Value.Errors != null ? documentEvent.Value.Errors : null
                }
            };

        }
    }
}
