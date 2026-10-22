using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class MemberCustomAttributesDTO : CosmosDocument
    {
        public string SchemaVersion { get; set; }
        public string DocType { get; set; }
        public string CustomerId { get; set; }
        public string CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string ActiveYearCounterForTenureBenefits { get; set; }
        public string TotalActiveYear { get; set; }
        public IEnumerable<MemberLastFiveVSActivityYearDTO> MemberLastFiveVSActivityYear { get; set; }

        public static explicit operator MemberCustomAttributesDTO(MemberCustomAttributes memberCustomAttributes)
        {
            return new MemberCustomAttributesDTO()
            {
                SchemaVersion = Constants.Application.AwardDocSchemaVersion,
                DocType = ConfigReader.Get(EnvironmentSettings.MemberCustomAttributesDocType),
                PartitionKey = memberCustomAttributes.CustomerId,
                Id = memberCustomAttributes.Id,
                CreatedBy = memberCustomAttributes.CreatedBy,
                CreatedOn = string.IsNullOrEmpty(memberCustomAttributes.Id) ? DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat) : memberCustomAttributes?.CreatedOn.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                LastUpdatedBy = string.IsNullOrEmpty(memberCustomAttributes.Id) ? memberCustomAttributes.CreatedBy : memberCustomAttributes.LastUpdatedBy,
                LastUpdatedOn = DateTime.UtcNow.ToUniversalTime().ToString(Constants.Application.ISODateTimeFormat),
                CustomerId = memberCustomAttributes.CustomerId,
                ActiveYearCounterForTenureBenefits = memberCustomAttributes.ActiveYearCounterForTenureBenefits ?? string.Empty,
                TotalActiveYear = memberCustomAttributes.TotalActiveYear ?? string.Empty,
                MemberLastFiveVSActivityYear = memberCustomAttributes.MemberLastFiveVSActivityYear.Select(x => (MemberLastFiveVSActivityYearDTO)x)
            };           
        }
    }
}
