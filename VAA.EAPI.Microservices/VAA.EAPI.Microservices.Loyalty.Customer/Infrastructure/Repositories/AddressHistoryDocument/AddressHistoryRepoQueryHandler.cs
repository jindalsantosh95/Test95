using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument
{
    public class AddressHistoryRepoQueryHandler : IRequestHandler<AddressHistoryRepoQuery, IEnumerable<AddressHistory>>
    {
        private readonly IMediator _mediator;

        public AddressHistoryRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<AddressHistory>> Handle(AddressHistoryRepoQuery request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.CustomerId),
                new KeyValuePair<string, object>("docType", DocumentType.AddressHistory.ToString())
            };

            var addressHistoryResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), dBSearchParams, Constants.Application.SearchDbQuery));
            return addressHistoryResponse?.ConvertAll(x => Json.DeserializeObject<AddressHistory>((JToken)x));
        }
    }
}
