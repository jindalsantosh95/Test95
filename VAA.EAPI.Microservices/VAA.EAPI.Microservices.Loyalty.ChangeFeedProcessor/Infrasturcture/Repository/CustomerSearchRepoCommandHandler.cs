using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Interfaces;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository
{
    public class CustomerSearchRepoCommandHandler : IRequestHandler<CustomerSearchRepoCommand, bool>
    {
        public ICustomerService<CustomerSearchDTO> _customerService;
        public readonly ILogger<CustomerSearchRepoCommandHandler> _logger;

        public CustomerSearchRepoCommandHandler(ICustomerService<CustomerSearchDTO> customerService, ILogger<CustomerSearchRepoCommandHandler> logger)
        {
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<bool> Handle(CustomerSearchRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                CustomerSearchDTO customerSearch = await _customerService.GetItemAsync(request.CustomerSearchDocument.LoyaltyMemberNumber, DocumentType.CustomerSearch.ToString());

                if (customerSearch != null)
                {
                    request.CustomerSearchDocument.Id = customerSearch.Id;
                    request.CustomerSearchDocument.EmailAddress = request.CustomerSearchDocument.EmailAddress.ToLower();
                    request.CustomerSearchDocument.PhoneNumber = customerSearch.PhoneNumber;
                    request.CustomerSearchDocument.PhoneCountryCode = customerSearch.PhoneCountryCode;
                    
                    await _customerService.PatchItemAsync(new CustomerSearchDTO(request.CustomerSearchDocument), DocumentType.CustomerSearch);
                }

                else
                {
                    await _customerService.InsertItemAsync(new CustomerSearchDTO(request.CustomerSearchDocument), DocumentType.CustomerSearch);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"UpdateCustomerSearch failed for MemberNumber :{request?.CustomerSearchDocument?.LoyaltyMemberNumber} with message :{ex.Message}");
                throw;
            }

            return true;
        }
    }
}