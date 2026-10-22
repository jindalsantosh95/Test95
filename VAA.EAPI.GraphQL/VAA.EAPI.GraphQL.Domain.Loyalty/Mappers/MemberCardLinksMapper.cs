using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.Reference;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mappers
{
    public class MemberCardLinksMapper
    {
        public static List<MemberCardLinkResponse> MapToMemberCardDetails(VredCardDetails cardDetails, IEnumerable<PartnerProfileDetails> partnerProfileDetails, IEnumerable<PartnerMembership> partnerMembershipList)
        {
            List<MemberCardLinkResponse> memberCardLinkList = new();

            if (cardDetails != null && cardDetails.CardLinks != null && cardDetails.Errors == null)
            {
                memberCardLinkList.AddRange(cardDetails.CardLinks.Select(cardlink => new MemberCardLinkResponse
                {
                    PartnerCode = cardlink.CardLinkPartnerUserIdDomain,
                    PartnerName = partnerProfileDetails.Where(t => t.PartnerCode == cardlink.CardLinkPartnerUserIdDomain.ToUpper())?.FirstOrDefault().Name,
                    CardType = cardlink.CardType,
                    LinkedDate = cardlink.LinkedDateTime,
                    EndDate = cardlink.CancelledDateTime,
                    AnniversaryDate = cardlink.AnniversaryDateTime,
                    PAN = cardlink.CardLinkPartnerUserId
                }));
            }

            if (partnerMembershipList != null && partnerMembershipList.Any())
            {
                memberCardLinkList.AddRange(partnerMembershipList.Select(partnerMembership => new MemberCardLinkResponse
                {
                    PartnerCode = partnerProfileDetails.Where(t => t.RowId == partnerMembership.PartnerId.ToUpper())?.FirstOrDefault().PartnerCode,
                    PartnerName = partnerProfileDetails.Where(t => t.RowId == partnerMembership.PartnerId.ToUpper())?.FirstOrDefault().Name,
                    CardType = partnerMembership.PartnerStatusCode,
                    LinkedDate = partnerMembership.StartDate,
                    EndDate = partnerMembership.EndDate,
                    PAN = partnerMembership.PartnerId,
                    DocumentId = partnerMembership.Id
                }));
            }

            return memberCardLinkList;
        }
    }
}

