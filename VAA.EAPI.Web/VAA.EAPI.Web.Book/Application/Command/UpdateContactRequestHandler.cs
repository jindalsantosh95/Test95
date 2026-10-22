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
    public class UpdateContactRequestHandler : IRequestHandler<UpdateContactRequest, YourDetailsResponse>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateContactRequestHandler> _logger;

        public UpdateContactRequestHandler(IMediator mediator, ILogger<UpdateContactRequestHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<YourDetailsResponse> Handle(UpdateContactRequest request, CancellationToken cancellationToken)
        {
            try
            {
                var graphQLRequest = new GraphQLRequest
                {
                    PqId = ConfigReader.Get(Constants.Environment.UpdateContactDetailsPqId),
                    Variables = JObject.Parse(Json.Serialize(new BasketUpdateContactRequest(request), false)),
                    Headers = Utility.GetHeaders(request.AuthToken)
                };

                var graphQLResponse = await _mediator.Send(graphQLRequest, cancellationToken);
                var mapperResponse = BasketMapper.YourDetailsMapper(Json.DeserializeObject<YourDetailsResponse>(graphQLResponse.Data["updateContactDetails"]));
                return mapperResponse;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"BasketId: {request.BasketId}, CorrelationId: {AsyncContext.Current.CorrelationId}, Error in BasketUpdateContactHandler: {ex.Message}");
                throw;
            }
        }
    }
}
