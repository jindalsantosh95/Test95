namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class Contact
    {
        public string PhoneNumber { get; set; }
        public string AltPhoneNumber { get; set; }
        public string EmailId { get; set; }
        public bool AcceptPost { get; set; }
        public bool AcceptTelephone { get; set; }
        public bool AcceptEmail { get; set; }
        public bool AcceptTextMsg { get; set; }
        public string CountryCode { get; set; }
        public string PeopleId { get; set; }

        public Contact()
        {
            PhoneNumber = GenerateRandomPhoneNumber("746"); ;
            AltPhoneNumber = GenerateRandomPhoneNumber("746"); ;
            EmailId = GenerateRandomEmail();
            AcceptPost = false;
            AcceptTelephone = false;
            AcceptEmail = true;
            AcceptTextMsg = false;
            CountryCode = "44";
            PeopleId = null;
        }

        private string GenerateRandomPhoneNumber(string prefix)
        {
            string randomPart = Random.Shared.Next(1000000, 9999999).ToString();
            return $"{prefix}{randomPart}";
        }

        private string GenerateRandomEmail()
        {
            const string chars = "abcdefghijklmnopqrstuvwxyz0123456789";
            char[] randomChars = new char[6];

            for (int i = 0; i < randomChars.Length; i++)
                randomChars[i] = chars[Random.Shared.Next(chars.Length)];

            return new string(randomChars) + "@example.com";
        }
    }
}
