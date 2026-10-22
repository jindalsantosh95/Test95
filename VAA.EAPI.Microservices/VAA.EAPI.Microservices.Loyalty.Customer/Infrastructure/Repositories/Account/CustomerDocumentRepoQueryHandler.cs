using CSharpFunctionalExtensions;
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
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Account
{
    public class CustomerDocumentRepoQueryHandler : IRequestHandler<CustomerDocumentRepoQuery, Result<Response<Customers>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerDocumentRepoQueryHandler> _logger;

        public CustomerDocumentRepoQueryHandler(IMediator mediator, ILogger<CustomerDocumentRepoQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<Customers>>> Handle(CustomerDocumentRepoQuery request, CancellationToken cancellationToken)
        {
            try
            {
                List<KeyValuePair<string, object>> parameters = new()
                {
                new KeyValuePair<string, object>(Constants.Application.PartitionKey, request.CustomerID),
                new KeyValuePair<string, object>(Constants.Application.DocType, DocumentType.Customer.ToString()),
                };

                var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.CustomerDocQuery));

                if (customerSearchResponse.Any())
                    return Result.Success(new Response<Customers>
                    {
                        Value = customerSearchResponse.ConvertAll(x => Json.DeserializeObject<Customers>((JToken)x)).FirstOrDefault(),
                        Status = Status.OK
                    });

                return Result.Success(new Response<Customers>
                {
                    Status = Status.BusinessException
                });
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error while retrieving information from Customer cosmos DB\n", ex.StackTrace);
                throw;
            }
        }
    }
}
