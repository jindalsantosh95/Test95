using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink
{
    public class VredCardLink
    {
        public string CardLinkPartnerUserIdDomain { get; set; }
        public string PartnerName { get; set; }
        public DateTime? LinkedDateTime { get; set; }
        public DateTime? AnniversaryDateTime { get; set; }
        public string CardType { get; set; }
        public DateTime? CancelledDateTime { get; set; }
        public string CardLinkPartnerUserId { get; set; }
    }
}
