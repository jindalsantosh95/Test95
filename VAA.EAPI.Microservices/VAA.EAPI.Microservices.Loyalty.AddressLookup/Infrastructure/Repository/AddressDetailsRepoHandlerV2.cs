using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity.VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Mapper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository
{
    public class AddressDetailsRepoHandlerV2 : IRequestHandler<AddressDetailsRepoCommandV2, List<CompleteAddressDetails>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddressDetailsRepoHandlerV2> _logger;

        public AddressDetailsRepoHandlerV2(IMediator mediator, ILogger<AddressDetailsRepoHandlerV2> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<List<CompleteAddressDetails>> Handle(AddressDetailsRepoCommandV2 request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            CompleteAddressLookupResponse wesalResponse = await _mediator.Send(new AddressLookUpServiceCommandV2(AddressLookupRequestMapper.AddressLookupServiceRequestMapper(request.CountryCode, request.PostCode, request.AddressLine1)), cancellationToken);

            if (wesalResponse != null && wesalResponse?.CompleteAddressLookupResult != null)
            {
                var completeAddressLookupRs = Common.Serialisation.Xml.Deserialize<CompleteAddressLookup_RS>(wesalResponse.CompleteAddressLookupResult);
                _logger.LogInformation($"complete AddressLookup Response{completeAddressLookupRs}");
                return completeAddressLookupRs.Message?.CompleteAddressDetails;
            }

            return null;
        }
    }
}
