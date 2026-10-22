using System;
using System.Text;
using System.Collections.Generic;


namespace VAA.EAPI.NDC.Offer.Domain.Model.Order
{
    public class Party
    {
        public NDCParty Sender { get; set; }
        public IEnumerable<NDCParty> Participants { get; set; }
        public Credential PartyId { get; set; }
        public string PartyLegalName { get; set; }
        public string PartyOwner { get; set; }
        public string IATANumber { get; set; }
        public string PCC { get; set; }
        public string PartyType { get; set; }
        public string PartySubType { get; set; }
        public string Status { get; set; }
        public DateTime? AgreementEffectiveDateStart { get; set; }
        public DateTime? AgreementEffectiveDateEnd { get; set; }
        public IEnumerable<PartyIdentityDocument> PartyIdentityDocuments { get; set; }
        public ContactsCollection PartyContacts { get; set; }
        public IEnumerable<CredentialsCollection> PartyCredentials { get; set; }
        public PartyRelationshipCollection Relationships { get; set; }
        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append("class Party {\n");
            sb.Append("  Sender: ").Append(Sender).Append("\n");
            sb.Append("  Participants: ").Append(Participants).Append("\n");
            sb.Append("  PartyId: ").Append(PartyId).Append("\n");
            sb.Append("  PartyLegalName: ").Append(PartyLegalName).Append("\n");
            sb.Append("  PartyOwner: ").Append(PartyOwner).Append("\n");
            sb.Append("  IATANumber: ").Append(IATANumber).Append("\n");
            sb.Append("  PCC: ").Append(PCC).Append("\n");
            sb.Append("  PartyType: ").Append(PartyType).Append("\n");
            sb.Append("  PartySubType: ").Append(PartySubType).Append("\n");
            sb.Append("  Status: ").Append(Status).Append("\n");
            sb.Append("  AgreementEffectiveDateStart: ").Append(AgreementEffectiveDateStart).Append("\n");
            sb.Append("  AgreementEffectiveDateEnd: ").Append(AgreementEffectiveDateEnd).Append("\n");
            sb.Append("  PartyIdentityDocuments: ").Append(PartyIdentityDocuments).Append("\n");
            sb.Append("  PartyContacts: ").Append(PartyContacts).Append("\n");
            sb.Append("  PartyCredentials: ").Append(PartyCredentials).Append("\n");
            sb.Append("  Relationships: ").Append(Relationships).Append("\n");
            sb.Append("}\n");
            return sb.ToString();
        }

    }
}
