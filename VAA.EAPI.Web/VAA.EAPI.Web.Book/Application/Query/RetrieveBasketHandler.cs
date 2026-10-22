using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Web.Book.Helper;
using VAA.EAPI.Web.Book.Mapper;
using VAA.EAPI.Web.Book.Models.YourDetails;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Entities;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Infrastructure.Entities.GraphQL;
using VAA.EAPI.Web.Common.Infrastructure.Exceptions;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Book.Application.Query
{
    public class RetrieveBasketHandler : IRequestHandler<RetrieveBasketRequest, YourDetailsResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<RetrieveBasketHandler> _logger;

        public RetrieveBasketHandler(IMediator mediator, ILogger<RetrieveBasketHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<YourDetailsResponse> Handle(RetrieveBasketRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.RetrieveBasketDetailsPqId),
                    Variables = JObject.Parse(Json.Serialize(new
                    {
                        basketId = request.BasketId
                    })),
                    Headers = Utility.GetHeaders(request.AuthToken)
                };

                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);
                var mapperResponse = BasketMapper.YourDetailsMapper(Json.DeserializeObject<YourDetailsResponse>(graphQLResponse.Data["retrieveBasket"]));
                return mapperResponse;
            }            
            catch (Exception ex)
            {
                _logger.LogError(ex, $"CorrelationId: {AsyncContext.Current.CorrelationId}, Error in BasketRetrieveRequestHandler: {ex.Message}");
                throw;
            }
        }
    }
}
