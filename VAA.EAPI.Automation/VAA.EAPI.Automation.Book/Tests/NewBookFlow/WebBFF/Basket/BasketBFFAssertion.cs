using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.WebBFF.Basket
{
    public class BasketBFFAssertion
    {
        public static bool ValidateBasketSummaryCurrency(dynamic flightSummaryResponse, dynamic basketResponse, string expectedCurrency, out string message)
        {
            message = string.Empty;

            dynamic FlightSummaryResp = JsonConvert.DeserializeObject<dynamic>(flightSummaryResponse);

            var basketSummaryCurrency = basketResponse.summary.priceCurrency;
            var flightSummaryCurrency = FlightSummaryResp.price.currency;

            if (basketSummaryCurrency != flightSummaryCurrency)
            {
                message = $"Basket summary data was null in response \n ----------------- \n Basket Summary Currency: {basketResponse.summary.currency} \n  flightSummary Summary Currency: {basketResponse.summary.currency}\n ";
                return false;
            }

            if (basketSummaryCurrency != expectedCurrency)
            {
                message = $"Basket summary data was null in response \n ----------------- \n Basket Summary Currency: {basketResponse.summary.currency} \n Expected Currency: {expectedCurrency}\n";
                return false;
            }

            return true;
        }

        public static bool ValidateBasketSummaryPassengerCount(dynamic basketResponse, int adults, int teen, int child, int infant, out string message)
        {
            message = string.Empty;

            int basketSummaryAdultCount = basketResponse.summary.partyComposition.adults;
            int basketSummaryTeenCount = basketResponse.summary.partyComposition.teens;
            int basketSummaryChildCount = basketResponse.summary.partyComposition.children;
            int basketSummaryInfantCount = basketResponse.summary.partyComposition.infants;
            int totalPassengerCount = basketResponse.summary.partyComposition.total;
            int expectedPassengerCount = adults + teen + child + infant;

            if (basketSummaryAdultCount != adults)
            {
                message = $"Basket summary data was null in response \n ----------------- \n Basket Summary Adult Count: {basketSummaryAdultCount} \n  Expected Adult Count: {adults}\n ";
                return false;
            }

            if (basketSummaryTeenCount != teen)
            {
                message = $"Basket summary data was null in response \n ----------------- \n Basket Summary Teen Count: {basketSummaryTeenCount} \n  Expected Teen Count: {teen}\n ";
                return false;
            }

            if (basketSummaryChildCount != child)
            {
                message = $"Basket summary data was null in response \n ----------------- \n Basket Summary Child Count: {basketSummaryChildCount} \n  Expected Child Count: {child}\n ";
                return false;
            }

            if (basketSummaryInfantCount != infant)
            {
                message = $"Basket summary data was null in response \n ----------------- \n Basket Summary Infant Count: {basketSummaryInfantCount} \n  Expected Infant Count: {infant}\n ";
                return false;
            }

            if (totalPassengerCount != expectedPassengerCount)
            {
                message = $"Basket summary data was null in response \n ----------------- \n Basket Summary Total Passenger Count: {totalPassengerCount} \n  Expected Total Passenger Count: {expectedPassengerCount}\n ";
                return false;
            }

            return true;
        }

        public static bool ValidateBasketSummaryPassengerPrice(dynamic basketResponse, int adults, int teen, int child, int infant, out string message)
        {
            message = string.Empty;

            //Each Passenger count
            int basketSummaryAdultCount = basketResponse.summary.partyComposition.adults;
            int basketSummaryTeenCount = basketResponse.summary.partyComposition.teens;
            int basketSummaryChildCount = basketResponse.summary.partyComposition.children;
            int basketSummaryInfantCount = basketResponse.summary.partyComposition.infants;
            int totalPassengerCount = basketResponse.summary.partyComposition.total;
            int expectedPassengerCount = adults + teen + child + infant;

            //Pirce Per Person
            double pricePerPerson = basketResponse.summary.pricePerPerson;
            double expectedPricePerPerson = basketResponse.summary.totalPrice / (adults + teen + child);
            expectedPricePerPerson = Math.Round(expectedPricePerPerson, 2);

            if (basketSummaryAdultCount != adults)
            {
                message = $"Basket summary Adult Count data mismatch in response \n ----------------- \n Basket Summary Adult Count: {basketSummaryAdultCount} \n  Expected Adult Count: {adults}\n ";
                return false;
            }

            if (basketSummaryTeenCount != teen)
            {
                message = $"Basket summary teen Count data mismatch in response \n ----------------- \n Basket Summary Teen Count: {basketSummaryTeenCount} \n  Expected Teen Count: {teen}\n ";
                return false;
            }

            if (basketSummaryChildCount != child)
            {
                message = $"Basket summary child Count data mismatch in response \n ----------------- \n Basket Summary Child Count: {basketSummaryChildCount} \n  Expected Child Count: {child}\n ";
                return false;
            }

            if (basketSummaryInfantCount != infant)
            {
                message = $"Basket summary infant Count data mismatch in response \n ----------------- \n Basket Summary Infant Count: {basketSummaryInfantCount} \n  Expected Infant Count: {infant}\n ";
                return false;
            }

            if (totalPassengerCount != expectedPassengerCount)
            {
                message = $"Basket summary Passenger Count data mismatch in response \n ----------------- \n Basket Summary Total Passenger Count: {totalPassengerCount} \n  Expected Total Passenger Count: {expectedPassengerCount}\n ";
                return false;
            }

            if (pricePerPerson != expectedPricePerPerson)
            {
                message = $"Basket summary data was null in response \n ----------------- \n Basket Summary Total Price Per Passenger : {pricePerPerson} \n  Expected Total Price Per Passenger Count: {expectedPricePerPerson}\n ";
                return false;
            }

            return true;
        }

        public static bool ValidateBasketSummaryTotalPrice(dynamic flightSummaryResponse, dynamic basketResponse, out string message)
        {
            message = string.Empty;

            dynamic FlightSummaryResp = JsonConvert.DeserializeObject<dynamic>(flightSummaryResponse);

            //Total Price Basket
            string basketSummaryTotalPrice = basketResponse.summary.totalPrice;

            //Total Price Flight Summary price
            string flightSummaryTotalPrice = FlightSummaryResp.price.amountIncludingTax;

            if (basketSummaryTotalPrice != flightSummaryTotalPrice)
            {
                message = $"Basket summary Total price dont match in response \n ----------------- \n Basket Summary Total Price  : {basketSummaryTotalPrice} \n  Expected Total Price : {flightSummaryTotalPrice}\n ";
                return false;
            }

            return true;
        }

        //Basket Redemption points validation
        public static bool ValidateBasketMembershipPoints(HttpResponseMessage offerPriceResult, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());
            JObject offerPriceResponse = JObject.Parse(offerPriceResult.ReadResponseAsString());

            var basketMembershipPoints = basketResult.SelectToken(".summary.membershipPoints").ToString();
            var offerPriceMembershipPoints = offerPriceResponse.SelectToken(".price.awardPoints").ToString();

            if (basketMembershipPoints != offerPriceMembershipPoints)
            {
                message = $"The Basket Membership point {basketMembershipPoints} is not equal to offer price award points {offerPriceMembershipPoints} ";
                return false;
            }

            return true;
        }

        //Basket Flight price validation
        public static bool ValidateBasketFlightPrice(HttpResponseMessage offerPriceResult, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());
            JObject offerPriceResponse = JObject.Parse(offerPriceResult.ReadResponseAsString());

            var basketFlightPrice = basketResult.SelectToken(".summary.flightPrice").ToString();
            var offerPriceFlightPrice = offerPriceResponse.SelectToken(".price.amountIncludingTax").ToString();

            if (basketFlightPrice != offerPriceFlightPrice)
            {
                message = $"The Basket flightPrice {basketFlightPrice} is not equal to offer price amountIncludingTax {offerPriceFlightPrice} ";
                return false;
            }

            return true;
        }

        public static bool ValidateFCPointsEligibilityNegativeTestInsufficientPoints(dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            long basketMembershipPoints = (long)basketResult.SelectToken("...summary.membershipPoints");

            long fqtPoints = (long)basketResult.SelectToken("...customerProfile.membershipDetails.fqtPoints");

            bool hasSufficientPoints = (bool)basketResult.SelectToken("...summary.hasSufficientPoints");

            if (fqtPoints > basketMembershipPoints && !hasSufficientPoints)
            {
                message = $"The customer's FQT points {fqtPoints} are eligible because they meet or exceed the required basket membership points {basketMembershipPoints}.";
                return false;
            }

            if (fqtPoints < basketMembershipPoints && hasSufficientPoints)
            {
                message = $"The customer's FQT points {fqtPoints} are eligible because they meet or exceed the required basket membership points {basketMembershipPoints}.";
                return false;
            }

            return true;
        }
    }
}
