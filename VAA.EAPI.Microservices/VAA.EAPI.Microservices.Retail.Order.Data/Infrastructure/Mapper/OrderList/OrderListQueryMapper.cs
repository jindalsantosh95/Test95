using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Model.OrderList;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Mapper.OrderList
{
    public class OrderListQueryMapper
    {
        public static CosmosQueryParameters CosmosQueryMapper(NameValueCollection query)
        {
            var cosmosQuery = "SELECT c.id FROM c";
            var queries = new List<string>();
            var queryParams = new List<KeyValuePair<string, object>>();

            if (!string.IsNullOrEmpty(query["CreationDateFrom"]) && !string.IsNullOrEmpty(query["CreationDateTo"]))
            {
                queries.Add($"c.CreationDateTime.UtcDateTime >= \"{query["CreationDateFrom"]}\" AND c.CreationDateTime.UtcDateTime <= \"{query["CreationDateTo"]}\"");
                queryParams.Add(new KeyValuePair<string, object>("CreationDateFrom", query["CreationDateFrom"]));
                queryParams.Add(new KeyValuePair<string, object>("CreationDateTo", query["CreationDateTo"]));
            }

            if (!string.IsNullOrEmpty(query["LastModifiedDateFrom"]) && !string.IsNullOrEmpty(query["LastModifiedDateTo"]))
            {
                long fromModifiedDto = DateTimeOffset.Parse(query["LastModifiedDateFrom"]).ToUnixTimeSeconds();
                long toModifiedDto = DateTimeOffset.Parse(query["LastModifiedDateTo"]).ToUnixTimeSeconds();

                queries.Add($"c._ts >= {fromModifiedDto} AND c._ts <= {toModifiedDto}");
                queryParams.Add(new KeyValuePair<string, object>("LastModifiedDateFrom", fromModifiedDto));
                queryParams.Add(new KeyValuePair<string, object>("LastModifiedDateTo", toModifiedDto));
            }
            
            if (!string.IsNullOrEmpty(query["OrderCancellationDate"]))
            {
                queries.Add($"NOT IS_DEFINED(c.OrderItems[0].PaymentDetails) AND SUBSTRING(c.TimeLimits[0][\"Limit\"], 0, 10) = \"{query["OrderCancellationDate"]}\"");
                queryParams.Add(new KeyValuePair<string, object>("OrderCancellationDate", query["OrderCancellationDate"]));
            }

            if (!string.IsNullOrEmpty(query["PaymentStatus"]))
            {
                if (query["PaymentStatus"] == "paid")
                {
                    queries.Add($"IS_DEFINED(c.OrderItems[0].PaymentDetails)");
                    queryParams.Add(new KeyValuePair<string, object>("PaymentStatus", query["PaymentStatus"]));
                }
                else if (query["PaymentStatus"] == "unpaid")
                {
                    queries.Add($"NOT IS_DEFINED(c.OrderItems[0].PaymentDetails)");
                    queryParams.Add(new KeyValuePair<string, object>("PaymentStatus", query["PaymentStatus"]));
                }
            }

            if (!string.IsNullOrEmpty(query["TravelDateFrom"]))
            {
                queries.Add($"SUBSTRING(c.DataList.SegmentDetails[0].ScheduledDepartureTime.LocalDateTime, 0, 10) = \"{query["TravelDateFrom"]}\" ");
                queryParams.Add(new KeyValuePair<string, object>("TravelDateFrom", query["TravelDateFrom"]));
            }

            if (queries.Any())
            {
                cosmosQuery += " WHERE " + string.Join(" AND ", queries);
            }

            return new CosmosQueryParameters
            {
                Parameters = queryParams,
                Query = cosmosQuery
            };
        }
    }
}
