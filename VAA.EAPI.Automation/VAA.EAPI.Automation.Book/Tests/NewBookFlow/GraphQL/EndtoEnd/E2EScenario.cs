using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using VAA.EAPI.Automation.Core.Constants;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using Xunit.Abstractions;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL.EndtoEnd
{
    public class E2EScenario : IXunitSerializable
    {
        public E2EScenario() { }
        [JsonProperty("testName")]
        public string TestName { get; set; }

        [JsonProperty("origin"), JsonConverter(typeof(StringEnumConverter))]
        public LocationType Origin { get; set; }

        [JsonProperty("destination"), JsonConverter(typeof(StringEnumConverter))]
        public LocationType Destination { get; set; }

        [JsonProperty("multiCityLocations", ItemConverterType = typeof(StringEnumConverter))]
        public LocationType[] MultiCityLocations { get; set; }

        [JsonProperty("journeyType"), JsonConverter(typeof(StringEnumConverter))]
        public JourneyType JourneyType { get; set; } = JourneyType.OneWay;

        [JsonProperty("fareFamily"), JsonConverter(typeof(StringEnumConverter))]
        public FareFamilyType FareFamily { get; set; } = FareFamilyType.ECON;

        [JsonProperty("flightType"), JsonConverter(typeof(StringEnumConverter))]
        public FlightType FlightType { get; set; } = FlightType.VS;

        [JsonProperty("inboundFlightType"), JsonConverter(typeof(StringEnumConverter))]
        public FlightType? InboundFlightType { get; set; }

        [JsonProperty("adults")]
        public int Adults { get; set; } = 1;

        [JsonProperty("teens")]
        public int Teens { get; set; } = 1;

        [JsonProperty("children")]
        public int Children { get; set; }

        [JsonProperty("validateChildMealRemarks")]
        public bool ValidateChildMealRemarks { get; set; }

        [JsonProperty("infants")]
        public int Infants { get; set; }

        [JsonProperty("bookingType"), JsonConverter(typeof(StringEnumConverter))]
        public BookingType BookingType { get; set; } = BookingType.Revenue;

        [JsonProperty("loyaltyTierCode")]
        public string LoyaltyTierCode { get; set; }

        [JsonProperty("minLoyaltyPoints")]
        public int MinLoyaltyPoints { get; set; } = 1000000;

        [JsonProperty("payWithPointsAmount")]
        public string PayWithPointsAmount { get; set; }

        [JsonProperty("paymentMethod"), JsonConverter(typeof(StringEnumConverter))]
        public PaymentMethod PaymentMethod { get; set; } = PaymentMethod.Card;

        [JsonProperty("cardNumber")]
        public string CardNumber { get; set; } = "4761344136141390";

        [JsonProperty("cardType")]
        public string CardType { get; set; } = "Visa";

        [JsonProperty("is3DS")]
        public bool Is3DS { get; set; }

        [JsonProperty("usePlaywrightForHpp")]
        public bool UsePlaywrightForHpp { get; set; }

        [JsonProperty("paymentCountry")]
        public string PaymentCountry { get; set; } = "United Kingdom";

        [JsonProperty("seatMode"), JsonConverter(typeof(StringEnumConverter))]
        public SeatMode SeatMode { get; set; } = SeatMode.None;

        [JsonProperty("seatProductName")]
        public string SeatProductName { get; set; }

        [JsonProperty("cabinName"), JsonConverter(typeof(StringEnumConverter))]
        public CabinName? CabinName { get; set; }

        [JsonProperty("insurance"), JsonConverter(typeof(StringEnumConverter))]
        public InsuranceAction Insurance { get; set; } = InsuranceAction.None;

        [JsonProperty("expectFraudCheck")]
        public bool ExpectFraudCheck { get; set; }

        [JsonProperty("useNearDepartureDate")]
        public bool UseNearDepartureDate { get; set; }

        [JsonProperty("departureDate")]
        public string DepartureDate { get; set; }

        [JsonProperty("isCoBrand")]
        public bool IsCoBrand { get; set; }

        [JsonProperty("coBrandLastName")]
        public string CoBrandLastName { get; set; }

        [JsonProperty("coBrandFirstName")]
        public string CoBrandFirstName { get; set; }

        [JsonProperty("synchronyApplicationStatus")]
        public string SynchronyApplicationStatus { get; set; }

        [JsonProperty("isRewardSearch")]
        public bool IsRewardSearch { get; set; }

        [JsonProperty("flyingClubPassenger")]
        public bool FlyingClubPassenger { get; set; }

        [JsonProperty("isLoggedIn")]
        public bool IsLoggedIn { get; set; }

        [JsonProperty("flyingClubTier")]
        public string FlyingClubTier { get; set; }

        [JsonProperty("categories")]
        public string[] Categories { get; set; } = new[] { GlobalTraits.LocalRun };

        [JsonProperty("useExplicitSearchOverload")]
        public bool UseExplicitSearchOverload { get; set; }

        [JsonProperty("useUpdateBasketShortcut")]
        public bool UseUpdateBasketShortcut { get; set; }

        [JsonProperty("randomOffer")]
        public bool RandomOffer { get; set; }

        [JsonProperty("requiredLegOperatingAirlineCode")]
        public string RequiredLegOperatingAirlineCode { get; set; }

        [JsonProperty("validateInsurancePriceWithPaymentMS")]
        public bool ValidateInsurancePriceWithPaymentMS { get; set; }

        [JsonProperty("validateInsuranceInOrderDoc")]
        public bool ValidateInsuranceInOrderDoc { get; set; }

        [JsonProperty("validateRejectedInsuranceInOrderDoc")]
        public bool ValidateRejectedInsuranceInOrderDoc { get; set; }

        [JsonProperty("useSynchronyCardPayment")]
        public bool UseSynchronyCardPayment { get; set; }

        [JsonProperty("fullPointsSelection")]
        public bool FullPointsSelection { get; set; }

        [JsonProperty("hasPaidSeat")]
        public bool HasPaidSeat { get; set; }

        [JsonProperty("hasFreeSeat")]
        public bool HasFreeSeat { get; set; }

        [JsonProperty("useRetryHelperForLoyalty")]
        public bool UseRetryHelperForLoyalty { get; set; }

        [JsonProperty("paymentCardType"), JsonConverter(typeof(StringEnumConverter))]
        public CardType? PaymentCardType { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("postcode")]
        public string Postcode { get; set; }

        [JsonProperty("isMultiCity")]
        public bool IsMultiCity { get; set; }

        [JsonProperty("writeToAzure")]
        public bool WriteToAzure { get; set; } = true;

        [JsonProperty("validateRedemptionBasket")]
        public bool ValidateRedemptionBasket { get; set; }

        [JsonProperty("validatePaymentRetreive")]
        public bool ValidatePaymentRetreive { get; set; }

        [JsonProperty("validateVoucherConfirmation")]
        public bool ValidateVoucherConfirmation { get; set; }

        [JsonProperty("voucherRedemptionType")]
        public string VoucherRedemptionType { get; set; }

        [JsonProperty("voucherType")]
        public string VoucherType { get; set; }

        [JsonProperty("exactVoucherCount")]
        public int? ExactVoucherCount { get; set; }

        [JsonProperty("expectedAppliedVoucherCount")]
        public int? ExpectedAppliedVoucherCount { get; set; }

        [JsonProperty("requireAllVouchersMatchType")]
        public bool RequireAllVouchersMatchType { get; set; }

        [JsonProperty("voucherMinimumPoints")]
        public int? VoucherMinimumPoints { get; set; }

        public void Deserialize(IXunitSerializationInfo info)
        {
            var json = info.GetValue<string>("json");
            JsonConvert.PopulateObject(json, this, new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            });
        }

        public void Serialize(IXunitSerializationInfo info)
        {
            var json = JsonConvert.SerializeObject(this, new JsonSerializerSettings
            {
                Converters = { new StringEnumConverter() }
            });
            info.AddValue("json", json);
        }

        public override string ToString() => TestName;
    }
}
