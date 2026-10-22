using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Loyalty.Sync.Domain.Entities.PointsCurrencyConverter;
using VAA.EAPI.Microservices.Loyalty.Sync.PointsCurrencyConvertServiceRef;

namespace VAA.EAPI.Microservices.Loyalty.Sync.Infrastructure.Mapper.CurrencyConvert
{
    public class CurrencyConvertDeltaMapper
    {
        public static ExecuteRequestType CurrencyConvertRequestMapper(IEnumerable<PointsCurrencyConverterRequest> currencyConverterRequest)
        {
            return new ExecuteRequestType()
            {
                RequestInfo = new RequestInfoType()
                {
                    AppChannelName = ConfigReader.Get(Constants.Environment.DFAppChannelName),
                    ApplicationId = Constants.Application.ApplicationId,
                    DebugOptions = Constants.Application.DebugOption,
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    TransactionId = "" //TBC
                },
                PayWithMilesMessage = currencyConverterRequest.Select(x => new PayWithMilesMessageType()
                {
                    CountryCode = x.CountryCode,
                    CurrencyCode = x.Amount.BaseCurrency,
                    PassengerTypeCode = x.PassengerTypeCode,
                    EligibilityIndicator = Constants.Application.EligibilityIndicator,
                    NumberOfPassengers = x.NumberOfPassengers.ToString(),
                    SkymilesBalance = x.LoyaltyPoints,
                    SkymilesTierStatus = x.Tier,
                    TotalFareAmount = x.Amount.MonetaryAmount
                }).ToArray()
            };
        }

        public static IEnumerable<PointsCurrencyConverterResponse> CurrencyConverterResponseMapper(ExecuteResponse deltaResponse)
        {
            return new List<PointsCurrencyConverterResponse>
            {
                new()
                {
                   PointsEquivalentCurrencyDetails = deltaResponse.ExecuteResponse1.PayWithMilesResponse.Select(x => new LoyaltyPointsEquivalentCurrency()
                   {
                       Amount = new() { BaseCurrency = x.CurrencyCode, MonetaryAmount = x.AmountOfFareReduction },
                       LoyaltyPoints = x.MileageRedemptionAmount,
                       PassengerTypeCode = x.ThreeCharPassengerTypeCode
                   }),
                   PtcWisePointsEquivalentCurrencyDetails = PtcWisePointsCurrencyDetailsMapper(deltaResponse.ExecuteResponse1.PayWithMilesPlusMoneyResponse)
                }
            };
        }

        public static IEnumerable<PtcWisePointsEquivalentCurrencyDetails> PtcWisePointsCurrencyDetailsMapper(IEnumerable<PayWithMilesResponseType> response)
        {
            var ptcWisePointsEquivalentCurrencyDetails = new List<PtcWisePointsEquivalentCurrencyDetails>();
            var ptcWisePointsEquivalentCurrency = new List<LoyaltyPointsEquivalentCurrency>();

            foreach (var item in response)
            {
                if (item.ThreeCharPassengerTypeCode == "ALL")
                {
                    LoyaltyPointsEquivalentCurrency totalPointsEquivalentCurrency = new()
                    {
                        Amount = new() { MonetaryAmount = item.AmountOfFareReduction, BaseCurrency = item.CurrencyCode },
                        LoyaltyPoints = item.MileageRedemptionAmount,
                        PassengerTypeCode = item.ThreeCharPassengerTypeCode
                    };
                    ptcWisePointsEquivalentCurrencyDetails.Add(new()
                    {
                        PtcWisePointsEquivalentCurrency = new List<LoyaltyPointsEquivalentCurrency>(ptcWisePointsEquivalentCurrency),
                        TotalPointsEquivalentCurrency = totalPointsEquivalentCurrency
                    });

                    ptcWisePointsEquivalentCurrency.Clear();
                }
                else
                {
                    ptcWisePointsEquivalentCurrency.Add(new LoyaltyPointsEquivalentCurrency()
                    {
                        Amount = new() { MonetaryAmount = item.AmountOfFareReduction, BaseCurrency = item.CurrencyCode },
                        LoyaltyPoints = item.MileageRedemptionAmount,
                        PassengerTypeCode = item.ThreeCharPassengerTypeCode
                    });
                }
            }

            return ptcWisePointsEquivalentCurrencyDetails;
        }
    }
}