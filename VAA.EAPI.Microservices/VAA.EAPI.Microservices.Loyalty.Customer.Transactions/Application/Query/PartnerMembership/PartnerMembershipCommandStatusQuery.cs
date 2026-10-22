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
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.PartnerMembership;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.PartnerMembership
{
    public class PartnerMembershipCommandStatusQuery : CommandStatusQueryBase, IRequest<Result<Response<PartnerMembershipCommandQueryResponse>>>
    {
    }

    public class PartnerMembershipCommandStatusQueryHandler : CommandStatusBaseQueryHandler<PartnerMembershipCommandQueryResponse>, IRequestHandler<PartnerMembershipCommandStatusQuery, Result<Response<PartnerMembershipCommandQueryResponse>>>
    {
        public PartnerMembershipCommandStatusQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<PartnerMembershipCommandQueryResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<PartnerMembershipCommandQueryResponse>>> Handle(PartnerMembershipCommandStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override Response<PartnerMembershipCommandQueryResponse> CreateResponseFromEvent(string requestId, string commandEventOutcome)
        {
            var documentEvent = Json.Deserialize<Result<Response<PartnerMembershipCosmosDocumentEvent>>>(commandEventOutcome);

            return new Response<PartnerMembershipCommandQueryResponse>()
            {
                Status = Status.OK,
                Value = new PartnerMembershipCommandQueryResponse()
                {
                    RequestId = requestId,
                    CommandStatus = documentEvent.Value is not null && documentEvent.Value.Status != Status.OK ? Status.BusinessException : Status.OK,
                    PartnerMembershipDocument = documentEvent.Value is not null && documentEvent.Value.Value is not null ? (Domain.PartnerMembership.PartnerMembership)documentEvent.Value.Value.Document : null,
                    Errors = documentEvent.Value is not null && documentEvent.Value.Errors != null ? documentEvent.Value.Errors : null
                }
            };

        }
    }
}
