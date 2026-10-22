using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.ContactHistoryDocument
{
    public class ContactHistoryRepoQueryHandler : IRequestHandler<ContactHistoryRepoQuery, IEnumerable<ContactHistory>>
    {
        private readonly IMediator _mediator;

        public ContactHistoryRepoQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<IEnumerable<ContactHistory>> Handle(ContactHistoryRepoQuery request, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> dBSearchParams = new()
            {
                new KeyValuePair<string, object>("partitionKey", request.CustomerId),
                new KeyValuePair<string, object>("docType", DocumentType.ContactHistory.ToString())
            };
            return Json.Deserialize<List<ContactHistory>>(Json.Serialize(await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr),
                ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint),
                ConfigReader.Get(Constants.Environment.SCVCustDB),
                dBSearchParams,
                Constants.Application.SearchDbQuery))));
        }
    }
}
