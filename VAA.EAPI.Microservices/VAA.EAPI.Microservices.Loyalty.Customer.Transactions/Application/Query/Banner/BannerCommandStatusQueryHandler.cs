using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.BaseClasses;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.CommandStatus;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Application.Query.Banner
{
    public class BannerCommandStatusQueryHandler(IMediator mediator, ILogger<CommandStatusBaseQueryHandler<BannerCommandQueryResponse>> logger) : CommandStatusBaseQueryHandler<BannerCommandQueryResponse>(mediator, logger), IRequestHandler<BannerCommandStatusQuery, Result<Response<BannerCommandQueryResponse>>>
    {
        public async Task<Result<Response<BannerCommandQueryResponse>>> Handle(BannerCommandStatusQuery request, CancellationToken cancellationToken)
        {
            return await base.Handle(request, cancellationToken);
        }

        public override Response<BannerCommandQueryResponse> CreateResponseFromEvent(string requestId, string commandEventOutcome)
        {
            var documentEvent = Json.Deserialize<Result<Response<BannerCosmosDocumentEvent>>>(commandEventOutcome);

            return new Response<BannerCommandQueryResponse>()
            {
                Status = Status.OK,
                Value = documentEvent.Value.Status == Status.OK ? new BannerCommandQueryResponse()
                {
                    CommandStatus = Status.OK,
                    RequestId = requestId,
                    Banner = documentEvent.Value.Value is null ? null : (BannerDetail)documentEvent.Value.Value.Document
                } : new BannerCommandQueryResponse()
                {
                    CommandStatus = Status.BusinessException,
                    RequestId = requestId,
                    Errors = documentEvent.Value.Errors
                }
            };

        }
    }
}
