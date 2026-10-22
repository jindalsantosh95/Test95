using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Book.Helpers;
using VAA.EAPI.Automation.Core.DB_Model.Entities.OrderDocument;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Microservices.Order.Search.Domain.BookingDetails.Request;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.GraphQL
{
    public class GraphQLAssertion
    {
        public static bool ValidateVoucherConfirmationConsistency(
            JToken orderCreateResponse,
            JToken finalBasketResponse,
            string expectedRedemptionType,
            int? expectedAppliedVoucherCount,
            out string message)
        {
            JToken confirmation = orderCreateResponse?.SelectToken("$.data.bookCreateOrder.summary");
            JToken basket = finalBasketResponse?.SelectToken("$.data.retrieveBasket.summary");
            if (confirmation == null || basket == null)
            {
                message = $"Voucher confirmation comparison requires both summaries. ConfirmationPresent={confirmation != null}. BasketPresent={basket != null}.";
                return false;
            }

            if (confirmation.SelectToken("voucherApplied")?.Value<bool?>() != true ||
                basket.SelectToken("voucherApplied")?.Value<bool?>() != true)
            {
                message = $"voucherApplied was not true in both the confirmation and final basket. Confirmation={confirmation.SelectToken("voucherApplied")}. Basket={basket.SelectToken("voucherApplied")}.";
                return false;
            }

            foreach (string field in new[] { "hasSufficientPoints", "voucherPoints", "pointsToPay" })
            {
                string confirmationValue = confirmation.SelectToken(field)?.ToString() ?? string.Empty;
                string basketValue = basket.SelectToken(field)?.ToString() ?? string.Empty;
                if (confirmationValue.IsNullOrWhiteSpace() || !confirmationValue.Is(basketValue))
                {
                    message = $"Confirmation field '{field}' did not match the final basket. Confirmation='{confirmationValue}'. Basket='{basketValue}'.";
                    return false;
                }
            }

            JToken confirmationSelection = confirmation.SelectToken("voucherRedemptionSelection");
            JToken basketSelection = basket.SelectToken("voucherRedemptionSelection");
            if (confirmationSelection == null || basketSelection == null)
            {
                message = $"voucherRedemptionSelection was not returned in both summaries. ConfirmationPresent={confirmationSelection != null}. BasketPresent={basketSelection != null}.";
                return false;
            }

            string selectedOptionId = confirmationSelection?.SelectToken("selectedOptionId")?.ToString() ?? string.Empty;
            if (selectedOptionId.IsNullOrWhiteSpace() ||
                !selectedOptionId.Is(basketSelection?.SelectToken("selectedOptionId")?.ToString()))
            {
                message = $"The selected voucher option was not preserved in confirmation. Confirmation={confirmationSelection}. Basket={basketSelection}.";
                return false;
            }

            List<string> confirmationVoucherIds = confirmationSelection
                .SelectTokens("selectedVoucherIds[*]")
                .Select(value => value.ToString())
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            List<string> basketVoucherIds = basketSelection
                .SelectTokens("selectedVoucherIds[*]")
                .Select(value => value.ToString())
                .OrderBy(value => value, StringComparer.OrdinalIgnoreCase)
                .ToList();
            if (confirmationVoucherIds.Count == 0 || !confirmationVoucherIds.SequenceEqual(basketVoucherIds, StringComparer.OrdinalIgnoreCase))
            {
                message = $"Selected voucher IDs did not match. Confirmation={string.Join(",", confirmationVoucherIds)}. Basket={string.Join(",", basketVoucherIds)}.";
                return false;
            }

            JObject confirmationOption = confirmation
                .SelectTokens("voucherDetails.redemptionOptions[*]")
                .OfType<JObject>()
                .FirstOrDefault(option => option.SelectToken("optionId")?.ToString().Is(selectedOptionId) == true);
            JObject basketOption = basket
                .SelectTokens("voucherDetails.redemptionOptions[*]")
                .OfType<JObject>()
                .FirstOrDefault(option => option.SelectToken("optionId")?.ToString().Is(selectedOptionId) == true);
            if (confirmationOption == null || basketOption == null ||
                confirmationOption.SelectToken("redemptionType")?.ToString().Is(expectedRedemptionType) != true ||
                basketOption.SelectToken("redemptionType")?.ToString().Is(expectedRedemptionType) != true)
            {
                message = $"The selected {expectedRedemptionType} option was not returned in both summaries. ConfirmationOption={confirmationOption}. BasketOption={basketOption}.";
                return false;
            }

            Dictionary<string, decimal> confirmationAppliedVouchers = GetAppliedVoucherPoints(confirmationOption);
            Dictionary<string, decimal> basketAppliedVouchers = GetAppliedVoucherPoints(basketOption);
            if (expectedAppliedVoucherCount.HasValue && confirmationAppliedVouchers.Count != expectedAppliedVoucherCount.Value)
            {
                message = $"Confirmation applied {confirmationAppliedVouchers.Count} voucher(s); expected {expectedAppliedVoucherCount.Value}. ConfirmationOption={confirmationOption}.";
                return false;
            }

            if (confirmationAppliedVouchers.Count == 0 ||
                confirmationAppliedVouchers.Count != basketAppliedVouchers.Count ||
                confirmationAppliedVouchers.Any(item => !basketAppliedVouchers.TryGetValue(item.Key, out decimal basketPoints) || basketPoints != item.Value))
            {
                message = $"Applied voucher IDs or pointsUsed did not match. Confirmation={JsonConvert.SerializeObject(confirmationAppliedVouchers)}. Basket={JsonConvert.SerializeObject(basketAppliedVouchers)}.";
                return false;
            }

            if (!decimal.TryParse(confirmation.SelectToken("voucherPoints")?.ToString(), out decimal voucherPoints) ||
                voucherPoints != confirmationAppliedVouchers.Values.Sum())
            {
                message = $"Confirmation voucherPoints did not equal the total applied voucher pointsUsed. voucherPoints={confirmation.SelectToken("voucherPoints")}. AppliedPoints={confirmationAppliedVouchers.Values.Sum()}.";
                return false;
            }

            message = $"{expectedRedemptionType} voucher confirmation matched the final basket for option {selectedOptionId} and voucher(s) {string.Join(",", confirmationVoucherIds)}.";
            return true;
        }

        private static Dictionary<string, decimal> GetAppliedVoucherPoints(JObject option)
        {
            return option
                .SelectTokens("appliedVouchers[*]")
                .OfType<JObject>()
                .Where(voucher => !(voucher.SelectToken("voucherId")?.ToString() ?? string.Empty).IsNullOrWhiteSpace())
                .ToDictionary(
                    voucher => voucher.SelectToken("voucherId").ToString(),
                    voucher => voucher.SelectToken("pointsUsed")?.Value<decimal?>() ?? 0M,
                    StringComparer.OrdinalIgnoreCase);
        }

        public static bool ValidateBasketSummaryCurrency(dynamic flightSummaryResponse, dynamic basketResponse, string expectedCurrency, out string message)
        {
            message = string.Empty;

            // JToken myRequest = JsonConvert.DeserializeObject<dynamic>(jsonResponse);

            var basketSummaryCurrency = basketResponse.data.retrieveBasket.summary.priceCurrency;
            var flightSummaryCurrency = flightSummaryResponse.data.searchSummary.price.currency;

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

            //Each Passenger count
            int basketSummaryAdultCount = basketResponse.data.retrieveBasket.summary.partyComposition.adults;
            int basketSummaryTeenCount = basketResponse.data.retrieveBasket.summary.partyComposition.teens;
            int basketSummaryChildCount = basketResponse.data.retrieveBasket.summary.partyComposition.children;
            int basketSummaryInfantCount = basketResponse.data.retrieveBasket.summary.partyComposition.infants;
            int totalPassengerCount = basketResponse.data.retrieveBasket.summary.partyComposition.total;
            int expectedPassengerCount = adults + teen + child + infant;

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

            return true;
        }

        public static bool ValidateBasketSummaryPassengerPrice(dynamic basketResponse, int adults, int teen, int child, int infant, out string message)
        {
            message = string.Empty;

            //Each Passenger count
            int basketSummaryAdultCount = basketResponse.data.retrieveBasket.summary.partyComposition.adults;
            int basketSummaryTeenCount = basketResponse.data.retrieveBasket.summary.partyComposition.teens;
            int basketSummaryChildCount = basketResponse.data.retrieveBasket.summary.partyComposition.children;
            int basketSummaryInfantCount = basketResponse.data.retrieveBasket.summary.partyComposition.infants;
            int totalPassengerCount = basketResponse.data.retrieveBasket.summary.partyComposition.total;
            int expectedPassengerCount = adults + teen + child + infant;

            //Pirce Per Person
            double pricePerPerson = basketResponse.data.retrieveBasket.summary.pricePerPerson;
            double expectedPricePerPerson = basketResponse.data.retrieveBasket.summary.totalPrice / (adults + teen + child);
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

            //Total Price Basket
            var basketSummaryTotalPrice = basketResponse.data.retrieveBasket.summary.totalPrice;

            //Total Price Flight Summary price
            var flightSummaryTotalPrice = flightSummaryResponse.data.searchSummary.price.amountIncludingTax;

            if (basketSummaryTotalPrice != flightSummaryTotalPrice)
            {
                message = $"Basket summary Total price dont match in response \n ----------------- \n Basket Summary Total Price  : {basketSummaryTotalPrice} \n  Expected Total Price Per Passenger Count: {flightSummaryTotalPrice}\n ";
                return false;
            }

            return true;
        }

        public static bool ValidatePassengerBasketMembershipDetails(dynamic basketResponse, string basketId, string passengerId, out string message)
        {
            message = string.Empty;

            // Validate that the basket ID matches
            if (basketResponse.data.updatePeople.basketId.Value != basketId)
            {
                message = "Basket ID doesn't match";
                return false;
            }

            var people = (JArray)basketResponse.data.updatePeople.people;

            foreach (var item in people)
            {
                if (item["id"].ToString() == passengerId)
                {
                    if (!ValidateMembershipDetails(item, out string validationMessage))
                    {
                        message = validationMessage;
                        return false;
                    }
                }
            }

            return true;
        }

        private static bool ValidateMembershipDetails(dynamic item, out string message)
        {
            message = string.Empty;

            var membershipDetails = item["membershipDetails"];
            var fqtProgramme = membershipDetails["fqtProgramme"].ToString();
            var fqtNumber = membershipDetails["fqtNumber"].ToString();
            var membershipValidation = membershipDetails["validation"]?.ToString();
            var fqtValidityStatus = membershipDetails["fqtValidityStatus"]?.ToString();
            var itemValidation = item["validation"].ToString();
            var itemType = item["type"].ToString();
            var itemId = item["id"].ToString();

            bool areMembershipDetailsEmpty = string.IsNullOrEmpty(fqtProgramme) && string.IsNullOrEmpty(fqtNumber);

            if (areMembershipDetailsEmpty)
            {
                bool isInfantPassenger = itemType.Equals("INFANT", StringComparison.OrdinalIgnoreCase);
                bool validInfantValidation = isInfantPassenger
                    ? itemValidation == "VALID" || itemValidation == "INVALID"
                    : itemValidation == "VALID";

                if (membershipValidation != string.Empty || !validInfantValidation)
                {
                    message = $"Basket Passenger {itemId} for Type {itemType} validation is not as expected \n Response -- \n {item}";
                    return false;
                }
            }
            else
            {
                if (fqtProgramme == "VS")
                {
                    if (membershipValidation != "VALID" || itemValidation != "VALID" || fqtValidityStatus != "Valid")
                    {
                        message = $"Basket Passenger {itemId} for Type {itemType} validation is not Valid \n Response -- \n {item}";
                        return false;
                    }
                }
                else
                {
                    if (!string.IsNullOrWhiteSpace(membershipValidation) || itemValidation != "VALID" || fqtValidityStatus != "Unchecked")
                    {
                        message = $"Basket Passenger {itemId} for Type {itemType} validation is not Valid \n Response -- \n {item}";
                        return false;
                    }
                }
            }
            return true;
        }

        //Basket Redemption points validation
        public static bool ValidateBasketMembershipPoints(dynamic offerPriceResult, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            var basketMembershipPoints = basketResult.SelectToken("...summary.membershipPoints");
            var offerPriceMembershipPoints = offerPriceResult.data.searchSummary.price.awardPoints.Value;

            if (basketMembershipPoints != offerPriceMembershipPoints)
            {
                message = $"The Basket Membership point {basketMembershipPoints} is not equal to offer price award points {offerPriceMembershipPoints} ";
                return false;
            }

            return true;
        }

        //Basket Redemption points validation
        public static bool ValidateOrderConfirmationMembershipPoints(JToken orderCreateResponse, JToken finalBasketResponse, out string message)
        {
            message = string.Empty;
            JToken basketMembershipPoints = finalBasketResponse.SelectToken("$.data.retrieveBasket.summary.membershipPoints");
            JToken orderCreateMembershipPoints = orderCreateResponse.SelectToken("$.data.bookCreateOrder.summary.membershipPoints");

            if (basketMembershipPoints == null || orderCreateMembershipPoints == null)
            {
                message = $"The final basket and Order Confirmation must both return membershipPoints. Basket={basketMembershipPoints}. Confirmation={orderCreateMembershipPoints}.";
                return false;
            }

            if (!basketMembershipPoints.ToString().Is(orderCreateMembershipPoints.ToString()))
            {
                message = $"The Basket Membership point {basketMembershipPoints} is not equal to Order Confirmation Membership points {orderCreateMembershipPoints} ";
                return false;
            }

            return true;
        }

        //Basket Redemption FC details validation
        public static bool ValidateBasketFlyingClubNumber(Dictionary<string, string> loyaltyDetails, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            var basketFCNumber = basketResult.SelectToken("...customerProfile.membershipDetails.fqtNumber").ToString();

            string fcNumber = loyaltyDetails["rowKey"];

            if (basketFCNumber != fcNumber)
            {
                message = $"The Basket FCnumber point {basketFCNumber} is not equal to Data storage Flying Club number {fcNumber} ";
                return false;
            }

            return true;
        }

        public static bool ValidateBasketFlyingClubTierCode(Dictionary<string, string> loyaltyDetails, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            var basketFQTTierCode = basketResult.SelectToken("...customerProfile.membershipDetails.fqtTierCode").ToString();

            string tierCode = loyaltyDetails["tier"];

            if (basketFQTTierCode != tierCode)
            {
                message = $"The Basket FQTTierCode {basketFQTTierCode} is not equal to Data storage TierCode {tierCode} ";
                return false;
            }

            return true;
        }

        public static bool ValidateBasketFlyingClubPoints(Dictionary<string, string> loyaltyDetails, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            var basketFQTPoints = basketResult.SelectToken("...customerProfile.membershipDetails.fqtPoints").ToString();

            var points = loyaltyDetails["points"].ToString();

            if (basketFQTPoints != points)
            {
                message = $"The Basket FCnumber points {basketFQTPoints} not equal to Data storage Flying Club points {points} ";
                return false;
            }

            return true;
        }

        public static bool ValidateBasketFlyingClubFirstName(Dictionary<string, string> loyaltyDetails, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            var basketFirstName = basketResult.SelectToken("...customerProfile.firstName").ToString();

            string firstName = loyaltyDetails["firstName"];

            if (basketFirstName != firstName)
            {
                message = $"The Basket FCnumber FirstName {basketFirstName} is not equal to Data storage Flying Club FirstName {firstName} ";
                return false;
            }

            return true;
        }

        public static bool ValidateBasketFlyingClubLastName(Dictionary<string, string> loyaltyDetails, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            var basketFirstName = basketResult.SelectToken("...customerProfile.firstName").ToString();

            string firstName = loyaltyDetails["firstName"];

            if (basketFirstName != firstName)
            {
                message = $"The Basket FCnumber FirstName {basketFirstName} is not equal to Data storage Flying Club FirstName {firstName} ";
                return false;
            }

            return true;
        }

        //Basket FC member points validation with basket login
        public static bool ValidateFcPointElibility(dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            long basketMembershipPoints = (long)basketResult.SelectToken("...summary.membershipPoints");

            long fqtPoints = (long)basketResult.SelectToken("...customerProfile.membershipDetails.fqtPoints");

            bool hasSufficientPoints = (bool)basketResult.SelectToken("...summary.hasSufficientPoints");

            if (fqtPoints < basketMembershipPoints && !hasSufficientPoints)
            {
                message = $"The customer's FQT points {fqtPoints} are not eligible because they are less than the required basket membership points {basketMembershipPoints} \n The customer needs to restart their search from the beginning.";
                return false;
            }

            return true;
        }

        public static bool ValidateFcPointsEligibility_NegativeTest_InsufficientPoints(dynamic basketResponse, out string message)
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

        public static bool ValidateBasketFlightPrice(dynamic offerPriceResult, dynamic basketResponse, out string message)
        {
            message = string.Empty;
            JObject basketResult = JObject.Parse(basketResponse.ToString());

            var basketFlightPrice = basketResult.SelectToken("...summary.flightPrice");

            if (basketFlightPrice != offerPriceResult.data.searchSummary.price.amountIncludingTax.Value)
            {
                message = $"The Basket flightPrice {basketFlightPrice} not equal to offer price amountIncludingTax {offerPriceResult.data.searchSummary.price.amountIncludingTax.Value} ";
                return false;
            }

            return true;
        }

        public static bool ValidateSeatForFirstPassengerOnly(dynamic basketResponse, out string message)
        {
            message = string.Empty;

            if (basketResponse.data.retrieveBasket.flightPassengerSeatMap[0].seatNumber.ToString() == string.Empty)
            {
                message = "Seat was not shown for the first passenger when it should have been.";
                return false;
            }

            if (basketResponse.data.retrieveBasket.flightPassengerSeatMap[1].seatNumber.ToString() != string.Empty)
            {
                message = "Seat for second passenger was returned when it was not selected and should not have been.";
                return false;
            }

            return true;
        }

        public static bool ValidateSeatsForAllPassengers(dynamic basketResponse, out string message)
        {
            message = string.Empty;

            var flightPassengerSeatMap = (JArray)basketResponse.data.retrieveBasket.flightPassengerSeatMap;

            foreach (var item in flightPassengerSeatMap)
            {
                var passengerId = item["passengerId"].ToString();
                if (item["seatNumber"].ToString() == string.Empty)
                {
                    message = $"Seat was not shown for the passenger id {passengerId}";
                    return false;
                }
            }

            return true;
        }

        public static bool FqtProgrammeGQLAssertion(string programCode, string airlineName, string programName, dynamic fqtProgrammeGQLResponse, out string message)
        {
            message = string.Empty;

            var FQTProgrammes = (JArray)fqtProgrammeGQLResponse.data.retrieveFQTProgrammes;

            foreach (var item in FQTProgrammes)
            {
                if (item["programCode"].ToString() == programCode && item["airlineName"].ToString() == airlineName && item["programName"].ToString() == programName)
                    return true;
            }
            message = $" Expected --------- \n\n \t ProgramCode: {programCode} \n \t airlineName:{airlineName} \n \t programName:{programName} \n\n Actual Response --------- \n \t {fqtProgrammeGQLResponse}";

            return false;
        }

        public static void ValidatePaymentRetreiveGQL(dynamic basketResponse, dynamic paymentRetrieveResponse)
        {
            JObject basketResponseResult = JsonConvert.DeserializeObject<JObject>(basketResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());
            JObject paymentRetrieveResponseResult = JsonConvert.DeserializeObject<JObject>(paymentRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            decimal basketPaymentAmount = basketResponseResult.SelectToken("data.retrieveBasket.summary.paymentAmount")?.Value<decimal>()
                ?? throw new Exception($"Unable to read basket paymentAmount from response: {basketResponseResult}");
            decimal paymentTotalAmount = paymentRetrieveResponseResult.SelectToken("data.orderPaymentReferenceId.totalAmount")?.Value<decimal>()
                ?? throw new Exception($"Unable to read payment totalAmount from response: {paymentRetrieveResponseResult}");
            decimal basketTotalSeatPrice = basketResponseResult.SelectToken("data.retrieveBasket.summary.totalSeatPrice")?.Value<decimal>() ?? 0.0m;

            decimal absoluteDifference = Math.Abs(basketPaymentAmount - paymentTotalAmount);
            decimal expectedPaymentExcludingSeats = basketPaymentAmount - basketTotalSeatPrice;
            decimal absoluteDifferenceExcludingSeats = Math.Abs(expectedPaymentExcludingSeats - paymentTotalAmount);
            bool matchesBasketPaymentAmount = absoluteDifference <= 0.01m;
            bool matchesBasketPaymentMinusSeatPrice = basketTotalSeatPrice > 0.0m && absoluteDifferenceExcludingSeats <= 0.01m;

            Assert.True(
                matchesBasketPaymentAmount || matchesBasketPaymentMinusSeatPrice,
                $"Payment amount mismatch. Basket paymentAmount={basketPaymentAmount}, Basket totalSeatPrice={basketTotalSeatPrice}, Payment totalAmount={paymentTotalAmount}, " +
                $"DiffToBasket={absoluteDifference}, DiffToBasketMinusSeats={absoluteDifferenceExcludingSeats}");
        }

        public static void GetPriGQL(dynamic paymentRetrieveResponse, TestConfig config)
        {
            var paymentRetrieveResponseResult = JsonConvert.DeserializeObject<dynamic>(paymentRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.NotNull(paymentRetrieveResponseResult.data.orderPaymentReferenceId.paymentSessions[0].paymentLink.Value);

            config.Book.Order.HppUrl = paymentRetrieveResponseResult.data.orderPaymentReferenceId.paymentSessions[0].paymentLink.Value;
            config.Book.Order.PRI = config.Book.Order.HppUrl.Split("paymentReferenceId=")[1].Split('&')[0];
        }

        [Obsolete("Marked as obsolete in favour of config driven approach. Please use 'GetPriGQL(dynamic paymentRetrieveResponse, TestConfig config)'.")]
        public static void GetPriGQL(dynamic paymentRetrieveResponse, out string hppUrl, out string pri)
        {
            var paymentRetrieveResponseResult = JsonConvert.DeserializeObject<dynamic>(paymentRetrieveResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.NotNull(paymentRetrieveResponseResult.data.orderPaymentReferenceId.paymentSessions[0].paymentLink.Value);

            hppUrl = paymentRetrieveResponseResult.data.orderPaymentReferenceId.paymentSessions[0].paymentLink.Value;
            pri = hppUrl.Split('=')[1];
            string orderNo = pri.Substring(0, 20);
        }

        public static bool CheckPnrCreated(dynamic orderGQLResponse, out string message)
        {
            message = string.Empty;

            var orderGQLResponseResult = JsonConvert.DeserializeObject<dynamic>(orderGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            if (string.IsNullOrEmpty(orderGQLResponseResult?.data?.bookCreateOrder?.summary?.slices[0]?.sectors[0]?.pnr?.ToString()))
            {
                message = $" Order confirmation Response doesnt contains PNR \n Graphql Order confirmation response -> {orderGQLResponseResult}";
                return false;
            }

            return true;
        }

        public static bool ValidateOrderConfirmationTotalPrice(JToken orderCreateResponse, JToken finalBasketResponse, out string message)
        {
            message = string.Empty;

            decimal? basketPaymentAmount = finalBasketResponse.SelectToken("$.data.retrieveBasket.summary.paymentAmount")?.Value<decimal?>();
            decimal? confirmationTotalPrice = orderCreateResponse.SelectToken("$.data.bookCreateOrder.summary.totalPrice")?.Value<decimal?>();

            if (!basketPaymentAmount.HasValue || !confirmationTotalPrice.HasValue)
            {
                message = $"The final basket paymentAmount and Order Confirmation totalPrice must both be returned. Basket={basketPaymentAmount}. Confirmation={confirmationTotalPrice}.";
                return false;
            }

            if (basketPaymentAmount.Value != confirmationTotalPrice.Value)
            {
                message = " Order confirmation amount price is not equal to basket amount price";
                return false;
            }

            return true;
        }

        public static bool ValidateConfirmationGraphqlResponse(string orderConfirmationRequest, dynamic orderConfirmationResponse, out string message, string fcMembershipNumber = "")
        {
            message = string.Empty;

            if (orderConfirmationResponse?.data?.bookCreateOrder == null && orderConfirmationResponse.errors.Count > 0)
            {
                message = (!fcMembershipNumber.Any() ? "" : $"\n FC account - {fcMembershipNumber}") + $"\nGraphql Order confirmation Request -> {orderConfirmationRequest} \n Graphql Order confirmation response -> {orderConfirmationResponse}";
                return false;
            }
            return true;
        }

        public static bool CheckFraudCheck(dynamic orderGQLResponse, out string message)
        {
            message = string.Empty;

            var orderGQLResponseResult = JsonConvert.DeserializeObject<dynamic>(orderGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var errorCode = orderGQLResponseResult.data.bookCreateOrder.errors[0].code.ToString();
            var errorMessage = orderGQLResponseResult.data.bookCreateOrder.errors[0].message.ToString();

            if (string.IsNullOrEmpty(errorCode) && string.IsNullOrEmpty(errorMessage) && errorCode != "2001" && errorMessage != "Card Presentation Required.")
            {
                message = $" Fraud check failed -- \n Expected error code : 2001 Expected error message = Card Presentation Required. \n Actual error code : {errorCode} \n Actual error message: {errorMessage} \n";
                return false;
            }

            return true;
        }

        public static bool CheckDeltaError(dynamic orderGQLResponse, out string message)
        {
            message = string.Empty;

            var orderGQLResponseResult = JsonConvert.DeserializeObject<dynamic>(orderGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var errorResponse = JsonConvert.DeserializeObject<dynamic>(orderGQLResponseResult.errors[0].extensions.response.ToString());

            var errorCode = orderGQLResponseResult.errors[0].extensions.code.ToString();
            var errorResponseCode = errorResponse[0].code.ToString();

            if (string.IsNullOrEmpty(errorCode) && string.IsNullOrEmpty(errorResponseCode))
            {
                return false;
            }

            return true;
        }

        public static bool SeatMapValidation(HttpResponseMessage retrieveSeatMapGQLResponse, FareFamilyType fareFamilyType, out string message)
        {
            message = string.Empty;

            var retrieveSeatMapGQLResult = JsonConvert.DeserializeObject<dynamic>(retrieveSeatMapGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            string operatingCarrier = null;

            if (retrieveSeatMapGQLResult?.data?.retrieveSeatMap != null)
            {
                var flightSegment = retrieveSeatMapGQLResult.data.retrieveSeatMap.flightSegment;
                if (flightSegment != null && flightSegment.operatingCarrier != null)
                {
                    operatingCarrier = flightSegment.operatingCarrier.Value;
                }
            }

            if (fareFamilyType is not FareFamilyType.ECON || fareFamilyType is FareFamilyType.ECON or FareFamilyType.ECONCLASSICFLEXIBLE && operatingCarrier is not "DL")
            {
                if (GraphQLHelper.SeatColumnsCount(retrieveSeatMapGQLResponse, "FREE") == 0
                    && GraphQLHelper.SeatColumnsCount(retrieveSeatMapGQLResponse, "GALLEY") == 0 && GraphQLHelper.SeatColumnsCount(retrieveSeatMapGQLResponse, "LAVATORY") == 0)
                {
                    message = $"For {fareFamilyType} \n No seat returned \n --------Response------\n \t {retrieveSeatMapGQLResponse.ReadResponseAsString()}";
                    return false;
                }
            }

            if (fareFamilyType is FareFamilyType.COMFORTPLUSPREMIUMECON or FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE || fareFamilyType is FareFamilyType.ECON or FareFamilyType.ECONCLASSICFLEXIBLE && operatingCarrier is not "DL")
            {
                if (GraphQLHelper.SeatColumnsCount(retrieveSeatMapGQLResponse, "PREFERRED_SEAT") == 0)
                {
                    message = $"For {fareFamilyType} \n No seat returned \n --------Response------\n \t {retrieveSeatMapGQLResponse.ReadResponseAsString()}";
                    return false;
                }
            }

            if (GraphQLHelper.SeatColumnsCount(retrieveSeatMapGQLResponse, "SEAT") == 0 && GraphQLHelper.SeatColumnsCount(retrieveSeatMapGQLResponse, "AISLE") == 0)
            {
                message = $"For {fareFamilyType} \n No seat returned \n --------Response------\n \t {retrieveSeatMapGQLResponse.ReadResponseAsString()}";
                return false;
            }

            return true;
        }

        public static void SeatSelectionValidation(HttpResponseMessage seatSelectionGQLResponse, int passengerId, string seatNumber)
        {
            dynamic selectSeatGQLResult = JsonConvert.DeserializeObject<dynamic>(seatSelectionGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            Assert.NotNull(selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber.Value);

            Assert.True(selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber == seatNumber, $" Selected Seat number dont match with assginged SeatNumber for passenger {passengerId} \n  Passenger Seat Number = {selectSeatGQLResult.data.selectSeat.flightPassengerSeatMap[passengerId].seatNumber} \n Selected Seat Number = {seatNumber}");

            Assert.True(ValidateColumnSeatSelected(seatSelectionGQLResponse, seatNumber, out string message), message);
        }

        public static bool ValidateSeatPricesAreAllZero(HttpResponseMessage retrieveSeatMapGQLResponse, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken retrieveSeatMapGQLResult = JsonConvert.DeserializeObject<JToken>(retrieveSeatMapGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var standardSeatAmounts = retrieveSeatMapGQLResult.SelectTokens("$..offers[?(@.productName == 'Standard Seat')].price.amount").Select(a => a.ToString()).ToArray();
            var exitSeatAmounts = retrieveSeatMapGQLResult.SelectTokens("$..offers[?(@.productName == 'Exit Seat')].price.amount").Select(a => a.ToString()).ToArray();
            var preferredSeatAmounts = retrieveSeatMapGQLResult.SelectTokens("$..offers[?(@.productName == 'Preferred Seat - Economy Cabin')].price.amount").Select(a => a.ToString()).ToArray();

            if (!standardSeatAmounts.Any())
            {
                result.Message.Add("No standard seats returned.");
            }

            if (!exitSeatAmounts.Any())
            {
                result.Message.Add("No exit seats returned.");
            }

            if (!preferredSeatAmounts.Any())
            {
                result.Message.Add("No preferred seats returned.");
            }

            foreach (var amount in standardSeatAmounts)
            {
                if (amount != "0")
                {
                    result.Message.Add($"Seat amount is '{amount}', when it should be '0'.");
                }
            }

            foreach (var amount in exitSeatAmounts)
            {
                if (amount != "0")
                {
                    result.Message.Add($"Seat amount is '{amount}', when it should be '0'.");
                }
            }

            foreach (var amount in preferredSeatAmounts)
            {
                if (amount != "0")
                {
                    result.Message.Add($"Seat amount is '{amount}', when it should be '0'.");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateSeatPriceCurrency(HttpResponseMessage retrieveSeatMapGQLResponse, string currency, out ValidationResultHelper result)
        {
            result = new ValidationResultHelper();

            JToken retrieveSeatMapGQLResult = JsonConvert.DeserializeObject<JToken>(retrieveSeatMapGQLResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            var seatCurrencies = retrieveSeatMapGQLResult.SelectTokens("$..offers[*].price.currency").Select(a => a.ToString()).ToArray();

            if (!seatCurrencies.Any())
            {
                result.Message.Add("No seats returned.");
            }

            foreach (var seatCurrency in seatCurrencies)
            {
                if (seatCurrency != currency)
                {
                    result.Message.Add($"\nSeat currency is '{seatCurrency}', when it should be '{currency}'.");
                }
            }

            return result.IsValid;
        }

        public static bool ValidateColumnSeatSelected(HttpResponseMessage seatSelectionGQLResponse, string seatNumber, out string message)
        {
            message = string.Empty;

            JObject retrieveSeatMapResult = JObject.Parse(seatSelectionGQLResponse.ReadResponseAsString());

            dynamic seatOccupied = retrieveSeatMapResult.SelectToken($"$.data.selectSeat.seatMap.rows[*].columns[?(@.type == 'SEAT' && @.availability == 'OCCUPIED' && @.seatNumber == '{seatNumber}')]");

            if (seatOccupied.seatNumber.Value == seatNumber && seatOccupied.availability.Value == "OCCUPIED") return true;

            message = $" Assertion failed due to no SELECTED seat for SeatNumber ${seatNumber}";
            return false;
        }

        public static bool ValidateColumnSeatAmountCurrencySelected(HttpResponseMessage seatSelectionGQLResponse, string seatNumber, double amount, string currency, out string message)
        {
            message = string.Empty;

            JObject retrieveSeatMapResult = JObject.Parse(seatSelectionGQLResponse.ReadResponseAsString());

            dynamic seatOccupied = retrieveSeatMapResult.SelectToken($"$.data.selectSeat.flightPassengerSeatMap[?(@.seatNumber == '{seatNumber}')]");

            if (seatOccupied.seatNumber.Value == seatNumber && seatOccupied.price.amount.Value == amount && seatOccupied.price.currency.Value == currency) return true;

            message = $" Assertion failed due to Amount and Currency dont match SELECTED seat for SeatNumber ${seatNumber}";
            return false;
        }

        public static void ValidateSectorIdInBothSegment(HttpResponseMessage retrieveBasketGQLResponse)
        {
            TestConfig _testConfig = new TestConfig();

            JObject retrieveBasketGQLResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());

            var sectorInbound = retrieveBasketGQLResult.SelectTokens("$.data.retrieveBasket.summary.slices[1].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            var sectorOutbound = retrieveBasketGQLResult.SelectTokens("$.data.retrieveBasket.summary.slices[0].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            foreach (var i in sectorInbound)
            {
                foreach (var j in sectorOutbound)
                    Assert.False(i == j, $"both SectorID are same \n SectorInbound = {i}\n SectorOutbound = {j}");
            }
            
            _testConfig.Book.Seat.SectorInbound = sectorInbound;
            _testConfig.Book.Seat.SectorOutbound = sectorOutbound;
        }

        public static void ValidateSectorIdInBothSegment(HttpResponseMessage retrieveBasketGQLResponse, TestConfig _testConfig)
        {
            JObject retrieveBasketGQLResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());

            var sectorInbound = retrieveBasketGQLResult.SelectTokens("$.data.retrieveBasket.summary.slices[1].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            var sectorOutbound = retrieveBasketGQLResult.SelectTokens("$.data.retrieveBasket.summary.slices[0].sectors[*].sectorId").Select(x => x.ToString()).ToArray();

            foreach (var i in sectorInbound)
            {
                foreach (var j in sectorOutbound)
                    Assert.False(i == j, $"both SectorID are same \n SectorInbound = {i}\n SectorOutbound = {j}");
            }

            _testConfig.Book.Seat.SectorInbound = sectorInbound;
            _testConfig.Book.Seat.SectorOutbound = sectorOutbound;
        }

        public static bool CheckProximity(HttpResponseMessage seatMap, string productName, CabinName cabinName, out string message)
        {
            message = string.Empty;

            JObject retrieveSeatMapResult = JObject.Parse(seatMap.ReadResponseAsString());

            //Json path to find the Availibility seat with respect cabin fare product and product name
            IEnumerable<JToken> freeSeat = retrieveSeatMapResult.SelectTokens($".data.retrieveSeatMap.seatMap.rows[*].columns[?(@.type == 'SEAT' && @.availability == 'FREE' && @.offers[?(@.productName == '{productName}' && @.fareProduct == '{cabinName}')])]");

            if (freeSeat.Any() != false)
            {
                message = $"Seats were returned for {productName} and {cabinName} when an infant or child was added as a Passenger";
                return false;
            }
            return true;
        }

        public static bool SeatMapLegendValidation(HttpResponseMessage retrieveSeatMapGQLResponse, FareFamilyType fareFamilyType, out string message)
        {
            message = string.Empty;

            JObject retrieveSeatMapResult = JObject.Parse(retrieveSeatMapGQLResponse.ReadResponseAsString());

            if (fareFamilyType is FareFamilyType.ECON or FareFamilyType.ECONCLASSICFLEXIBLE)
            {
                if (!LegendFareSummaryValidation(retrieveSeatMapResult, "PREFFERED", "Preferred Seat - Economy Cabin", CabinName.VSCL, out string errorMessage) == true)
                {
                    message = errorMessage;
                    return false;
                }
                if (!LegendFareSummaryValidation(retrieveSeatMapResult, "EXIT", "Exit Seat", CabinName.VSCL, out errorMessage) == true)
                {
                    message = errorMessage;
                    return false;
                }
                if (!LegendFareSummaryValidation(retrieveSeatMapResult, "STANDARD", "Standard Seat", CabinName.VSCL, out errorMessage) == true)
                {
                    message = errorMessage;
                    return false;
                }
            }
            if (fareFamilyType is FareFamilyType.ECONDELIGHTFLEXIBLE or FareFamilyType.ECONOMYDELIGHT)
            {
                if (!LegendFareSummaryValidation(retrieveSeatMapResult, "DELIGHT", "Economy Delight seat", CabinName.VSDT, out string errorMessage) == true)
                {
                    message = errorMessage;
                    return false;
                }
            }
            if (fareFamilyType is FareFamilyType.COMFORTPLUSPREMIUMECON or FareFamilyType.COMFORTPLUSPREMIUMFLEXIBLE)
            {
                if (!LegendFareSummaryValidation(retrieveSeatMapResult, "PREFFERED", "Premium Economy Cabin Seat", CabinName.VSPE, out string errorMessage) == true)
                {
                    message = errorMessage;
                    return false;
                }
                if (!LegendFareSummaryValidation(retrieveSeatMapResult, "STANDARD", "Premium Economy seat", CabinName.VSPE, out errorMessage) == true)
                {
                    message = errorMessage;
                    return false;
                }
            }
            if (fareFamilyType is FareFamilyType.BUSFIRST or FareFamilyType.BUSFIRSTFLEXIBLE)
            {
                if (!LegendFareSummaryValidation(retrieveSeatMapResult, "STANDARD", "Upper Class seat", CabinName.VSUP, out string errorMessage) == true)
                {
                    message = errorMessage;
                    return false;
                }
            }

            return true;
        }

        public static bool LegendFareSummaryValidation(JObject retrieveSeatMapResult, string seatType, string productName, CabinName cabinName, out string message)
        {
            message = string.Empty;

            //Fetch the SeatType legend
            dynamic fareSummaryLegend = retrieveSeatMapResult.SelectToken($"...seatMap.fareSummary[?(@.seatType=='{seatType}')]");
            var amount = retrieveSeatMapResult.SelectTokens($"...seatMap.rows[*].columns[?(@.type == 'SEAT' && @.offers[?(@.productName == '{productName}' && @.fareProduct == '{cabinName}')])].offers[*].price.amount").Select(x => x.ToString().Parse<decimal>());
            var currency = retrieveSeatMapResult.SelectTokens($"...seatMap.rows[*].columns[?(@.type == 'SEAT' && @.offers[?(@.productName == '{productName}' && @.fareProduct == '{cabinName}')])].offers[*].price.currency").Select(x => x.ToString());

            var blockedCheck = retrieveSeatMapResult.SelectTokens($"$...seatMap.rows[*].columns[?(@.type == 'SEAT' && @.availability!='BLOCKED' && @.offers[?(@.productName == '{productName}')])]").Count();

            if (blockedCheck > 0)
            {
                decimal minAmount = 0;
                decimal maxAmount = 0;

                if (amount != null)
                {
                    minAmount = amount.Min();
                    maxAmount = amount.Max();

                    //negative validation on miminum aount
                    if (fareSummaryLegend.min.Value == null || fareSummaryLegend.max.Value == null)
                    {
                        message = $"This min and value is null due to Legend summary issue \n\n Legend ----------: {fareSummaryLegend}";
                        return false;
                    }
                    //negative validation on miminum aount
                    if (minAmount != decimal.Parse(fareSummaryLegend.min.Value))
                    {
                        message = $"This failed due to response min amount {minAmount} with {fareSummaryLegend.min.Value} Legend summary issue \n\n Legend ----------: {fareSummaryLegend}";
                        return false;
                    }
                    //check the Currency is same in all response
                    if (currency.Distinct().Count() != 1 && currency.First() != fareSummaryLegend.currency.Value)
                    {
                        message = $"This failed due to Legend currency is not same \n\n Legend ----------: {fareSummaryLegend}";
                        return false;
                    }
                }
            }

            return true;
        }

        //order document
        public static bool ValidatePnrOrderDocument(string pnr, out string message)
        {
            message = string.Empty;
            if (pnr != new BookOrderDocumentHelper().GetPNRDetails(pnr))
            {
                return false;
                message = $" Confirmation response PNR not created in Order document";
            }
            return true;
        }

        public static bool ValidatePnrInsuranceOrderDocument(TestConfig _testConfig, out string message)
        {
            message = string.Empty;
            OrderDocument order = new BookOrderDocumentHelper().GetPNRInsuranceAddedDetails(_testConfig.Book.Order.PNR);

            if (order == null && !order.IsInsuranceAdded && order.InsurancePurchaseDetails?.InsurancePurchaseStatus?.Code != "SUCCESS" && order.InsurancePurchaseDetails?.InsurancePurchaseAmount != _testConfig.Book.Cost.InsurancePrice)
            {
                message = "Insurance purchase was not successful or the response was not created in the Order document."; 
                return false;
            }
            return true;
        }

        public static bool ValidatePnrRejectedInsuranceOrderDocument(TestConfig _testConfig, out string message)
        {
            message = string.Empty;
            OrderDocument order = new BookOrderDocumentHelper().GetPNRRejectedInsuranceAddedDetails(_testConfig.Book.Order.PNR);

            if (order == null && order.IsInsuranceAdded)
            {
                message = "Insurance flag  was not false in the order document.";
                return false;
            }
            return true;
        }

        public static bool ValidateGQLOriginDestination(dynamic searchRequest, HttpResponseMessage retrieveSummaryResponse, out string message)
        {
            message = string.Empty;

            JToken flightSearchRequest = JsonConvert.DeserializeObject<JToken>(searchRequest);

            JObject retrieveSummaryResult = JObject.Parse(retrieveSummaryResponse.ReadResponseAsString());

            var requestedOrigin = flightSearchRequest.SelectTokens("$.variables.request.flightSearchRequest.searchOriginDestinations[*].origin").Select(x => x.ToString()).ToArray();

            var requestedDestination = flightSearchRequest.SelectTokens("$.variables.request.flightSearchRequest.searchOriginDestinations[*].destination").Select(x => x.ToString()).ToArray();

            var responseOrigin = retrieveSummaryResult.SelectTokens("$...flights[*].origin.code").Select(x => x.ToString()).ToArray();

            var responseDestination = retrieveSummaryResult.SelectTokens("$...flights[*].destination.code").Select(x => x.ToString()).ToArray();

            if (!requestedOrigin.SequenceEqual(responseOrigin))
            {
                message = $"Origin data missmatch in View itinerary \n Search requested flight origin : {requestedOrigin} \n View itinerary Response flight origin : {responseOrigin} ";

                return false;
            }

            if (!requestedDestination.SequenceEqual(responseDestination))
            {
                message = $"Destination data missmatch in View itinerary \n Search requested flight destination {requestedDestination} \n View itinerary Response flight destination {responseDestination}";

                return false;
            }
            return true;
        }

        public static bool ValidatePosInOrderConfirmation(HttpResponseMessage orderCreateGQLResponse, HttpResponseMessage getOfferResponse, out string message)
        {
            message = string.Empty;

            JObject orderConfirmationResult = JObject.Parse(orderCreateGQLResponse.ReadResponseAsString());
            JObject getOfferResult = JObject.Parse(getOfferResponse.ReadResponseAsString());

            var offerPOSCountryCode = getOfferResult.SelectToken(".pos.countryCode").ToString();
            var offerPOSCityCode = getOfferResult.SelectToken(".pos.cityCode").ToString();
            var orderPOSCountryCode = orderConfirmationResult.SelectToken(".data.bookCreateOrder.pointOfSale.countryCode").ToString();
            var orderPOSCityCode = orderConfirmationResult.SelectToken(".data.bookCreateOrder.pointOfSale.cityCode").ToString();

            if (offerPOSCountryCode != orderPOSCountryCode)
            {
                message = $"Point of Sale validation failed \n Get Offer response Country code : {offerPOSCountryCode} \n Get Offer response Country code : {orderPOSCountryCode}";

                return false;
            }

            if (offerPOSCityCode != orderPOSCityCode)
            {
                message = $"Point of Sale validation failed \n Get Offer response Country code : {offerPOSCityCode} \n Get Offer response Country code : {orderPOSCityCode}";

                return false;
            }

            return true;
        }

        //Pay with points validations
        public static bool ValidateCurrencyPayToCurrencyConverter(HttpResponseMessage pointToCurrencyConverterResponse, HttpResponseMessage retrieveBasketGQLResponse, out string message)
        {
            message = string.Empty;

            JObject PointToCurrencyConverterResult = JObject.Parse(pointToCurrencyConverterResponse.ReadResponseAsString());
            JObject retrieveBasketResult = JObject.Parse(retrieveBasketGQLResponse.ReadResponseAsString());

            var currencies = PointToCurrencyConverterResult.SelectTokens("$.data.PointsToCurrencyConverterQuery[*].currencyCode").Select(x => x.ToString()).ToArray();
            var basketCurrency = retrieveBasketResult.SelectToken("$.data.retrieveBasket.summary.priceCurrency")?.ToString();

            foreach (var currency in currencies)
            {
                if (string.IsNullOrEmpty(currency))
                {
                    message = $"PointToCurrencyConverterResult Currency is null: {currency}";
                    return false;
                }

                if (basketCurrency != currency)
                {
                    message = $"PointToCurrencyConverterResult Currency: {currency} \n is not equal to Basket Currency: {basketCurrency}";
                    return false;
                }
            }
            return true;
        }

        public static bool ValidatePayWithPointsList(HttpResponseMessage pointToCurrencyConverterResponse, int minimumRequiredPoints, int pointsArrayListCount, out string message)
        {
            message = string.Empty;

            JObject PointToCurrencyConverterResult = JObject.Parse(pointToCurrencyConverterResponse.ReadResponseAsString());

            var pointsArray = PointToCurrencyConverterResult.SelectTokens("$.data.PointsToCurrencyConverterQuery[*]").Select(x => x.ToString()).ToArray();

            int actualCount = pointsArray.Length;

            // Set minimum points (optional, if needed later)
            int minimumPoints = minimumRequiredPoints;

            if (actualCount != pointsArrayListCount)
            {
                message = $"Mismatch in Points Array Count: Actual count: {actualCount}, Expected count: {pointsArrayListCount}.";
                return false;
            }

            foreach (var pointItem in pointsArray)
            {
                var pointData = JsonConvert.DeserializeObject<dynamic>(pointItem);

                //get points, currency and amout from the list
                if (!decimal.TryParse((string)pointData.amount, out decimal amount))
                {
                    message = "Amount is not a valid decimal value.";
                    return false;
                }

                string currencyCode = (string)pointData.currencyCode;
                string loyaltyPoints = (string)pointData.loyaltyPoints;

                // Calculate expected points and expected amount
                const int pointsPerTier = 3000;
                const decimal amountPerTier = 16.5m;
                int expectedPoints = minimumPoints * actualCount;
                decimal expectedAmount = expectedPoints / pointsPerTier * amountPerTier;

                decimal tolerance = 0.01m;

                // Safe parsing of loyaltyPoints
                if (!int.TryParse(loyaltyPoints, out int parsedPoints))
                {
                    message = $"Loyalty points '{loyaltyPoints}' could not be parsed as an integer.";
                    return false;
                }

                // Validate points and amount with tolerance
                if (expectedPoints != parsedPoints || Math.Abs(expectedAmount - amount) > tolerance)
                {
                    message = $"Validation failed: Expected Points {expectedPoints} (actual: {parsedPoints}), " +
                              $"Expected Amount {expectedAmount} (actual: {amount}), with a tolerance of {tolerance}.";
                    return false;
                }

                actualCount--;
            }
            return true;
        }

        public static bool ValidatePointsAndAmount(HttpResponseMessage gQLResponse, string expectedPoints, out string message)
        {
            message = string.Empty;

            JObject result = JObject.Parse(gQLResponse.ReadResponseAsString());

            dynamic selectedPayWithPointsOption = result.SelectToken($"$.data..summary.selectedPayWithPointsOption");

            string pointsSelected = selectedPayWithPointsOption.loyaltyPoints?.Value ?? string.Empty;
            decimal? amount = (decimal?)selectedPayWithPointsOption.amount;
            string currency = selectedPayWithPointsOption.currencyCode?.Value ?? string.Empty;

            if (pointsSelected.IsNullOrEmpty() || currency.IsNullOrEmpty())
            {
                message = $"Selected pay with points value is null or empty: {selectedPayWithPointsOption} \nResponse: {result}";
                return false;
            }

            // Safely parse the loyalty points
            if (!int.TryParse(pointsSelected, out int parsedPoints))
            {
                message = $"Failed to parse loyalty points: {pointsSelected} \nResponse: {result}";
                return false;
            }

            // Calculate expected amount
            const int pointsPerPax = 3000;
            const decimal amountPerTier = 16.5m;
            decimal expectedAmount = parsedPoints / pointsPerPax * amountPerTier;
            decimal tolerance = 0.01m;

            if (amount == null || Math.Abs(expectedAmount - (decimal)amount) > tolerance)
            {
                message = $"Points selected amount is not correct :{amount}  \nResponse points selected is : {selectedPayWithPointsOption} \nResponse: {result}";
                return false;
            }

            var priceCurrency = result.SelectToken($"$.data..summary.priceCurrency")?.ToString();
            if (priceCurrency != currency)
            {
                message = $"Currency code is not correct: Expected Currency: {priceCurrency}  \nResponse Currency is : {currency} \nResponse: {result}";
                return false;
            }

            return true;
        }

        public static bool ValidateInsuranceCosts(JToken basketResponseBeforeInsurance, JToken basketResponseAfterInsurance, bool isInsuranceAccepted, out ValidationResultHelper result, JToken insuranceQuoteResponse = null)
        {
            result = new ValidationResultHelper();

            var isInsuranceAddedBefore = basketResponseBeforeInsurance.SelectToken("data.retrieveBasket.isInsuranceAdded").Value<bool?>();
            var insurancePriceBefore = basketResponseBeforeInsurance.SelectToken("data.retrieveBasket.summary.insurancePrice").Value<double>();
            var minDepositBefore = basketResponseBeforeInsurance.SelectToken("data.retrieveBasket.summary.minDeposit").Value<double>();
            var totalPriceWithoutDiscountBefore = basketResponseBeforeInsurance.SelectToken("data.retrieveBasket.summary.totalPriceWithoutDiscount").Value<double>();
            var totalPriceBefore = basketResponseBeforeInsurance.SelectToken("data.retrieveBasket.summary.totalPrice").Value<double>();
            var totalSeatPriceBefore = basketResponseBeforeInsurance.SelectToken("data.retrieveBasket.summary.totalSeatPrice").Value<double>();
            var paymentAmountBefore = basketResponseBeforeInsurance.SelectToken("data.retrieveBasket.summary.paymentAmount").Value<double>();

            var isInsuranceAddedAfter = basketResponseAfterInsurance.SelectToken("data.retrieveBasket.isInsuranceAdded").Value<bool>();
            var insurancePriceAfter = basketResponseAfterInsurance.SelectToken("data.retrieveBasket.summary.insurancePrice").Value<double>();
            var minDepositAfter = basketResponseAfterInsurance.SelectToken("data.retrieveBasket.summary.minDeposit").Value<double>();
            var totalPriceWithoutDiscountAfter = basketResponseAfterInsurance.SelectToken("data.retrieveBasket.summary.totalPriceWithoutDiscount").Value<double>();
            var totalPriceAfter = basketResponseAfterInsurance.SelectToken("data.retrieveBasket.summary.totalPrice").Value<double>();
            var totalSeatPriceAfter = basketResponseAfterInsurance.SelectToken("data.retrieveBasket.summary.totalSeatPrice").Value<double>();
            var pricePerPersonAfter = basketResponseAfterInsurance.SelectToken("data.retrieveBasket.summary.pricePerPerson").Value<double>();
            var paymentAmountAfter = basketResponseAfterInsurance.SelectToken("data.retrieveBasket.summary.paymentAmount").Value<double>();

            if (isInsuranceAddedBefore != null)
                result.Message.Add($"isInsuranceAdded is '{isInsuranceAddedBefore}' when it should be 'null' in before insurance response.");

            if (insurancePriceBefore != 0)
                result.Message.Add($"insurancePrice is '{insurancePriceBefore}' when it should be '0' in before insurance response.");

            if (isInsuranceAddedAfter != isInsuranceAccepted)
                result.Message.Add($"isInsuranceAdded is '{isInsuranceAddedAfter}' when it should be '{isInsuranceAccepted}' in after insurance response.");

            if (isInsuranceAccepted)
            {
                if (insurancePriceAfter <= 0)
                    result.Message.Add($"insurancePrice is '{insurancePriceAfter}' when it should be more than '0' in after insurance response.");

                var insuranceQuotePrice = insuranceQuoteResponse.SelectToken("data.InsuranceQuote[0].products[0].totalPrice").Value<double>();
                if (insuranceQuotePrice != insurancePriceAfter)
                    result.Message.Add($"insurancePrice '{insurancePriceAfter}' does not match the insurance quote '{insuranceQuotePrice}'.");
            }
            else
            {
                if (insurancePriceAfter != 0)
                    result.Message.Add($"insurancePrice is '{insurancePriceAfter}' when it should be '0' in after insurance response.");
            }

            if (minDepositBefore + insurancePriceAfter != minDepositAfter)
                result.Message.Add($"minDeposit in the after insurance response should equal minDeposit from before insurance response + insurancePrice.");

            if (totalPriceWithoutDiscountBefore + insurancePriceAfter != totalPriceWithoutDiscountAfter)
                result.Message.Add($"totalPriceWithoutDiscount in the after insurance response should equal totalPriceWithoutDiscount from before insurance response + insurancePrice.");

            if (totalPriceBefore + insurancePriceAfter != totalPriceAfter)
                result.Message.Add($"totalPrice in the after insurance response should equal totalPrice from before insurance response + insurancePrice.");

            if (totalSeatPriceBefore != totalSeatPriceAfter)
                result.Message.Add($"totalSeatPrice should be the same in both before and after insurance responses.");

            var pricePerPerson = Math.Round((totalPriceBefore + insurancePriceAfter) / 3, 2);
            if (pricePerPerson != pricePerPersonAfter)
                result.Message.Add($"pricePerPerson in the after insurance response should equal pricePerPerson from before insurance response + insurancePrice (divided by number of persons, excluding infants).");

            if (paymentAmountBefore + insurancePriceAfter != paymentAmountAfter)
                result.Message.Add($"paymentAmount in the after insurance response should equal paymentAmount from before insurance response + insurancePrice.");

            if (result.Message.Count != 0)
                result.Message.Add($"\nResponse before insurance:\n{basketResponseBeforeInsurance}\nResponse after insurance:\n{basketResponseAfterInsurance}");

            return result.IsValid;
        }

        public static bool CheckCoBrandEligibiltyStatusForPayWithPoints(HttpResponseMessage basketResponse, decimal amountToPay)
        {
            // Deserialize the response
            var basketResult = JsonConvert.DeserializeObject<JToken>(basketResponse.Content.ReadAsStringAsync().GetAwaiter().GetResult());

            bool isCoBrandEligible = basketResult.SelectToken("$..isCoBrandEligible").Value<bool>();

            // Check if the basket is greater than 20K
            bool isBasketGreaterThan20K = amountToPay > 20000;

            // Return eligibility based on eligibility status, and basket amount condition
            if (isBasketGreaterThan20K && !isCoBrandEligible)
            {
                return false;
            }

            // Return eligibility based on the amount and the co-brand eligibility
            return amountToPay <= 20000 && isCoBrandEligible;
        }
    }
}
