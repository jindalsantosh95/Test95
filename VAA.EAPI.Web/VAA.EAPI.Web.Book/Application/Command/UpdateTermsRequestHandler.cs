using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Domain.Basket.Requests;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Mapper;
using VAA.EAPI.Web.Book.Models.YourDetails;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Application.Command
{
    public class UpdateTermsRequestHandler : IRequestHandler<UpdateTermsRequest, YourDetailsResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateTermsRequestHandler> _logger;

        public UpdateTermsRequestHandler(IMediator mediator, ILogger<UpdateTermsRequestHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<YourDetailsResponse> Handle(UpdateTermsRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.UpdateTnCMutationPqId),
                    Variables = JObject.Parse(Json.Serialize(new UpdateTnCGraphQLRequest()
                    {
                        UpdateTnCRequest = new(request)
                    })),
                    Headers = Utility.GetHeaders(request.AuthToken)
                };
                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);
                var mapperResponse = BasketMapper.YourDetailsMapper(Json.DeserializeObject<YourDetailsResponse>(graphQLResponse.Data["updateTnC"]));
                return mapperResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BaskeId:{request.BasketId}, CorrelationId: {AsyncContext.Current.CorrelationId}, Error in BasketTnCHandler: {ex.Message}");
                throw;
            }
        }
    }
}
