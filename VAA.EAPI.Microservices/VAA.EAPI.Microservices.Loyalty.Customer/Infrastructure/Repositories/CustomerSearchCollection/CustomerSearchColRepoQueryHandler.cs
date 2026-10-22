using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities.CustomerSearchCollection;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchCollection
{
    public class CustomerSearchColRepoQueryHandler : IRequestHandler<CustomerSearchColRepoQuery, IEnumerable<CustomerSearch>>
    {
        private readonly IMediator _mediator;

        public CustomerSearchColRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<CustomerSearch>> Handle(CustomerSearchColRepoQuery request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType),
                new KeyValuePair<string, object>("docType", Constants.Application.CustomerSearchDocType),
                new KeyValuePair<string, object>("phoneNumber1", request.PhoneNumber),
                new KeyValuePair<string, object>("phoneNumber2", "0" + request.PhoneNumber)
            };


            return Json.Deserialize<List<CustomerSearch>>(Json.Serialize(await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.CustomerSearchCollection),
               ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint),
               ConfigReader.Get(Constants.Environment.SCVCustDB),
               dBSearchParams,
               Constants.Application.CustomerSearchByPhoneQuery))));
        }
    }
}
