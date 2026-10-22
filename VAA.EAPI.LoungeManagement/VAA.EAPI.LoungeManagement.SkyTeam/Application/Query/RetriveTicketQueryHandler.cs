using MediatR;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.LoungeManagement.SkyTeam.Domain;
using VAA.EAPI.Domain.Entities.Common;
using System.Net.Http;
using VAA.EAPI.LoungeManagement.SkyTeam.Mapper;
using Microsoft.Extensions.Logging;
using VAA.EAPI.LoungeManagement.SkyTeam.Domain.TicketData;


namespace VAA.EAPI.LoungeManagement.SkyTeam.Application.Query
{
    public class RetriveTicketQueryHandler : IRequestHandler<RetriveTicketQuery, Response<RetrieveTicketDetailsResponse>>
    {
        private readonly IOAuthTokenProvider _tokenProvider;
        private readonly IHttpConnector _httpConnector;
        private readonly ILogger<RetriveTicketQueryHandler> _logger; 

        public RetriveTicketQueryHandler(IOAuthTokenProvider tokenProvider, IHttpConnector httpConnector, ILogger<RetriveTicketQueryHandler> logger)
        {
            _tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
            _httpConnector = httpConnector ?? throw new ArgumentNullException(nameof(httpConnector));
            _logger = (logger ?? throw new ArgumentNullException(nameof(logger))); 
        }

        public async Task<Response<RetrieveTicketDetailsResponse>> Handle(RetriveTicketQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var Value = await _httpConnector.SendAsync<TicketFeedResponse>($"{ConfigReader.Get(Constants.Environment.TicketUrl)}{ConfigReader.Get(Constants.Application.RetrieveTicketEndpoint)}", GetHeaders(), null, HttpMethod.Post, Json.Serialize(new { ETicketNumber = request.RetrieveTicketDetailsRequest.ETicketNumber, IncludeAssociatedEmds = true }));
                var MapperResponse = TicketResponseMapper.GetTicketResponseMapper(Value, request);
                
                return new Response<RetrieveTicketDetailsResponse>()
                {
                    Status = Status.OK,
                    Errors = null,
                    Value = MapperResponse
                };
            }
            
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket details");
                
                return new Response<RetrieveTicketDetailsResponse>()
                {
                    Status = Status.Error,
                    Errors = new List<ErrorDetails>
                    {
                        new ErrorDetails
                        {
                            Message = Constants.Application.ErrorInfo
                        }
                    },
                };
             
            }
        }

        private static Dictionary<string, string> GetHeaders()
        {
            return new Dictionary<string, string>
            {
                { Constants.Application.OcpKeyHeader, ConfigReader.Get(Constants.Environment.TicketOcpKey) }
            };
        }
    }
}