using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Banner
{
    public class BannerResponse : ICustomerTransactionQueryResponse, ICustomerTransactionCommandResponse
    {
        public string CustomerId { get; set; }
        public string MemberNumber { get; set; }
        public string DocType { get; set; }
        public DateTime CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime LastUpdatedOn { get; set; }
        public IEnumerable<Banners> Banners { get; set; }
        public string Id { get; set; }
        public string PartitionKey { get; set; }

        public BannerResponse(string customerId, IEnumerable<object> documents)
        {
            var firstBannerDto = Json.Deserialize<IEnumerable<BannerDTO>>(Json.Serialize(documents)).FirstOrDefault();

            if (firstBannerDto != null)
            {
                CustomerId = firstBannerDto.CustomerId;
                MemberNumber = firstBannerDto.MemberNumber;
                DocType = firstBannerDto.DocType;
                CreatedOn = firstBannerDto.CreatedOn;
                CreatedBy = firstBannerDto.CreatedBy;
                LastUpdatedBy = firstBannerDto.LastUpdatedBy;
                LastUpdatedOn = firstBannerDto.LastUpdatedOn;
                Banners = firstBannerDto.Banners;
                Id = firstBannerDto.Id;
                PartitionKey = firstBannerDto.PartitionKey; 
            }
        }
    }
}
