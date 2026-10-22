using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Account
{
    public class CustomerSearchDocRepoQueryHandler : IRequestHandler<CustomerSearchDocRepoQuery, CustomerSearch>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerSearchDocRepoQueryHandler> _logger;

        public CustomerSearchDocRepoQueryHandler(IMediator mediator, ILogger<CustomerSearchDocRepoQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<CustomerSearch> Handle(CustomerSearchDocRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                List<KeyValuePair<string, object>> dBSearchParams = new()
                {
                    new KeyValuePair<string, object>(Constants.Application.PartitionKey, DocumentType.CustomerSearch.ToString()),
                    new KeyValuePair<string, object>(Constants.Application.LoyaltyMemberNumber, request.MemberNumber)
                };

                var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(Constants.Application.CustomerSearchDocType, ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), dBSearchParams, Constants.Application.CustomerSearchDocQuery));

                return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault());
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while retrieving information from Customer cosmos DB\n", ex.StackTrace);
                throw;
            }
        }


    }
}
