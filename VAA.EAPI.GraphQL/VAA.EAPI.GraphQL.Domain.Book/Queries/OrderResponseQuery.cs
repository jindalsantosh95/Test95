using HotChocolate;
using HotChocolate.Types;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Book.Models.Response.Order;

namespace VAA.EAPI.GraphQL.Domain.Book.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class OrderResponseQuery
    {
        protected readonly ILogger<OrderResponseQuery> _logger;
        private readonly IDistributedCache _distributedCache;

        public OrderResponseQuery(ILogger<OrderResponseQuery> logger, IDistributedCache distributedCache)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _distributedCache = distributedCache ?? throw new ArgumentNullException(nameof(distributedCache));
        }

        [GraphQLName("orderResponse")]
        [GraphQLDescription("Get Final Order For Order Confirmation From Cache")]
        public async Task<OrderCreateGraphQLResponse> GetOrderResponseAsync(string basketId)
        {
            try
            {
                if (string.IsNullOrEmpty(basketId)) throw new ArgumentNullException(nameof(basketId));

                if (ConfigReader.Caching.IsCacheEnabled("orderResponse", out string key, out TimeSpan expiry, out bool isSliding, basketId))
                {
                    var cachedValue = await _distributedCache?.GetStringAsync(key);

                    if (!cachedValue.IsNullOrWhiteSpace())
                    {
                        var orderCreateResponse = Encoding.UTF8.GetString(Convert.FromBase64String(cachedValue));

                        var response = Json.Deserialize<OrderCreateGraphQLResponse>(orderCreateResponse);

                        return response;
                    }
                }

                return null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Correlation Id - {AsyncContext.Current.CorrelationId} :: BasketId - {basketId} - Exception - {ex}");
                throw new GraphQLException(ErrorBuilder.New()
                             .SetMessage("Unhandled Exception in OrderResponseQuery.")
                             .SetExtension("Exception", ex.Message)
                             .SetCode(((int)HttpStatusCode.InternalServerError).ToString())
                             .Build());
            }
        }
    }
}
