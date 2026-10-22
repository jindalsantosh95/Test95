namespace VAA.EAPI.Automation.Core.Delta.Models.OrderCreate
{
    public class Customer
    {
        public string birthDate { get; set; }
        public string emailAdr { get; set; }
        public string genderCode { get; set; }
        public string passengerId { get; set; }
        public string passengerTypeCode { get; set; }
        public PersonName personName { get; set; }
        public List<Phone> phones { get; set; }
    }
}