using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content;
using VAA.EAPI.Microservices.CRM.Content.Domain.Content.Enums;
using VAA.EAPI.Microservices.CRM.Content.Interfaces;

namespace VAA.EAPI.Microservices.CRM.Content.Application
{
    public sealed class ContentRequestHandler : IContentRequestHandler
    {
        private readonly IHttpConnector _httpConnector;

        public ContentRequestHandler(IHttpConnector httpConnector)
        {
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }

        public async Task<JToken> Handle(string contentId, CancellationToken cancellationToken)
        {
            var response = default(JToken);

            var aemResponse = await _httpConnector.GetAsync(uri: ConfigReader.Get(Constants.Environment.AemGraphEndpointUri) + contentId, headers: new Dictionary<string, string>(), null);

            aemResponse.EnsureSuccessStatusCode();

            var graphContent = JObject.Parse(await aemResponse.Content.ReadAsStringAsync());

            var graphData = graphContent["data"];

            if (graphData != null)
            {
                // Extract out the CONTENT, which is 2nd level down (1st level being GraphQL Operation name)
                response = graphData.FirstOrDefault()?.FirstOrDefault();
            }
            return response;
        }

        public async Task<JToken> Handle(ContentRequest contentRequest, CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(contentRequest);

            var response = await _httpConnector.GetAsync(uri: GetUri(contentRequest), headers: new Dictionary<string, string>(), null);

            response.EnsureSuccessStatusCode();

            var graphContent = JObject.Parse(await response.Content.ReadAsStringAsync());

            return GetResponse(contentRequest.SourceType, graphContent);
        }
       
        private string GetUri(ContentRequest contentRequest)
        {
             return contentRequest.SourceType switch
            {
                SourceType.AEM => GetAemUri(contentRequest),
                SourceType.VHOLS => GetVholsUri(contentRequest),
                _ => throw new InvalidOperationException($"Unsupported source type: {contentRequest.SourceType}")
            };
        }

        private static string GetAemUri(ContentRequest contentRequest)
        {
             return $"{ConfigReader.Get(Constants.Environment.AemGraphEndpointUri)}" + $"{contentRequest.Detail.ContentFragmentName}";
        }

        private static string GetVholsUri(ContentRequest contentRequest)
        {
            var builder = new UriBuilder(ConfigReader.Get(Constants.Environment.VholsEndPointUri));

            builder.Path = $"{builder.Path.TrimEnd('/')}/{contentRequest.Detail.ContentType.Value.ToString().ToLowerInvariant()}";
               
            builder.Query = $"tbxId={Uri.EscapeDataString(contentRequest.Detail.ContentId)}" +
                            $"&brand={(int)contentRequest.Detail.BrandCode.Value}" +
                            $"&size=1";

            return builder.Uri.ToString();
        }

        private JToken GetResponse(SourceType? source, JToken graphContent)
        {
            return source switch
            {
                SourceType.VHOLS => graphContent,

                SourceType.AEM => graphContent["data"] is null ? default : graphContent["data"]!.FirstOrDefault()?.FirstOrDefault(),

                _ => throw new InvalidOperationException($"Unsupported source type: {source}")
            };
        }
    }
}
