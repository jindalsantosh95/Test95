using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.SaveBags
{
    public class SaveBaggageRequest
    {
        public string TravelPartyId {get; set;}
        public IEnumerable<BaggagePricing> BaggagePricing { get; set; }

        public static explicit operator UpdateBagsRequest(SaveBaggageRequest request)
        {
            return new UpdateBagsRequest
            {
                CheckedBagsCount = [..request.BaggagePricing.Select(x => x.BaggageCount.ToString())],
                PassengerNin = [.. request.BaggagePricing.Select(x => x.PassengerNiN)],
                TotalOfferItemid = [.. request.BaggagePricing.Select(x => x.BaggageOfferId)],
                TotalBagFee = [.. request.BaggagePricing.Select(x => x.TotalPrice.ToString())],
                SpecialItemFlag = [..request.BaggagePricing.Select(_ => "false")],
                TotalMilesFee = [.. request.BaggagePricing.Select(_ => "0")],
            };
        }
    }
}
