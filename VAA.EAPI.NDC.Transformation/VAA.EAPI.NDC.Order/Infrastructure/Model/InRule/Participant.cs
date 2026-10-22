using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using SourceParticipant = VAA.EAPI.NDC.Order.Infrastructure.Model.Participant;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class Participant
    {
        public SourceParticipant.PartyTypeEnum? PartyType { get; set; }
        public string Name { get; set; }
        public string AgencyID { get; set; }
        public string Ordinal { get; set; }
        public string AccountCode { get; set; }
        public string PccCode { get; set; }
        public Contacts Contacts { get; set; }
        public bool IsCreditCardAccepted { get; set; }
        public string RequestAgentType { get; set; }

        public static explicit operator Participant(SourceParticipant source) =>
            source is null
                ? null
                : new Participant
                {
                    PartyType = source.PartyType,
                    Name = source.Name,
                    AgencyID = source.AgencyID,
                    Ordinal = source.Ordinal,
                    AccountCode = source.AccountCode,
                    PccCode = source.PccCode,
                    Contacts = source.Contacts,
                    IsCreditCardAccepted = source.IsCreditCardAccepted,
                    RequestAgentType = source.RequestAgentType
                };
    }
}
