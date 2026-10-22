using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Core.DB_Model.BusinessData
{
    public class LoyaltyTestData
    {
        public static explicit operator LoyaltyTestData((string flyingclubNumber, string emailAddress, int points, string accountType, string name, bool underSixteen) source)
        {
            return new()
            {
                Env = Config.Environment,
                FlyingclubNumber = source.flyingclubNumber,
                EmailAddress = source.emailAddress,
                Points = source.points,
                AccountType = source.accountType,
                Name = source.name,
                UnderSixteen = source.underSixteen
            };
        }

        public string Env { get; set; }
        public string FlyingclubNumber { get; set; }
        public string EmailAddress { get; set; }
        public int Points { get; set; }
        public string AccountType { get; set; }
        public string Name { get; set; }
        public bool UnderSixteen { get; set; }

        public override string ToString()
        {
            return string.Join(',', Env.EscapeCsvValue(),
                                    FlyingclubNumber.EscapeCsvValue(),
                                    EmailAddress.EscapeCsvValue(),
                                    Points.EscapeCsvValue(),
                                    AccountType.EscapeCsvValue(),
                                    Name.EscapeCsvValue(),
                                    UnderSixteen.EscapeCsvValue());
        }

        public string ToMarkdownRow()
        {
            return $"| {Env} | {FlyingclubNumber} | {EmailAddress} | {Points} | {AccountType} | {Name} | {UnderSixteen} |";
        }
    }
}
