using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Retail.Offer.Infrastructure.Services.Ita
{
    public class ItaService : IItaService
    {
        private static readonly XmlSerializer ErrorSerializer = new(typeof(error), new XmlRootAttribute("error"));
        private static readonly XmlSerializer MessagesSerializer = new(typeof(resultMessages), new XmlRootAttribute("messages"));
        private static readonly XmlSerializer ResultSerializer = new(typeof(result));
        private static readonly XmlSerializer SolutionSerializer = new(typeof(resultBundledSolutionSummarySolution), new XmlRootAttribute("solution"));
        private readonly string _itaServiceUrl;
        private readonly IHttpConnector _httpConnector;

        public ItaService(IHttpConnector httpConnector, ILogger<ItaService> logger)
        {
            _itaServiceUrl = ConfigReader.Get(Constants.Environment.ItaServiceUrl);
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _ = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<result> GetOffers(search search, System.Threading.CancellationToken cancellationToken)
        {
            if (search == null) throw new ArgumentNullException(nameof(search));

            var stringWriter = new StringWriter();
            var requestSerializer = new XmlSerializer(typeof(search));

            requestSerializer.Serialize(stringWriter, search);
            using var request = new StringContent(stringWriter.ToString(), Encoding.UTF8, Constants.Application.TextXmlContentType);
            var Headers = new Dictionary<string, string>()
            {
                {
                    Constants.Application.ApimOcpSubKey, ConfigReader.Get(Constants.Environment.ItaServiceAPIMSubKey)
                }
            };

            using var httpResponse = await _httpConnector.SendAsync(_itaServiceUrl, Headers, string.Empty, HttpMethod.Post, request, cancellationToken);
            await using var responseStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var xmlReader = XmlReader.Create(responseStream, new XmlReaderSettings
            {
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });
            return (result)ResultSerializer.Deserialize(xmlReader);
        }

        public async Task<result> StreamOfferSolutions(
            search search,
            Func<resultBundledSolutionSummarySolution, CancellationToken, Task> solutionHandler,
            CancellationToken cancellationToken)
        {
            if (search == null) throw new ArgumentNullException(nameof(search));
            if (solutionHandler == null) throw new ArgumentNullException(nameof(solutionHandler));

            using var stringWriter = new StringWriter();
            new XmlSerializer(typeof(search)).Serialize(stringWriter, search);
            using var request = new StringContent(stringWriter.ToString(), Encoding.UTF8, Constants.Application.TextXmlContentType);
            var headers = new Dictionary<string, string>
            {
                { Constants.Application.ApimOcpSubKey, ConfigReader.Get(Constants.Environment.ItaServiceAPIMSubKey) }
            };

            using var httpResponse = await _httpConnector.SendAsync(_itaServiceUrl, headers, string.Empty, HttpMethod.Post, request, cancellationToken);
            await using var responseStream = await httpResponse.Content.ReadAsStreamAsync(cancellationToken);
            using var reader = XmlReader.Create(responseStream, new XmlReaderSettings
            {
                Async = true,
                DtdProcessing = DtdProcessing.Prohibit,
                XmlResolver = null
            });

            var response = new result();
            var bundledSolutionDepth = -1;
            var processedSolutionCount = 0;

            if (!await reader.ReadAsync())
            {
                return response;
            }

            while (!reader.EOF)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var readerAdvancedByDeserializer = false;

                if (reader.NodeType == XmlNodeType.Element)
                {
                    switch (reader.LocalName)
                    {
                        case "result":
                            response = PopulateResultMetadata(reader);
                            break;
                        case "error":
                            response.error = (error)ErrorSerializer.Deserialize(reader);
                            readerAdvancedByDeserializer = true;
                            break;
                        case "messages":
                            response.messages = (resultMessages)MessagesSerializer.Deserialize(reader);
                            readerAdvancedByDeserializer = true;
                            break;
                        case "bundledSolutionSummary":
                            bundledSolutionDepth = reader.Depth;
                            break;
                        case "solution" when bundledSolutionDepth >= 0:
                            var solution = (resultBundledSolutionSummarySolution)SolutionSerializer.Deserialize(reader);
                            readerAdvancedByDeserializer = true;
                            await solutionHandler(solution, cancellationToken);
                            processedSolutionCount++;
                            break;
                    }
                }
                else if (reader.NodeType == XmlNodeType.EndElement && reader.LocalName == "bundledSolutionSummary" && reader.Depth == bundledSolutionDepth)
                {
                    bundledSolutionDepth = -1;
                }

                if (!readerAdvancedByDeserializer && !await reader.ReadAsync())
                {
                    break;
                }
            }

            response.solutionCount = processedSolutionCount;
            response.solutionCountSpecified = true;

            return response;
        }

        private static result PopulateResultMetadata(XmlReader reader)
        {
            using var metadataWriter = new StringWriter();
            using (var xmlWriter = XmlWriter.Create(metadataWriter, new XmlWriterSettings { OmitXmlDeclaration = true }))
            {
                xmlWriter.WriteStartElement(reader.Prefix, reader.LocalName, reader.NamespaceURI);

                if (reader.MoveToFirstAttribute())
                {
                    do
                    {
                        xmlWriter.WriteAttributeString(reader.Prefix, reader.LocalName, reader.NamespaceURI, reader.Value);
                    }
                    while (reader.MoveToNextAttribute());

                    reader.MoveToElement();
                }

                xmlWriter.WriteEndElement();
            }

            using var metadataReader = new StringReader(metadataWriter.ToString());
            return (result)ResultSerializer.Deserialize(metadataReader);
        }

    }
}
