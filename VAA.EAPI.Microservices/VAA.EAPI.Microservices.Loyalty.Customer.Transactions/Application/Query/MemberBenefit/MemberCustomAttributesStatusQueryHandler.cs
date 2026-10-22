using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.MemberBenefit
{
    public class MemberCustomAttributesStatusQueryHandler : CommandStatusBaseQueryHandler<MemberCustomAttributesCommandQueryResponse>, IRequestHandler<MemberCustomAttributesStatusQuery, Result<Response<MemberCustomAttributesCommandQueryResponse>>>, IMemberCustomAttributesStatusQueryHandler
    {
        public MemberCustomAttributesStatusQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<MemberCustomAttributesCommandQueryResponse>> logger) : base(mediator, logger)
        {
        }

        public async Task<Result<Response<MemberCustomAttributesCommandQueryResponse>>> Handle(MemberCustomAttributesStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override Response<MemberCustomAttributesCommandQueryResponse> CreateResponseFromEvent(string requestId, string commandEventOutcome)
        {
            var documentEvent = Json.Deserialize<Result<Response<MemberCustomAttributesCosmosDocumentEvent>>>(commandEventOutcome);

            return new Response<MemberCustomAttributesCommandQueryResponse>()
            {
                Status = Status.OK,
                Value = new MemberCustomAttributesCommandQueryResponse()
                {
                    RequestId = requestId,
                    CommandStatus = documentEvent.Value is not null && documentEvent.Value.Status != Status.OK ? Status.BusinessException : Status.OK,
                    MemberCustomAttributesDocument = documentEvent.Value.Value is null ? null : new(documentEvent.Value.Value.Document),
                    Errors = documentEvent.Value is not null && documentEvent.Value.Errors != null ? documentEvent.Value.Errors : null
                }
            };

        }
    }
}
