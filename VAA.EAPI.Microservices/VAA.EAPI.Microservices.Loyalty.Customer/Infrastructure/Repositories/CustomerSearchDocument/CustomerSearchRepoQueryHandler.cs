using MediatR;
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

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument
{
    public class CustomerSearchRepoQueryHandler : IRequestHandler<CustomerSearchRepoQuery, CustomerSearch>
    {
        private readonly IMediator _mediator;

        public CustomerSearchRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<CustomerSearch> Handle(CustomerSearchRepoQuery request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.MemberNumber),
                new KeyValuePair<string, object>("docType", DocumentType.CustomerSearch.ToString())
            };

            var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), dBSearchParams, Constants.Application.SearchDbQuery));
            return Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault());
        }
    }
}
