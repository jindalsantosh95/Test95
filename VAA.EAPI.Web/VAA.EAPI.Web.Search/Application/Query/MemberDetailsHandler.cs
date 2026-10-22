using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Loyalty;

namespace VAA.EAPI.Web.Search.Application.Query
{
    public class MemberDetailsHandler : IRequestHandler<MemberDetailsRequest, MemberDetailsResponse>
    {
        private readonly IMediator _mediator;

        public MemberDetailsHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<MemberDetailsResponse> Handle(MemberDetailsRequest request, CancellationToken cancellationToken)
        {
            var graphQLRequest = new GraphQLRequest
            {
                PqId = ConfigReader.Get(Constants.Environment.GraphQLMemberDetailsPqId),
                Variables = JObject.Parse(Json.Serialize(new AccountManagerQuery() { ActivityCount = 0, FlightsCount = 0, VoucherCount = 0 }, false)),
                Headers = new Dictionary<string, string>()
                {
                    { "Token", request.AuthToken}
                },
                GraphqlUrl = ConfigReader.Get(Constants.Environment.GraphQLMyAccountUrl)
            };

            return Json.DeserializeObject<MemberDetailsResponse>((await _mediator.Send(graphQLRequest, cancellationToken)).Data[ConfigReader.Get(Constants.Environment.GraphQLMemberDetailsPqId)]);
        }
    }
}
