using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.DeltaSsci;

namespace VAA.EAPI.Microservices.GroundOperations.Passenger.Baggage.Domain.BaggageAllowance
{
    public class PassengerAllowance
    {
        public string GivenNames { get; set; }
        public string Surname { get; set; }
        public string PassengerNiN { get; set; }
        public int NumSelectedBags { get; set; }
        public bool CanChangeBags { get; set; }
        public bool IsInfantPassenger { get; set; } 
        public BaggageAdvisory? BaggageAdvisory { get; set; } 
        public IEnumerable<BaggageOffer> BaggageOffers { get; set; }

        public static explicit operator PassengerAllowance((PassengerBagInfo psgBagInfo, BaggageAdditionalDetails additionalBagsDetails, bool hidePaidBaggage) source)
        {
            var psgBagInfo = source.psgBagInfo;

            var allowance = new PassengerAllowance
            {
                GivenNames = psgBagInfo.FirstName,
                Surname = psgBagInfo.LastName,
                PassengerNiN = $"{psgBagInfo.FirstNIN}.{psgBagInfo.LastNIN}",
                IsInfantPassenger = psgBagInfo.InfantIndicator,
                NumSelectedBags = psgBagInfo.PendingBagsCount,
                CanChangeBags = psgBagInfo.BaggagePrices != null
            };

            if (psgBagInfo.BaggagePrices != null)
            {
                allowance.BaggageOffers = psgBagInfo.BaggagePrices.Select((bagPrice, priceIndex) =>
                {
                    double currentPrice = bagPrice.BaggagePrice;
                    double previousPrice = priceIndex > 0 ? psgBagInfo.BaggagePrices.ToList()[priceIndex - 1].BaggagePrice : 0.0;

                    double baggageAmount = priceIndex > 0 ? currentPrice - previousPrice : currentPrice;

                    return (BaggageOffer)(bagPrice, priceIndex, baggageAmount);
                });
            }
            else
            {
                var baggageOffers = new List<BaggageOffer>();
                for (int i = 1; i <= allowance.NumSelectedBags; i++)
                {
                    baggageOffers.Add(new BaggageOffer
                    {
                        BaggageOfferId = string.Empty,
                        BaggageCount = i,
                        Price = new BaggagePrice
                        {
                            Amount = 0,
                            Currency = string.Empty,
                            AdditionalBagPrice = 0,
                            DecimalPrecisionCount = 0
                        }
                    });
                }
                allowance.BaggageOffers = baggageOffers;
            }

            if (psgBagInfo.InfantIndicator) allowance.BaggageAdvisory = source.additionalBagsDetails?.InfantBaggageAdvisory;
            else if (source.hidePaidBaggage || psgBagInfo.Currency == null || source.additionalBagsDetails?.DisablePaidBaggageCurrencies.Contains(psgBagInfo.Currency.Replace("(", string.Empty).Replace(")", string.Empty)) == true)
            {
                allowance.BaggageAdvisory = source.additionalBagsDetails?.UnsupportedCurrencyBaggageAdvisory;
                allowance.BaggageOffers = allowance.BaggageOffers.Where(bo => bo.Price.Amount == 0);
            }
            else if (psgBagInfo.PendingBagsCount > 0 &&  !allowance.CanChangeBags)
            {
                allowance.BaggageAdvisory = new BaggageAdvisory
                {
                    Message = source.additionalBagsDetails.AlreadyPurchasedBaggageAdvisory.Message.Replace("{name}", !psgBagInfo.FirstName.IsNullOrEmpty() ? char.ToUpper(psgBagInfo.FirstName[0]) + psgBagInfo.FirstName[1..] : null)
                };
            }

            return allowance;
        }
    }
}
