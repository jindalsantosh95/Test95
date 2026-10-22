namespace VAA.EAPI.Infrastructure.NDC.Sabre.Models
{
    public class PersonName
    {
        public string NameNumber { get; set; }
        public string NameReference { get; set; }
        public string PassengerType { get; set; }
        public string GivenName { get; set; }
        public string Surname { get; set; }
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public bool Infant { get;set; }
        public bool DocumentHolder {  get; set; }
        public bool LapChild {  get; set; }
    }
}
