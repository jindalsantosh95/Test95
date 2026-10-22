using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerAssignAward
{
    public class CustomerAssignAwardRepoHandler : IRequestHandler<CustomerAssignAwardRepoCommand, bool>
    {
        private readonly ILogger<CustomerAssignAwardRepoHandler> _logger;
        private readonly ICustomerService<CustomersDTO> _customerService;
        private readonly ICustomerTransactionService<AwardDocumentDataDTO> _customerTranService;

        public CustomerAssignAwardRepoHandler(ILogger<CustomerAssignAwardRepoHandler> logger, ICustomerService<CustomersDTO> customerService, ICustomerTransactionService<AwardDocumentDataDTO> service)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _customerTranService = service ?? throw new ArgumentNullException(nameof(service));
        }

        public async Task<bool> Handle(CustomerAssignAwardRepoCommand request, CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogDebug("Fetching CustomerId from Cosmos DB - Start");
                string customerId = await FetchCustomerId(request.Data.MemberId);
                _logger.LogDebug($"Fetching CustomerId from Cosmos DB End for Id : {request.Id} customerId : {customerId}");
                if (string.IsNullOrEmpty(customerId))
                {
                    _logger.LogDebug(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.NoRecordsErrorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                    return false;
                }
                _logger.LogDebug($"Fetching Customer details from Transaction Cosmos DB - start");
                var customerDetails = await _customerTranService.GetItemAsync(DocumentType.Award, Constants.Application.AwardSqlQuery, customerId);
                _logger.LogDebug($"Fetching Customer details from Transaction Cosmos DB - End");
                if (customerDetails != null && customerDetails.Count > 0)
                {
                    _logger.LogDebug($"Updating document in Cosmos DB for Top Member - Start");
                    await _customerTranService.PatchItemAsync(customerDetails.FirstOrDefault().Id, customerDetails.FirstOrDefault().CustomerId);
                    _logger.LogDebug($"Updating document in Cosmos DB for Top Member- End");
                    return true;
                }
                _logger.LogDebug($"Creating document in Cosmos DB - Start");
                AwardDocumentData awardDocData = AwardDocumentMapper.Mapper(customerId, request);
                await _customerTranService.CreateItemAsync(new AwardDocumentDataDTO(awardDocData), customerId, DocumentType.Award);
                _logger.LogDebug($"Creating document in Cosmos DB - End");

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Exception occured - {ex}");
            }

            return false;
        }

        private async Task<string> FetchCustomerId(string memberId)
        {
            var result = await _customerService.GetItemAsync(DocumentType.Customer, Constants.Application.SqlQuery, memberId);
            return result?.FirstOrDefault()?.CustomerId;
        }
    }
}
