using System.Net.Http.Headers;
using VAA.EAPI.GraphQL.Common;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Search.Models.BFFResponse;

namespace VAA.EAPI.GraphQL.Domain.Search.Helpers
{
    public static class BasketId
    {
        public static string GetBasketId()
        {
            string basketId = new Guid().ToString();
            if (AsyncContext.Current.Context.Items.TryGetValue(Constants.Application.BasketIdHeaderContainer, out var basketHeaders))
            {
                if (((HttpResponseHeaders)basketHeaders).TryGetValues(Constants.Application.BasketIdHeader, out var headerBasketId))
                {
                    basketId = headerBasketId.First();
                }
                else
                {
                    if (AsyncContext.Current.Context.BindingContext.BindingData.TryGetValue(Constants.Application.BasketIdHeader, out object headers))
                        basketId = headers.ToString();

                    if (AsyncContext.Current.Context.BindingContext.BindingData.TryGetValue("ids", out object searchRequestIds))
                        basketId = Json.Deserialize<SearchRequestIds>(searchRequestIds?.ToString())?.Id;
                }
            }
            return basketId;
        }
    }
}
