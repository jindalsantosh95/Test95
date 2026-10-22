using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Mapper.EnrolmentCustomer
{
    public class TierDetailMapper
    {
        public static TierInfo MapFromSCVResponse(CreateCustomerEntities createCustomerEntities, MembershipChange memberDetails)
        {
            return new TierInfo()
            {
                CurrentTier = memberDetails?.TierCode,
                PreviousTier = null,
                JoinDatetime = DateTimeHelper.ConvertToUtc(memberDetails?.ApprovalDate, Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat),
                TierChangeDatetime = DateTimeHelper.ConvertToUtc(memberDetails?.ApprovalDate, Constants.Application.VRedDateFormat, Constants.Application.SCVDateFormat),
                AddressDetails = new Address[]
                {
                    new Address
                    {
                        Country =  createCustomerEntities?.AddressDetails?.Any(x => x.PreferredFlag == "Y") == true ? createCustomerEntities?.AddressDetails?.First(x => x.PreferredFlag.Is("Y"))?.CountryCode : createCustomerEntities?.AddressDetails?.FirstOrDefault()?.CountryCode

                    }
                },
                Gender = string.IsNullOrEmpty(createCustomerEntities?.Gender) ? null : createCustomerEntities?.Gender
            };
        }
    }
}
