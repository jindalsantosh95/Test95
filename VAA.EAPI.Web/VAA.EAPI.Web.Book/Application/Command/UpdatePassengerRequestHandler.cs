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
    public class UpdatePassengerRequestHandler : IRequestHandler<UpdatePassengerRequest, YourDetailsResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePassengerRequestHandler> _logger;

        public UpdatePassengerRequestHandler(IMediator mediator, ILogger<UpdatePassengerRequestHandler> logger)
        {
            _mediator = mediator ?? throw new System.ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
        }

        public async Task<YourDetailsResponse> Handle(UpdatePassengerRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.UpdatePassengerMuationReqPqId),
                    Variables = JObject.Parse(Json.Serialize(new BasketUpdatePassengerRequest(request))),
                    Headers = Utility.GetHeaders(request.AuthToken)
                };

                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);
                var mapperResponse = BasketMapper.YourDetailsMapper(Json.DeserializeObject<YourDetailsResponse>(graphQLResponse.Data["updatePeople"]));
                return mapperResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BasketId: {request.BasketId}, CorrelationId: {AsyncContext.Current.CorrelationId}, Error in BasketUpdatePassengerHandler: {ex.Message} ");
                throw;
            }
        }
    }
}
