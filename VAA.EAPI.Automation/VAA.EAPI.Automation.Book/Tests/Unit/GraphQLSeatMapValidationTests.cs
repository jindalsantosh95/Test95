using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL;
using VAA.EAPI.Automation.Core.Helpers.Enums;

namespace VAA.EAPI.Automation.Book.Tests.Unit
{
    public sealed class GraphQLSeatMapValidationTests
    {
        [Theory]
        [InlineData(FareFamilyType.ECON, "DL", false, true)]
        [InlineData(FareFamilyType.ECON, "VS", false, false)]
        [InlineData(FareFamilyType.ECON, "VS", true, true)]
        [InlineData(FareFamilyType.ECONCLASSICFLEXIBLE, "DL", false, false)]
        [InlineData(FareFamilyType.ECONCLASSICFLEXIBLE, "DL", true, true)]
        [InlineData(FareFamilyType.ECONCLASSICFLEXIBLE, "VS", false, false)]
        [InlineData(FareFamilyType.COMFORTPLUSPREMIUMECON, "DL", false, false)]
        [InlineData(FareFamilyType.COMFORTPLUSPREMIUMECON, "DL", true, true)]
        public void SeatMapValidation_PreservesFreeSeatRequirements(
            FareFamilyType fareFamily, string carrier, bool hasFreeSeat, bool expected)
        {
            JArray columns = new("SEAT", "PREFERRED_SEAT");
            if (hasFreeSeat)
            {
                columns.Add("FREE");
            }

            JObject payload = new()
            {
                ["data"] = new JObject
                {
                    ["retrieveSeatMap"] = new JObject
                    {
                        ["flightSegment"] = new JObject { ["operatingCarrier"] = carrier },
                        ["seatMap"] = new JObject
                        {
                            ["rows"] = new JArray(new JObject { ["columns"] = columns })
                        }
                    }
                }
            };
            using HttpResponseMessage response = new()
            {
                Content = new StringContent(payload.ToString(Formatting.None))
            };

            bool valid = GraphQLAssertion.SeatMapValidation(response, fareFamily, out string message);

            Assert.Equal(expected, valid);
            if (expected)
            {
                Assert.Empty(message);
            }
            else
            {
                Assert.Contains("No seat returned", message);
            }
        }
    }
}
