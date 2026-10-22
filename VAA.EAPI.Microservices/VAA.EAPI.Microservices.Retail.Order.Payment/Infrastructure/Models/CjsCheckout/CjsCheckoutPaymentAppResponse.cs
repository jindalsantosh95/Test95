using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Models.CjsCheckout
{
    public sealed record CjsCheckoutPaymentAppResponse(
        [property: JsonProperty("type")] string? Type,
        [property: JsonProperty("paymentSessions")] IReadOnlyList<CjsCheckoutPaymentSession>? PaymentSessions,
        [property: JsonProperty("milesPayment")] CjsCheckoutMilesPayment? MilesPayment,
        [property: JsonProperty("billingInformation")] CjsCheckoutBillingInformation? BillingInformation,
        [property: JsonProperty("payPalSession")] CjsCheckoutPayPalSession? PayPalSession,
        [property: JsonProperty("paymentDetails")] CjsCheckoutPaymentDetails? PaymentDetails,
        [property: JsonProperty("payPalPaymentDetails")] CjsCheckoutPayPalPaymentDetails? PayPalPaymentDetails,
        [property: JsonProperty("voucherPayment")] CjsCheckoutVoucherPayment? VoucherPayment,
        [property: JsonProperty("totalAmountOnSessions")] decimal? TotalAmountOnSessions,
        [property: JsonProperty("minDeposit")] decimal? MinDeposit,
        [property: JsonProperty("displayDeposit")] bool? DisplayDeposit,
        [property: JsonProperty("totalAmount")] decimal? TotalAmount,
        [property: JsonProperty("amountToPay")] decimal? AmountToPay,
        [property: JsonProperty("hasDirectDebit")] bool? HasDirectDebit,
        [property: JsonProperty("cpdPayment")] CjsCheckoutCpdPayment? CpdPayment,
        [property: JsonProperty("apexxPayment")] JToken? ApexxPayment,
        [property: JsonProperty("directDebitLink")] string? DirectDebitLink,
        [property: JsonProperty("link")] string? Link,
        [property: JsonProperty("confirmationLink")] string? ConfirmationLink,
        [property: JsonProperty("summary")] JObject? Summary,
        [property: JsonProperty("resourceType")] string? ResourceType);

    public sealed record CjsCheckoutPaymentSession(
        [property: JsonProperty("amountToPay")] decimal? AmountToPay,
        [property: JsonProperty("status")] string? Status,
        [property: JsonProperty("paymentAmount")] CjsCheckoutPaymentAmountLink? PaymentAmount,
        [property: JsonProperty("paymentLink")] string? PaymentLink,
        [property: JsonProperty("removeLink")] string? RemoveLink);

    public sealed record CjsCheckoutPaymentAmountLink(
        [property: JsonProperty("amountToPay")] decimal? AmountToPay,
        [property: JsonProperty("link")] string? Link);

    public sealed record CjsCheckoutMilesPayment(
        [property: JsonProperty("miles")] JToken? Miles,
        [property: JsonProperty("amount")] decimal? Amount,
        [property: JsonProperty("increments")] JToken? Increments,
        [property: JsonProperty("balance")] JToken? Balance,
        [property: JsonProperty("available")] JToken? Available,
        [property: JsonProperty("exchangeRate")] JToken? ExchangeRate,
        [property: JsonProperty("milesDetails")] CjsCheckoutMilesDetails? MilesDetails,
        [property: JsonProperty("link")] string? Link);

    public sealed record CjsCheckoutMilesDetails(
        [property: JsonProperty("membershipNumber")] string? MembershipNumber,
        [property: JsonProperty("emailAddress")] string? EmailAddress,
        [property: JsonProperty("password")] string? Password,
        [property: JsonProperty("link")] string? Link);

    public sealed record CjsCheckoutBillingInformation(
        [property: JsonProperty("firstName")] string? FirstName,
        [property: JsonProperty("lastName")] string? LastName,
        [property: JsonProperty("streetLineOne")] string? StreetLineOne,
        [property: JsonProperty("streetLineTwo")] string? StreetLineTwo,
        [property: JsonProperty("townCity")] string? TownCity,
        [property: JsonProperty("postCode")] string? PostCode,
        [property: JsonProperty("emailAddress")] string? EmailAddress,
        [property: JsonProperty("country")] string? Country,
        [property: JsonProperty("link")] string? Link);

    public sealed record CjsCheckoutPayPalSession(
        [property: JsonProperty("paymentURI")] string? PaymentUri,
        [property: JsonProperty("orderId")] string? OrderId,
        [property: JsonProperty("authorizationFailed")] JToken? AuthorizationFailed,
        [property: JsonProperty("payment")] CjsCheckoutPayPalSessionPayment? Payment);

    public sealed record CjsCheckoutPayPalSessionPayment(
        [property: JsonProperty("amount")] JToken? Amount,
        [property: JsonProperty("approveLink")] string? ApproveLink,
        [property: JsonProperty("resetLink")] string? ResetLink,
        [property: JsonProperty("removeLink")] string? RemoveLink);

    public sealed record CjsCheckoutPaymentDetails(
        [property: JsonProperty("returnUrl")] string? ReturnUrl,
        [property: JsonProperty("expiryUrl")] string? ExpiryUrl,
        [property: JsonProperty("link")] string? Link);

    public sealed record CjsCheckoutPayPalPaymentDetails(
        [property: JsonProperty("payPalClientUrl")] string? PayPalClientUrl,
        [property: JsonProperty("link")] string? Link);

    public sealed record CjsCheckoutVoucherPayment(
        [property: JsonProperty("code")] string? Code,
        [property: JsonProperty("link")] string? Link);

    public sealed record CjsCheckoutCpdPayment(
        [property: JsonProperty("paymentAmount")] CjsCheckoutPaymentAmountLink? PaymentAmount,
        [property: JsonProperty("link")] string? Link);
}

