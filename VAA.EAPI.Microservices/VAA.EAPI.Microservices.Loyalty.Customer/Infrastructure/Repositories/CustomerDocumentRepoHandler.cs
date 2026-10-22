using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
{
    public class CustomerDocumentRepoHandler : IRequestHandler<CustomerDocumentRepoQuery, Result<Response<Customers>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerDocumentRepoHandler> _logger;

        public CustomerDocumentRepoHandler(IMediator mediator, ILogger<CustomerDocumentRepoHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<Response<Customers>>> Handle(CustomerDocumentRepoQuery request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.CustomerId),
                new KeyValuePair<string, object>("docType", DocumentType.Customer.ToString())
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), dBSearchParams, Constants.Application.SearchDbQuery));

            if (customerSearchResponse.Any())
            {
                var custResponse = customerSearchResponse.ConvertAll(x => Json.DeserializeObject<Customers>((JToken)x)).FirstOrDefault();
                return Result.Success(new Response<Customers>
                {
                    Value = custResponse,
                    Status = Status.OK
                });
            }

            return null;
        }
    }
}
