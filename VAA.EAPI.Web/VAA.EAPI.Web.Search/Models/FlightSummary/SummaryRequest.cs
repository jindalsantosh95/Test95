using Microsoft.IdentityModel.Tokens;
using System.IO;
using VAA.EAPI.Web.Common.Extensions;
using VAA.EAPI.Web.Common.Serialisation;

namespace VAA.EAPI.Web.Search.Models.FlightSummary
{
    public class SummaryRequest
    {
        public string SummaryLink { get; set; }

        public static explicit operator SummaryRequest(Stream source)
        {
            var bodyContent = new StreamReader(source).ReadToEndAsync().GetAwaiter().GetResult();

            return bodyContent.IsNullOrEmpty() ? new() : Json.Deserialize<SummaryRequest>(bodyContent);
        }
    }
}
