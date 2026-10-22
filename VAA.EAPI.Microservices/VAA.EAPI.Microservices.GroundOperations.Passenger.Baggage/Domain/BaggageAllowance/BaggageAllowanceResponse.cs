using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance
{
    public class BaggageAllowanceResponse
    {
        public string TravelPartyId { get; set; }
        public IEnumerable<PassengerAllowance> Passengers { get; set; }
        public bool ShowBaggageAllowance { get; set; }
        public AllAboutBagsResponse AllAboutBags { get; set; }
        public string BaggageLearnMoreUrl { get; set; } = ConfigReader.Get(Constants.Environment.BaggageUrl);

        public static explicit operator BaggageAllowanceResponse((BagsData bagsData, BaggageAdditionalDetails additionalBagsDetails, bool hidePaidBaggage) source)
        {
            var response = new BaggageAllowanceResponse
            {
                ShowBaggageAllowance = true,
                Passengers = source.bagsData.PassengerBagInfo?.Select(psgBag => (PassengerAllowance)(psgBag, source.additionalBagsDetails, source.hidePaidBaggage)) ?? [],
                AllAboutBags = source.additionalBagsDetails.AllAboutBags
            };

            if (response.Passengers.All(p => p.BaggageOffers.IsEmpty()) || response.Passengers.All(p => p.BaggageOffers.All(bo => bo.Price.Amount > 0 && bo.Price.DecimalPrecisionCount != 0)))
            {
                response.ShowBaggageAllowance = false;
            }

            return response;
        }
    }
}
