using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service
{
    public class AddressLookUpServiceHandler : IRequestHandler<AddressLookUpServiceCommand, CompleteAddressLookupResponse>
    {
        private readonly ILogger<AddressLookUpServiceHandler> _logger;

        public AddressLookUpServiceHandler(ILogger<AddressLookUpServiceHandler> logger)
        {
            _logger = logger;
        }

        public async Task<CompleteAddressLookupResponse> Handle(AddressLookUpServiceCommand request, CancellationToken cancellationToken)
        {
            try
            {
                WesalSoapClient wesalSoapClient = new(WesalSoapClient.EndpointConfiguration.WesalSoap);

                return await wesalSoapClient.CompleteAddressLookupAsync(new ServiceCallingContext(), request.CompleteAddressLookUpRequest);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ValidateAddress-Error--WesalRepository.wesalResponse\n", ex.StackTrace);
                throw;
            }
        }
    }
}
