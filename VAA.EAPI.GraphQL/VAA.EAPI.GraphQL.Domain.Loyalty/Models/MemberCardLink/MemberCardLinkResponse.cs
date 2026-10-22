
using System;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberCardLink
{
    public class MemberCardLinkResponse
    {
        public string DocumentId { get; set; }
        public string PartnerCode { get; set; }
        public string PartnerName { get; set; }
        public string CardType { get; set; }
        public DateTime? LinkedDate { get; set; }
        public DateTime? EndDate { get; set; }
        public DateTime? AnniversaryDate { get; set; }
        public string PAN { get; set; }
    }
}
