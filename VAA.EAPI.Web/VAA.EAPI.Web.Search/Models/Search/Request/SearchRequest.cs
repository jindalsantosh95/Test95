using MediatR;
using System.Collections.Generic;
using System.IO;
using System.Text.Json.Serialization;
using VAA.EAPI.Web.Common.Serialisation;
using VAA.EAPI.Web.Search.Models.Search.Response.TravelPlus;

namespace VAA.EAPI.Web.Search.Models.Search.Request
{
    public class SearchRequestV2 : IRequest<SearchResultsResponse>
    {
        [JsonPropertyName("slices")]
        public IEnumerable<SearchSlice> slices { get; set; }

        [JsonPropertyName("passengers")]
        public IEnumerable<string> passengers { get; set; }

        [JsonPropertyName("promoCode")]
        public string promoCode { get; set; }

        [JsonPropertyName("awardSearch")]
        public bool awardSearch { get; set; }

        [JsonPropertyName("brandId")]
        public string brandId { get; set; }

        [JsonPropertyName("authToken")]
        public string authToken { get; set; }

        public SearchRequestIds Ids { get; set; }

        public static explicit operator SearchRequestV2(Stream source)
        {
            var bodyContent = new StreamReader(source).ReadToEndAsync().GetAwaiter().GetResult();

            return Json.Deserialize<SearchRequestV2>(bodyContent);
        }
    }


    public class SearchRequest : IRequest<SearchResults>
    {
        [JsonPropertyName("slices")]
        public IEnumerable<SearchSlice> slices { get; set; }

        [JsonPropertyName("passengers")]
        public IEnumerable<string> passengers { get; set; }

        [JsonPropertyName("cabinClasses")]
        public IEnumerable<string> cabinClasses { get; set; }

        public SearchRequestIds Ids { get; set; }

        public static explicit operator SearchRequest(Stream source)
        {
            var bodyContent = new StreamReader(source).ReadToEndAsync().GetAwaiter().GetResult();

            return Json.Deserialize<SearchRequest>(bodyContent);
        }
    }

    public class SearchRequestIds
    {
        public string Id { get; set; }
        public string FareId { get; set; }
    }
}
