using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Base;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.MemberBenefit
{
    public class MemberCustomAttributes : CustomerTransaction
    {
        public string ActiveYearCounterForTenureBenefits { get; set; }
        public string TotalActiveYear { get; set; }
        public IEnumerable<MemberLastFiveVSActivityYear> MemberLastFiveVSActivityYear { get; set; }

        public MemberCustomAttributes()
        {
            // Required for Serialization
        }

        public MemberCustomAttributes(MemberCustomAttributesDTO dto)
        {
            Id = dto.Id;
            CustomerId = dto.CustomerId;
            CreatedBy = dto.CreatedBy;
            CreatedOn = DateTime.Parse(dto.CreatedOn);
            LastUpdatedBy = dto.LastUpdatedBy;
            LastUpdatedOn = DateTime.Parse(dto.LastUpdatedOn);
            ActiveYearCounterForTenureBenefits = dto.ActiveYearCounterForTenureBenefits;
            TotalActiveYear = dto.TotalActiveYear;
            MemberLastFiveVSActivityYear = dto.MemberLastFiveVSActivityYear.Select(x => new MemberLastFiveVSActivityYear { ActivityYear = x.ActivityYear, BenefitGiven = x.BenefitGiven });
        }
    }
}
