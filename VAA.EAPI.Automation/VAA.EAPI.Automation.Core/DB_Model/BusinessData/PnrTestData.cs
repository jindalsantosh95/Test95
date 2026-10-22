using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.TestDataConfig;

namespace VAA.EAPI.Automation.Core.DB_Model.BusinessData
{
    public class PnrTestData
    {
        public static explicit operator PnrTestData(TestConfig source)
        {
            return new()
            {
                Env = Config.Environment,
                TestScenario = string.Join(", ", source.Search.JourneyType.ToString(), source.Book.Order.OutboundSectorInformation.ToString(), source.Book.Order.InboundSectorInformation?.ToString() ?? string.Empty),
                Pnr = source.Book.Order.PNR.ToString(),
                DepartureDate = source.Book.Order.DepartureDate.ToString(),
                LeadPassengerFirstName = source.Book.Order.LeadPassengerFirstName.ToString(),
                LeadPassengerLastName = source.Book.Order.LeadPassengerLastName.ToString(),
                TotalPrice = source.Book.Cost.TotalPrice.ToString(),
            };
        }

        public string Env { get; set; }
        public string TestScenario { get; set; }
        public string Pnr { get; set; }
        public string DepartureDate { get; set; }
        public string LeadPassengerFirstName { get; set; }
        public string LeadPassengerLastName { get; set; }
        public string TotalPrice { get; set; }

        public override string ToString()
        {
            return string.Join(',', Env.EscapeCsvValue(),
                                    TestScenario.EscapeCsvValue(),
                                    Pnr.EscapeCsvValue(),
                                    DepartureDate.EscapeCsvValue(),
                                    LeadPassengerFirstName.EscapeCsvValue(),
                                    LeadPassengerLastName.EscapeCsvValue(),
                                    TotalPrice.EscapeCsvValue());
        }

        public string ToMarkdownRow()
        {
            return $"| {Env} | {TestScenario} | {Pnr} | {DepartureDate} | {LeadPassengerFirstName} | {LeadPassengerLastName} | {TotalPrice} |";
        }
    }
}
