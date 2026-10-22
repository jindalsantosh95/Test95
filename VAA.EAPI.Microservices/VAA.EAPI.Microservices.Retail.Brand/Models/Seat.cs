using System.Linq;

namespace VAA.EAPI.Microservices.Retail.Brand.Models
{
    public class Seat : AmenityBase
    {
        private static readonly string[] fareFamilyDelight =
           [
                "ECONOMY-DELIGHT", "ECON-DELIGHT-FLEXIBLE"
           ];
        private static readonly string[] fareFamilyPremium =
            [
                 "COMFORT-PLUS-PREMIUM-ECON", "COMFORT-PLUS-PREMIUM-FLEXIBLE", "AWARD-COMFORT-PLUS-PREMIUM-ECONOMY"
            ];
        private static readonly string[] fareFamilyUpper =
            [
                 "BUS-FIRST", "BUS-FIRST-FLEXIBLE", "AWARD-BUSINESS-FIRST"
            ];

        public string Flatness { get; set; }
        public string Legroom { get; set; }
        public string Pitch { get; set; }
        public string Width { get; set; }

        public static explicit operator Seat((RouteHappy.Seat source, string operatingCarrier, string fareFamily) data)
        {
            if (data.source == null) return null;

            bool showSeatPitch = true;
            switch (data.operatingCarrier)
            {
                case "VS":
                    showSeatPitch = fareFamilyDelight.Contains(data.fareFamily) || fareFamilyPremium.Contains(data.fareFamily) || fareFamilyUpper.Contains(data.fareFamily) ? false : true;
                    break;

                case "DL":
                    showSeatPitch = fareFamilyDelight.Contains(data.fareFamily) ? false : true;
                    break;

                default:
                    break;
            }

            return new()
            {
                DisplayText = showSeatPitch ? data.source.DisplayText : default,
                Quality = data.source.Quality.ToString(),
                Type = data.source.Type.ToString(),
                Pitch = showSeatPitch ? data.source.Pitch : default,
                Width = data.source.Width.ToString(),
                Flatness = data.source.Flatness.ToString(),
                Legroom = data.source.Legroom.ToString()
            };
        }
    }
}