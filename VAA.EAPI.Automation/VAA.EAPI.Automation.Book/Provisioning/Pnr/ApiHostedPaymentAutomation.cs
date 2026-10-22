using System.Collections.Specialized;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers.Enums;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Automation.Core.TestDataConfig.PaymentCards;

namespace VAA.EAPI.Automation.Book.Provisioning.Pnr
{
    public static class ApiHostedPaymentAutomation
    {
        private const string DefaultHppConfigUrl =
            "https://enterprisepaymentshppapi-si.vs01.vs.air4.com/UI/getConfigs";

        public static async Task SubmitCardAsync(TestConfig config)
        {
            string environment = Config.Environment?.Trim() ?? string.Empty;
            if (environment.IsNullOrWhiteSpace() || environment.Is("prod"))
            {
                throw new InvalidOperationException(
                    "API card payment is restricted to an explicitly configured non-production environment.");
            }

            string paymentReferenceId = config.Book.Order.PRI;
            if (paymentReferenceId.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException("API card payment requires a payment reference ID.");
            }

            JObject hppConfig = await GetJsonAsync(
                ConfigReader.Get("HppApiConfigUrl", DefaultHppConfigUrl));
            JObject apiConfig = RequireObject(hppConfig, "configs.api");

            JObject formsOfPaymentsConfig = RequireObject(apiConfig, "formsOfPayments");
            JObject formsOfPayments = await PostJsonAsync(
                RequireString(formsOfPaymentsConfig, "url"),
                new JObject(),
                BuildHeaders(formsOfPaymentsConfig, paymentReferenceId));

            string transactionId = formsOfPayments
                .RequireToken("cpdInitFormsOfPayment.transaction.transactionId")
                .ToString();

            JObject paymentDetailsConfig = RequireObject(apiConfig, "postPaymentDetails");
            JObject paymentDetailsResponse = await PostJsonAsync(
                RequireString(paymentDetailsConfig, "url"),
                BuildPaymentDetailsPayload(config.Book.Payment.CardDetails, paymentDetailsConfig),
                BuildHeaders(paymentDetailsConfig, paymentReferenceId, transactionId));

            string authenticatedPaymentReferenceId = paymentDetailsResponse
                .RequireToken("paymentReferenceId")
                .ToString();

            string psd2ConfigUrl = ResolvePsd2ConfigUrl(hppConfig);
            JObject psd2Config = await GetJsonAsync(psd2ConfigUrl);

            JObject initializationConfig = RequireObject(psd2Config, "endpoints.initialization");
            JObject initializationResponse = await PostJsonAsync(
                RequireString(initializationConfig, "url"),
                new JObject(),
                BuildHeaders(initializationConfig, authenticatedPaymentReferenceId));

            string serverTransactionId = initializationResponse
                .RequireToken("serverTransactionId")
                .ToString();

            JObject authenticationConfig = RequireObject(psd2Config, "endpoints.authentication");
            JObject authenticationResponse = await PostJsonAsync(
                RequireString(authenticationConfig, "url"),
                BuildAuthenticationPayload(serverTransactionId),
                BuildHeaders(authenticationConfig, authenticatedPaymentReferenceId));

            string confirmedPaymentReferenceId = authenticationResponse
                .RequireToken("formOfPayment.paymentReferenceId")
                .ToString();
            if (!confirmedPaymentReferenceId.Is(authenticatedPaymentReferenceId))
            {
                throw new InvalidOperationException(
                    "Payment authentication returned a different payment reference ID.");
            }
        }

        private static JObject BuildPaymentDetailsPayload(
            CardDetails cardDetails,
            JObject paymentDetailsConfig)
        {
            string digitsOnlyExpiry = new(cardDetails.ExpiryDate.Where(char.IsDigit).ToArray());
            if (digitsOnlyExpiry.Length != 4)
            {
                throw new InvalidOperationException("Payment-card expiry must use four digits in MMYY format.");
            }

            (string firstName, string lastName) = SplitCardholderName(cardDetails.NameOnCard);

            return new JObject
            {
                ["cardDetails"] = new JObject
                {
                    ["cardType"] = ResolveCardType(cardDetails.PaymentCardType),
                    ["expirationDate"] = $"{digitsOnlyExpiry[..2]}/{digitsOnlyExpiry[2..]}",
                    ["expirationMonthNum"] = digitsOnlyExpiry[..2],
                    ["expirationYearNum"] = digitsOnlyExpiry[2..],
                    ["lastFourDigits"] = cardDetails.CardNumber[^4..],
                    ["paymentCardSecurityCode"] = cardDetails.Cvv,
                    ["paymentCardNum"] = cardDetails.CardNumber,
                    ["cardHolderName"] = new JObject
                    {
                        ["firstName"] = firstName,
                        ["middleName"] = null,
                        ["lastName"] = lastName,
                    },
                    ["billingAdr"] = new JObject
                    {
                        ["addressLine1Text"] = cardDetails.AddressLineOne,
                        ["addressLine2Text"] = cardDetails.AddressLineTwo,
                        ["cityLocalityName"] = cardDetails.TownCity,
                        ["countryCode"] = ResolveCountryCode(cardDetails.Country),
                        ["countrySubdivisionCode"] = cardDetails.State.IsNullOrWhiteSpace()
                            ? null
                            : cardDetails.State,
                        ["postalCode"] = cardDetails.Postcode,
                    },
                    ["paymentType"] = "CREDIT CARD",
                    ["purchaseVrfyFlag"] = false,
                    ["savedFopId"] = null,
                },
                ["appId"] = paymentDetailsConfig.RequireToken("headers.appId").ToString(),
            };
        }

