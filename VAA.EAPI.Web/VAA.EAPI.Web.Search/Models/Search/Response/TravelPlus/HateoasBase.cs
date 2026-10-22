using System;
using System.Linq;
using System.Net.Http.Headers;
using VAA.EAPI.Web.Common;
using VAA.EAPI.Web.Common.Helper;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus
{
    public abstract class HateoasBasex
    {
        public HateoasBasex()
        {
            // As we are just deserializing into the models, dependency injection does not fire so we have to trt and get
            // FuctionContextAccessor from service collection
        }

        public string GetBasketId()
        {
            string basketId = (new Guid()).ToString();
            if (AsyncContext.Current.Context.Items.TryGetValue(Constants.Application.BasketIdHeaderContainer, out var heders))
            {
                if (((HttpResponseHeaders)heders).TryGetValues(Constants.Application.BasketIdHeaderName, out var headerBasketId))
                {
                    basketId = headerBasketId.First();
                }
                else
                {
                    if (AsyncContext.Current.Context.BindingContext.BindingData.TryGetValue(Constants.Application.BasketIdHeaderName, out object headers))
                        basketId = headers.ToString();

                    if (AsyncContext.Current.Context.BindingContext.BindingData.TryGetValue("ids", out object searchRequestIds))
                        basketId = (Json.Deserialize<Search.Request.SearchRequestIds>(searchRequestIds?.ToString()))?.Id;
                }
            }
            return basketId;
        }

        public string GetPostbackUrl()
        {
            return ConfigReader.Get(Constants.Environment.PostbackUrl);
        }
    }
}
