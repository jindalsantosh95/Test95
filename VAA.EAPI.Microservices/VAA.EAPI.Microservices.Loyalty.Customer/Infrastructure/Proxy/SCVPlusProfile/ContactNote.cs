namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Proxy.SCVPlusProfile
{
    public partial class ContactNote
    {
        public ContactNote(string noteType = default, string note = default, string noteGroup = default)
        {
            NoteType = noteType;
            Note = note;
            NoteGroup = noteGroup;
        }

        public string NoteType { get; set; }
        public string Note { get; set; }
        public string NoteGroup { get; set; }
    }
}
