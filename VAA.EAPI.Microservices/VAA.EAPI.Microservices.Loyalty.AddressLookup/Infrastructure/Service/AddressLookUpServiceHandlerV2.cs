using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Proxy;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Service
{
    public class AddressLookUpServiceHandlerV2 : IRequestHandler<AddressLookUpServiceCommandV2, CompleteAddressLookupResponse>
    {
        private readonly ILogger<AddressLookUpServiceHandlerV2> _logger;
        private readonly IAzureWesalApiConnector _azureApiConnector;

        public AddressLookUpServiceHandlerV2(ILogger<AddressLookUpServiceHandlerV2> logger, IAzureWesalApiConnector azureApiConnector)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureApiConnector = azureApiConnector ?? throw new ArgumentNullException(nameof(azureApiConnector));

        }
        public async Task<CompleteAddressLookupResponse> Handle(AddressLookUpServiceCommandV2 addressDetailRequest, CancellationToken cancellationToken)
        {

            string WesalRequest = AddressLookUpRequestSoapUI.AddressLookupServiceRequestMapperSoapUI(addressDetailRequest.CompleteAddressLookUpRequest);

            try
            {
                var wesalResponse = await _azureApiConnector.GetAddressDetails(WesalRequest);

                CompleteAddressLookupResponse response = new()
                {
                    CompleteAddressLookupResult = wesalResponse
                };
                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"ValidateAddress-Error--WesalRepository.wesalResponse\n", ex.StackTrace);
                throw;
            }
        }
    }
}
