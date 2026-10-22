using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Interface;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderDataMS;
using Environment = VAA.EAPI.NDC.Order.Constants.Environment;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS
{
    public class OrderListQueryHandler : IOrderListQuery
    {
        private readonly IHttpConnector _httpConnector;

        public OrderListQueryHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<IEnumerable<OrderListResponse>> Handle(IATA_OrderListRQ request, CancellationToken cancellationToken)
        {
            var headers = new Dictionary<string, string>()
            {
                { "Ocp-Apim-Subscription-Key", ConfigReader.Get(Environment.OrderDataMSSubscriptionKey)},
            };
            var orderSearchFilter = request.Request.OrderSearchFilterCriteria;
            var queryParams = new Dictionary<string, string>()
            {
                { "CreationDateFrom", orderSearchFilter.CreationDateTimeFilterCriteria?.FromDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "CreationDateTo", orderSearchFilter.CreationDateTimeFilterCriteria?.ToDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "LastModifiedDateFrom", orderSearchFilter.LastModifiedDateTimeFilterCriteria?.FromDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "LastModifiedDateTo", orderSearchFilter.LastModifiedDateTimeFilterCriteria?.ToDateTime.ToString("yyyy-MM-ddTHH:mm:ssZ") },
                { "OrderCancellationDate", orderSearchFilter.OrderCancellationDateFilterCriteria?.ExpirationDate.ToString("yyyy-MM-dd") },
                { "PaymentStatus", orderSearchFilter.PaymentFilterCriteria?.FirstOrDefault()?.PaymentStatusCode.ToLower()},
                { "TravelDateFrom", orderSearchFilter.TravelDateFilterCriteria?.EffectiveDate.ToString("yyyy-MM-dd") },
            };

            var uriBuilder = new UriBuilder(ConfigReader.Get(Constants.Environment.OrderDataMSOrderListEndpoint))
            {
                Query = string.Join("&", queryParams.Where(kvp => !string.IsNullOrEmpty(kvp.Value)).Select(kvp => $"{kvp.Key}={kvp.Value}"))
            };

            var url = uriBuilder.Uri.AbsoluteUri;

            var httpResponseMessage = await _httpConnector.SendAsync(url, headers, string.Empty, HttpMethod.Get, string.Empty);
            var response = await httpResponseMessage.Content.ReadAsStringAsync(cancellationToken);

            if(httpResponseMessage.StatusCode == HttpStatusCode.OK)
                return Json.Deserialize<IEnumerable<OrderListResponse>>(response);

            else
                return [];
        }
    }
}
