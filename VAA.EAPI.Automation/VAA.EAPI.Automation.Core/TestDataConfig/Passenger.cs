using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Core.TestDataConfig
{
    public class Passenger
    {
        public string DateOfBirth { get; set; }
        public string Gender { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string PeopleLink { get; set; }
        public string PeopleId { get; set; }
        public string Type { get; set; }
        public string FQTProgramme { get; set; }
        public string FlyingClubNumber { get; set; }

        public Passenger()
        {
            Gender = Random.Shared.Next(0, 2) == 1 ? "MALE" : "FEMALE";
            FirstName = $"First{DataGenerator.RandomString(5)}";
            LastName = $"Last{DataGenerator.RandomString(5)}";
            FQTProgramme = null;
            FlyingClubNumber = null;
        }

        public string GenerateDateOfBirth(string type)
        {
            switch (type)
            {
                case "ADULT":
                    DateOfBirth = DateTime.Now.AddYears(-Random.Shared.Next(16, 100)).ToString("yyyy-MM-dd");
                    break;

                case "TEEN":
                    DateOfBirth = DateTime.Now.AddYears(-Random.Shared.Next(12, 15)).ToString("yyyy-MM-dd");
                    break;

                case "CHILD":
                    DateOfBirth = DateTime.Now.AddYears(-Random.Shared.Next(2, 11)).ToString("yyyy-MM-dd");
                    break;

                case "INFANT":
                    DateOfBirth = DateTime.Today.AddDays(-Random.Shared.Next(14, 730)).ToString("yyyy-MM-dd");
                    break;

                default:
                    throw new ArgumentException("Invalid type", nameof(type));
            }

            return DateOfBirth;
        }

        public static string GetRandomTier()
        {
            // Create an array of the allowed tiers 
            FlyingClubMemberTiers[] allowedTiers = new[] { FlyingClubMemberTiers.Red, FlyingClubMemberTiers.Silver, FlyingClubMemberTiers.Gold };

            // Select a random index
            int index = Random.Shared.Next(allowedTiers.Length);

            // Return the randomly selected tier
            return allowedTiers[index].ToString();
        }
    }
}
