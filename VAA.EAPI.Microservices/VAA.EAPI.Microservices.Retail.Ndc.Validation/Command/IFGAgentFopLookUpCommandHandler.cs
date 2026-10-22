using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Microservices.Retail.Ndc.Validation.Domain.IFGAgentFopLookUp;

namespace VAA.EAPI.Microservices.Retail.Ndc.Validation.Command
{
    public class IFGAgentFopLookUpCommandHandler : IIFGAgentFopLookUp
    {
        private readonly ILogger<IFGAgentFopLookUpCommandHandler> _logger;
        private readonly IHttpConnector _httpConnector;

        public IFGAgentFopLookUpCommandHandler(ILogger<IFGAgentFopLookUpCommandHandler> logger, IHttpConnector httpConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
        }


        public async Task<Result<Response<IFGAgentFopLookUpResponse>>> Handle(IFGAgentFopLookUpRequest iFGAgentFopLookUpRequest, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                ArgumentNullException.ThrowIfNull(iFGAgentFopLookUpRequest);
                var credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{ConfigReader.Get(Constants.Environment.IFGAgentFopLookupUsername)}:{ConfigReader.Get(Constants.Environment.IFGAgentFopLookupPassword)}"));

                var headers = new Dictionary<string, string>()
                {
                    {"Authorization", $"Basic {credentials}"}
                };

                var httpResponseData = await _httpConnector.SendAsync(ConfigReader.Get(Constants.Environment.IFGAgentFopLookupUrl), headers, string.Empty, HttpMethod.Post,Json.Serialize(iFGAgentFopLookUpRequest));
                var response = await httpResponseData.Content.ReadAsStringAsync(cancellationToken);            
             
                if (httpResponseData.StatusCode == HttpStatusCode.OK)
                {
                    return new Response<IFGAgentFopLookUpResponse>
                    {
                        Status = Status.OK,
                        Value = Json.Deserialize<IFGAgentFopLookUpResponse>(response),
                        IgnoreNullValues = false,
                    };

                }
                else
                return new Response<IFGAgentFopLookUpResponse>
                {
                    Status = Status.BadRequest,
                    Value = Json.Deserialize<IFGAgentFopLookUpResponse>(response),
                    IgnoreNullValues = false,
                };
        }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "IFG Agent FOP lookup failed. {Operation}", "IFGAgentFopLookup");
                return null;
            }

        }
    }
}


