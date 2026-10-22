using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using MemberAttribute = VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember.MemberAttribute;
using FlyingClubMember = VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember.FlyingClubMember;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.FlyingClubMember;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Mapper
{
    public class FlyingClubMemberUpdateEventDataMapper
    {
        public static FlyingClubMemberPatchEvent MapVSFlownMilesEventData(FlyingClubMember existingFCMDoc, string distance, string customerId)
        {
            MemberAttribute memberAttribute = new();

            var flownMilesAttribute = existingFCMDoc.MemberAttributes.FirstOrDefault(x => x.AttributeName.Is(Constants.Application.FlownMiles));

            if (flownMilesAttribute != null)
            {
                memberAttribute = new MemberAttribute
                {
                    AttributeName = Constants.Application.VSFlownMiles,
                    Value = (int.Parse(flownMilesAttribute.Value) + int.Parse(distance)).ToString()
                };
            }
            else
            {
                memberAttribute = new MemberAttribute
                {
                    AttributeName = Constants.Application.VSFlownMiles,
                    Value = distance
                };
            }

            var updatedFCMAttributes = new FlyingClubMemberPatchEvent
            {
                FlyingClubMemberDocument = new FlyingClubMember
                {
                    LastUpdatedBy = Constants.Application.Channel,
                    CustomerId = existingFCMDoc.MemberNumber,
                    MemberAttributes = new List<MemberAttribute> { memberAttribute }
                },
                CustomerId = customerId
            };

            return updatedFCMAttributes;
        }
    }
}
