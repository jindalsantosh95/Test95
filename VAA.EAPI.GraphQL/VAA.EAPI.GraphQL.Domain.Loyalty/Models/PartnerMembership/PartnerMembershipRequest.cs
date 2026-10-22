using System;
using System.Net.Http;
using System.Text.Json.Serialization;
using VAA.EAPI.GraphQL.Common.Constants;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.PartnerMembership
{
    public class PartnerMembershipRequest
    {
        [JsonIgnore]
        public HttpMethod RequestType { get; set; }
        public PartnerMembership PartnerMembershipDocument { get; set; }

        public static explicit operator PartnerMembershipRequest(PartnerMembershipMutationRequest partnerMembershipRequest)
        {
            if (partnerMembershipRequest is null) throw new ArgumentNullException(nameof(partnerMembershipRequest));

            return new PartnerMembershipRequest()
            {
                PartnerMembershipDocument = new PartnerMembership
                {
                    Id = partnerMembershipRequest.Id,
                    CustomerId = partnerMembershipRequest.CustomerId,
                    PartnerId = partnerMembershipRequest.PartnerId,
                    PartnerCode = partnerMembershipRequest.PartnerCode,
                    PartnerStatusCode = partnerMembershipRequest.CardType,
                    Description = (partnerMembershipRequest.PartnerCode == Application.CenturionPartnerCode) ? Application.CenturionDescription : string.Empty,
                    StartDate = Convert.ToDateTime(partnerMembershipRequest.StartDate),
                    MemberNumber = partnerMembershipRequest.MemberNumber,
                    CreatedBy = partnerMembershipRequest.CreatedBy,
                    LastUpdatedBy = partnerMembershipRequest.LastUpdatedBy,
                    EndDate = string.IsNullOrEmpty(partnerMembershipRequest.EndDate) ? null : Convert.ToDateTime(partnerMembershipRequest.EndDate)
                },
                RequestType = partnerMembershipRequest.RequestType
            };
        }
    }
}