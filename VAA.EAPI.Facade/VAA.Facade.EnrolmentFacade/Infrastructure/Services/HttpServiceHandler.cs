using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Helper.Serialisation;
using VAA.Facade.EnrolmentFacade.Infrastructure.Interfaces;

namespace VAA.Facade.EnrolmentFacade.Infrastructure.Services
{
    public class HttpServiceHandler : IHttpServiceHandler
    {

        private readonly IHttpClientFactory _httpClientFactory;


        public HttpServiceHandler(IHttpClientFactory httpClientFactory)
        {
            _httpClientFactory = httpClientFactory ?? throw new ArgumentNullException(nameof(httpClientFactory));
        }

        public async Task<HttpResponseMessage> Handle(HttpServiceCommand request, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);

            HttpClient httpClient = _httpClientFactory.CreateClient();
            using var httpRequest = new HttpRequestMessage(request.RequestType, request.url);
            BindResponseHeaderPara(httpRequest, request.headersList);

            if (request.RequestType == HttpMethod.Post)
            {
                httpRequest.Content = new StringContent(Json.Serialize(request.EventGridTopicEvent).ToString(),
                                       Encoding.UTF8,
                                       Constants.Constants._contenTypeapplicationjson);
            }

            return await httpClient.SendAsync(httpRequest, cancellationToken);
        }

        private static void BindResponseHeaderPara(HttpRequestMessage httpRequestMessage, List<KeyValuePair<string, string>> responseHeaders)
        {

            if (responseHeaders != null)
            {
                foreach (var headerPar in responseHeaders)
                {
                    httpRequestMessage.Headers.Add(headerPar.Key, headerPar.Value);
                }
            }
        }
    }
}
