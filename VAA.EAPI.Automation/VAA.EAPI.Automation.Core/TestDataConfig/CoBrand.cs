namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class CoBrand
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Last4DigitsSSN { get; set; }
        public string MobileNumber { get; set; }
        public string StreetAddress { get; set; }
        public string CityAndState { get; set; }
        public string ZipCode { get; set; }
        public string SsnNumber { get; set; }
        public string DateOfBirth { get; set; }
        public string NetIncome { get; set; }

        public CoBrand()
        {
            Last4DigitsSSN = "6785";
            MobileNumber = "2234567891";
            StreetAddress = "460 N UNIVERSITY 201";
            CityAndState = "PROVO,UT";
            ZipCode = "84601";
            SsnNumber = "666286785";
            DateOfBirth = "04/03/1960";
            NetIncome = "10000";
        }
    }
}