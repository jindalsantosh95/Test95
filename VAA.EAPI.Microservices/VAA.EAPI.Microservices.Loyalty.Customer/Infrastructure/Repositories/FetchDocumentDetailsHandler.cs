using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
{
    public class FetchDocumentDetailsHandler : IRequestHandler<FetchDocumentDetailsQuery, bool>
    {
        private readonly IMediator _mediator;
        public readonly ILogger<FetchDocumentDetailsHandler> _logger;

        public FetchDocumentDetailsHandler(IMediator mediator, ILogger<FetchDocumentDetailsHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<bool> Handle(FetchDocumentDetailsQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"FetchDocumentDetailsHandler -Start");

            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.CreateCustomerEntities.FcNumber),
                new KeyValuePair<string, object>("docType", DocumentType.CustomerSearch.ToString())
            };
            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Environment.SCVCustDBEndPoint), ConfigReader.Get(Environment.SCVCustDB), dBSearchParams, Constants.Application.SearchDbQuery));

            if (customerSearchResponse.Any())
            {
                var customerSearch = Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault());
                List<KeyValuePair<string, object>> dBcustomerParams = new()
                {
                    new KeyValuePair<string, object>("partitionKey", customerSearch.CustomerId),
                    new KeyValuePair<string, object>("docType", DocumentType.Customer.ToString())
                 };
                var customerResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Environment.SCVCustDBEndPoint), ConfigReader.Get(Environment.SCVCustDB), dBcustomerParams, Constants.Application.SearchDbQuery));

                if (customerResponse.Any())
                {
                    List<KeyValuePair<string, object>> keyValues = new()
                {
                    new KeyValuePair<string, object>("partitionKey", customerSearch.CustomerId),
                    new KeyValuePair<string, object>("docType", DocumentType.FlyingClubMember.ToString())
                };
                    var memberProfileResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Environment.SCVCustTxnDBMmbrPrfleContnr), ConfigReader.Get(Environment.SCVCustTxnDBEndPoint), ConfigReader.Get(Environment.SCVCustTxnDB), keyValues, Constants.Application.SearchDbQuery));
                    if (memberProfileResponse.Count > 0)
                    {
                        var memberProfile = Json.DeserializeObject<MemberProfile>((JToken)memberProfileResponse.FirstOrDefault());
                        if (memberProfile.TierCode == TierCode.Glass.ToString())
                            return true;
                    }
                }
            }
            _logger.LogDebug($"FetchDocumentDetailsHandler -End");
            return false;
        }
    }
}