        private static JObject BuildAuthenticationPayload(string serverTransactionId)
        {
            return new JObject
            {
                ["DFReferenceId"] = string.Empty,
                ["browserSpecification"] = new JObject
                {
                    ["colorDepthCnt"] = "24",
                    ["javaEnabled"] = false,
                    ["languageCode"] = "en-GB",
                    ["screenHeightCnt"] = "1080",
                    ["screenWidthCnt"] = "1920",
                    ["timeZoneMinuteCnt"] = "0",
                    ["userAgentHeaderText"] = "TestAutomationHttpClient/v1.0",
                },
                ["formOfPayment"] = new JObject
                {
                    ["authenticationData"] = new JObject
                    {
                        ["serverTransactionId"] = serverTransactionId,
                    },
                },
            };
        }

        private static NameValueCollection BuildHeaders(
            JObject endpointConfig,
            string paymentReferenceId,
            string transactionId = "")
        {
            NameValueCollection headers = new();
            JObject configuredHeaders = RequireObject(endpointConfig, "headers");
            foreach (JProperty header in configuredHeaders.Properties())
            {
                headers[header.Name] = header.Value.Type == JTokenType.String
                    ? header.Value.ToString()
                    : header.Value.ToString(Formatting.None);
            }

            headers["paymentReferenceId"] = paymentReferenceId;
            if (!transactionId.IsNullOrWhiteSpace())
            {
                headers["TransactionId"] = transactionId;
            }

            AddCredentialHeader(headers, "X-Vaa_digitalfirst", "HppVaaDigitalFirstToken");
            AddCredentialHeader(
                headers,
                "X-Delta_enterprisepaymentsHPP_Test",
                "HppDeltaEnterprisePaymentsToken");
            return headers;
        }

        private static void AddCredentialHeader(
            NameValueCollection headers,
            string headerName,
            string settingName)
        {
            string value = ConfigReader.Get(settingName, string.Empty);
            if (!value.IsNullOrWhiteSpace())
            {
                headers[headerName] = value;
            }
        }

        private static async Task<JObject> GetJsonAsync(string url)
        {
            using HttpResponseMessage response = WebRequest.Get(url).EnsureIsSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            return JObject.Parse(body);
        }

        private static async Task<JObject> PostJsonAsync(
            string url,
            JObject payload,
            NameValueCollection headers)
        {
            using HttpResponseMessage response = WebRequest.Json.Post(
                url,
                payload.ToString(Formatting.None),
                headers).EnsureIsSuccessStatusCode();
            string body = await response.Content.ReadAsStringAsync();
            return JObject.Parse(body);
        }

        private static JObject RequireObject(JObject source, string path)
        {
            JToken token = source.RequireToken(path);
            return token as JObject
                ?? throw new JsonException($"Required JSON path '{path}' was not an object.");
        }

        private static string RequireString(JObject source, string path)
        {
            string value = source.RequireToken(path).ToString();
            return value.IsNullOrWhiteSpace()
                ? throw new JsonException($"Required JSON path '{path}' was empty.")
                : value;
        }

        private static string ResolvePsd2ConfigUrl(JObject hppConfig)
        {
            JToken? configUrl = hppConfig.SelectToken("configs.psd2data.configurl")
                ?? hppConfig.SelectToken("configs.psd2.configurl");
            string value = configUrl?.ToString() ?? string.Empty;
            return value.IsNullOrWhiteSpace()
                ? throw new JsonException("The HPP configuration did not contain a PSD2 config URL.")
                : value;
        }

        private static (string FirstName, string LastName) SplitCardholderName(string name)
        {
            string[] parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 2)
            {
                throw new InvalidOperationException("Cardholder name must include first and last names.");
            }

            return (parts[0], string.Join(' ', parts.Skip(1)));
        }

        private static string ResolveCountryCode(string country)
        {
            if (country.Is("United Kingdom") || country.Is("GB"))
                return "GB";
            if (country.Is("United States") || country.Is("US"))
                return "US";

            throw new InvalidOperationException(
                $"API card payment does not yet support billing country '{country}'.");
        }

        private static string ResolveCardType(CardType cardType)
        {
            return cardType switch
            {
                CardType.Visa => "VI",
                CardType.Mastercard => "MC",
                CardType.Discover => "DS",
                CardType.Diners => "DC",
                _ => throw new InvalidOperationException(
                    $"API card payment does not support card type '{cardType}'."),
            };
        }
    }
}
