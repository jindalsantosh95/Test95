using Microsoft.Azure.Cosmos;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Infrastructure.Services.Analytics;

namespace VAA.EAPI.Microservices.Retail.Analytics.Application.Command
{
    public class TrackAnalyticsDocument : CosmosDocument
    {
        public static explicit operator TrackAnalyticsDocument(TrackAnalyticsMessage source)
        {
            PartitionKey partitionKey = new PartitionKeyBuilder().Add(source.Date)
                                                                 .Add(source.GetValue("DocType"))
                                                                 .Build();
            return new()
            {
                Id = source.Id,
                Date = source.Date,
                DocType = source.GetValue("DocType"),
                Data = source.Data,
                PartitionKey = partitionKey.ToString(),
            };
        }

        public string Date { get; set; }
        public string DocType { get; set; }
        public JObject Data { get; set; }
    }
}
