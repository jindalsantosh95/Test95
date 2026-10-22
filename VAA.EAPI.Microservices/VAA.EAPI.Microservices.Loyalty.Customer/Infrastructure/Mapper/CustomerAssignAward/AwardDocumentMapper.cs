using System;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerAssignAward;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.CustomerAssignAward
{
    public class AwardDocumentMapper
    {
        public static AwardDocumentData Mapper(string customerId, CustomerAssignAwardRepoCommand request)
        {
            return new AwardDocumentData()
            {
                AwardBasis = string.Empty,
                AwardValue = 0,
                AwardNumber = string.Empty,
                AwardId = Guid.NewGuid().ToString(),
                Brand = Constants.Application.Brand,
                CreatedBy = string.Empty,
                CreatedOn = DateTime.UtcNow,
                CustomerId = customerId,
                DocType = DocumentType.Award.ToString(),
                ExpiryDate = string.Empty,
                ExpiryDateUpdatedBy = string.Empty,
                GiftedAwardDate = string.Empty,
                GiftedFromCustomerId = string.Empty,
                GiftedToCustomerId = string.Empty,
                id = Guid.NewGuid().ToString(),
                LastUpdatedBy = string.Empty,
                LastUpdatedOn = DateTime.UtcNow,
                PartitionKey = customerId,
                PreviousStatus = string.Empty,
                ProductId = request.Data.ProductName == Constants.Application.TopMember ? ConfigReader.Get(Constants.Environment.ProductIdTopMember) : string.Empty,
                Quantity = 0,
                SchemaVersion = Constants.Application.SchemaVersion,
                SourceIdentifier = string.Empty,
                SourceSystem = string.Empty,
                StartDate = DateTime.UtcNow,
                Status = string.Empty,
                TransactionId = string.Empty,
                UsedDate = string.Empty
            };

        }
    }
}
