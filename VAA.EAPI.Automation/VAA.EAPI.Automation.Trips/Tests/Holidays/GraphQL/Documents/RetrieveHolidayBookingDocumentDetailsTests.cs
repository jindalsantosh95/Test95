using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Trips.Constants;
using VAA.EAPI.Automation.Trips.Helpers;
using GraphQlEndpoint = VAA.EAPI.Automation.Trips.Requests.Endpoint.GraphQL;

namespace VAA.EAPI.Automation.Trips.Tests.Holidays.GraphQL.Documents
{
    public class RetrieveHolidayBookingDocumentDetailsTests
    {
        private readonly string _env = Config.Environment;
        private const string ConfirmedBookingStatus = "CONFIRMED";

        public static IEnumerable<object[]> OptionStatuses()
        {
            yield return new object[] { "FIRM" };
            yield return new object[] { "DEFINITE" };
        }

        [SkippableTheory]
        [Trait("Category", Traits.HolidayDocumentsRegression)]
        [MemberData(nameof(OptionStatuses), MemberType = typeof(RetrieveHolidayBookingDocumentDetailsTests))]
        public async Task RetrieveHolidayBookingDocumentDetails_ConfirmedFirmOrDefinite_ReturnsCoreDocuments(string optionStatus)
        {
            (JObject result, string bookingReference, string debugContext) = await RetrieveHolidayBookingDocuments(optionStatus);

            JArray documentInformation = (JArray)result.RequireToken("data.retrieveHolidayBookingDocumentDetails.documentInformation");
            Assert.True(documentInformation.Count > 0, $"Expected at least one document but none were returned. {debugContext}");

            foreach (JObject document in documentInformation.Children<JObject>())
            {
                ValidateDocumentShape(document, bookingReference, debugContext);
                AssertDocumentTypeAvailability(document);
            }
        }

        [SkippableFact]
        [Trait("Category", Traits.HolidayDocumentsRegression)]
        public async Task RetrieveHolidayBookingDocumentDetails_ConfirmedFullyPaid_ReturnsAdditionalDocuments()
        {
            if (_env.Is("sit") || _env.Is("dev") || _env.Is("uat"))
            {
                TestDataPrerequisiteHelper.Skip($"Test skipped in '{_env}' environment: ITINERARYDOCUMENT is not available for FULLY PAID holiday bookings.");
            }

            (JObject result, string bookingReference, string debugContext) = await RetrieveHolidayBookingDocuments("FULLY PAID");

            JArray documentInformation = (JArray)result.RequireToken("data.retrieveHolidayBookingDocumentDetails.documentInformation");
            Assert.True(documentInformation.Count > 0, $"Expected at least one document for fully paid booking but none were returned. {debugContext}");

            foreach (JObject document in documentInformation.Children<JObject>())
            {
                ValidateDocumentShape(document, bookingReference, debugContext);
                AssertDocumentTypeAvailability(document);
            }
        }

        [SkippableTheory]
        [Trait("Category", Traits.HolidayDocumentsRegression)]
        [MemberData(nameof(OptionStatuses), MemberType = typeof(RetrieveHolidayBookingDocumentDetailsTests))]
        public async Task RetrieveHolidayBooking_FirmOrDefinite_ReturnsImportantInformation(string optionStatus)
        {
            (JObject retrieveHolidayBookingResult, _, string debugContext, _) = await HolidaysHelper
                .RetrieveHolidayBookingResultAsync(_env, optionStatus, ConfirmedBookingStatus);

            JArray importantInformation = (JArray)retrieveHolidayBookingResult.RequireToken("data.retrieveHolidayBooking.importantInformation");
            Assert.True(importantInformation.Count > 0, $"Expected importantInformation to contain at least one item for optionStatus '{optionStatus}'. {debugContext}");

            foreach (JObject information in importantInformation.Children<JObject>())
            {
                Assert.False((information["priority"]?.ToString()).IsNullOrWhiteSpace(), $"importantInformation.priority should not be null or empty. {debugContext}");
                Assert.False((information["informationText"]?.ToString()).IsNullOrWhiteSpace(), $"importantInformation.informationText should not be null or empty. {debugContext}");
            }
        }

        [SkippableTheory]
        [Trait("Category", Traits.HolidayDocumentsRegression)]
        [MemberData(nameof(OptionStatuses), MemberType = typeof(RetrieveHolidayBookingDocumentDetailsTests))]
        public async Task RetrieveHolidayBookingDocumentDetails_FirmOrDefinite_ReturnsTicketsAsUnavailable(string optionStatus)
        {
            (JObject result, string bookingReference, string debugContext) = await RetrieveHolidayBookingDocuments(optionStatus);

            JArray documentInformation = (JArray)result.RequireToken("data.retrieveHolidayBookingDocumentDetails.documentInformation");
            JObject? ticketsDocument = documentInformation
                .Children<JObject>()
                .FirstOrDefault(d => string.Equals(d["documentType"]?.ToString(), "TRVDOC", StringComparison.OrdinalIgnoreCase));

            Assert.NotNull(ticketsDocument);

            Assert.Equal("Tickets", ticketsDocument!["name"]?.ToString());
            Assert.Equal(bookingReference, ticketsDocument["bookingReference"]?.ToString());
            Assert.True(ticketsDocument["documentId"] == null || ticketsDocument["documentId"]?.Type == JTokenType.Null,
                $"Expected Tickets documentId to be null for optionStatus '{optionStatus}'. {debugContext}");
            Assert.True(ticketsDocument["documentToken"] == null || ticketsDocument["documentToken"]?.Type == JTokenType.Null,
                $"Expected Tickets documentToken to be null for optionStatus '{optionStatus}'. {debugContext}");
            Assert.Equal(404, ticketsDocument["status"]?.Value<int>());
            Assert.True(ticketsDocument["errors"] == null || ticketsDocument["errors"]?.Type == JTokenType.Null,
                $"Expected Tickets errors to be null for optionStatus '{optionStatus}'. {debugContext}");
        }

        [SkippableFact]
        [Trait("Category", Traits.HolidayDocumentsRegression)]
        public async Task RetrieveHolidayBookingDocument_ValidDocumentTokens_ReturnsValidatedDocumentBytes()
        {
            (JObject result, _, string debugContext) = await RetrieveHolidayBookingDocuments("FIRM");
            JArray documentInformation = (JArray)result.RequireToken("data.retrieveHolidayBookingDocumentDetails.documentInformation");
            List<JObject> availableDocuments = documentInformation.Children<JObject>()
                .Where(item => item["status"]?.Value<int>() == 200)
                .ToList();
            Assert.True(availableDocuments.Count > 0, $"Expected at least one available holiday document. {debugContext}");

            foreach (JObject document in availableDocuments)
            {
                string documentId = document.RequireToken("documentId").ToString();
                string documentToken = document.RequireToken("documentToken").ToString();
                Assert.False(documentId.IsNullOrWhiteSpace(), $"Available documentId should not be empty. {debugContext}");
                Assert.False(documentToken.IsNullOrWhiteSpace(), $"Available documentToken should not be empty. {debugContext}");

                HttpResponseMessage documentResponse = GraphQlEndpoint
                    .RetrieveHolidayBookingDocument(_env, documentId, documentToken)
                    .EnsureGQLResponseSuccess();

                JObject documentResult = JObject.Parse(await documentResponse.Content.ReadAsStringAsync());
                Assert.Equal(documentId, documentResult.RequireToken("data.retrieveHolidayBookingDocument.documentId").ToString());

                JToken documentBytes = documentResult.RequireToken("data.retrieveHolidayBookingDocument.document");
                Assert.Equal(JTokenType.Array, documentBytes.Type);
                byte[] downloadedDocument = documentBytes.ToObject<byte[]>() ?? [];
                Assert.True(downloadedDocument.Length > 0, $"Downloaded document bytes should not be empty for '{document["documentType"]}'. {debugContext}");
            }
        }

        [SkippableTheory]
        [Trait("Category", Traits.HolidayDocumentsRegression)]
        [MemberData(nameof(OptionStatuses), MemberType = typeof(RetrieveHolidayBookingDocumentDetailsTests))]
        public async Task RetrieveHolidayBookingDocumentDetails_FirmOrDefinite_ReturnsDocumentsInExpectedOrder(string optionStatus)
        {
            (JObject result, _, string debugContext) = await RetrieveHolidayBookingDocuments(optionStatus);

            JArray documentInformation = (JArray)result.RequireToken("data.retrieveHolidayBookingDocumentDetails.documentInformation");
            List<string> documentTypes = documentInformation
                .Children<JObject>()
                .Select(d => d["documentType"]?.ToString() ?? string.Empty)
                .ToList();

            int ticketsIndex = documentTypes.FindIndex(type => type.Equals("TRVDOC", StringComparison.OrdinalIgnoreCase));
            int invoiceIndex = documentTypes.FindIndex(type => type.Equals("CBKINV", StringComparison.OrdinalIgnoreCase));
            int atolIndex = documentTypes.FindIndex(type => type.Equals("ATOLCERTIFICATE", StringComparison.OrdinalIgnoreCase));

            if (ticketsIndex >= 0 && invoiceIndex >= 0)
            {
                Assert.True(ticketsIndex < invoiceIndex,
                    $"Expected 'TRVDOC' to appear before 'CBKINV'. Returned order: {string.Join(" -> ", documentTypes)}. {debugContext}");
            }

            if (invoiceIndex >= 0 && atolIndex >= 0)
            {
                Assert.True(invoiceIndex < atolIndex,
                    $"Expected 'CBKINV' to appear before 'ATOLCERTIFICATE'. Returned order: {string.Join(" -> ", documentTypes)}. {debugContext}");
            }
        }

        private async Task<(JObject Result, string BookingReference, string DebugContext)> RetrieveHolidayBookingDocuments(string optionStatus)
        {
            (JObject retrieveHolidayBookingResult, string bookingReference, string debugContext, string tripResourceId) = await HolidaysHelper
                .RetrieveHolidayBookingResultAsync(_env, optionStatus, ConfirmedBookingStatus);

            HttpResponseMessage documentDetailsResponse = GraphQlEndpoint
                .RetrieveHolidayBookingDocumentDetails(_env, tripResourceId)
                .EnsureGQLResponseSuccess();

            string documentDetailsBody = await documentDetailsResponse.Content.ReadAsStringAsync();
            Assert.False(documentDetailsBody.IsNullOrWhiteSpace(), $"RetrieveHolidayBookingDocumentDetails response body was empty. {debugContext}, tripResourceId='{tripResourceId}'");

            JObject documentDetailsResult = JObject.Parse(documentDetailsBody);
            Assert.True(DynamicHelper.HasProperty(documentDetailsResult, "data"), $"Response should contain data property. {debugContext}, tripResourceId='{tripResourceId}'");
            JToken? rootErrors = documentDetailsResult["errors"];
            Assert.True(rootErrors == null || rootErrors.Type == JTokenType.Null,
                $"Expected GraphQL errors to be null in RetrieveHolidayBookingDocumentDetails response but got '{rootErrors}'. {debugContext}, tripResourceId='{tripResourceId}'");

            return (documentDetailsResult, bookingReference, debugContext);

        }

        private static void ValidateDocumentShape(JObject document, string bookingReference, string debugContext)
        {
            Assert.False((document["name"]?.ToString()).IsNullOrWhiteSpace(), $"documentInformation.name should not be null or empty. {debugContext}");
            Assert.False((document["documentType"]?.ToString()).IsNullOrWhiteSpace(), $"documentInformation.documentType should not be null or empty. {debugContext}");

            string returnedBookingReference = document["bookingReference"]?.ToString() ?? string.Empty;
            Assert.True(returnedBookingReference.Equals(bookingReference, StringComparison.OrdinalIgnoreCase),
                $"documentInformation.bookingReference should match source bookingReference. Expected '{bookingReference}', got '{returnedBookingReference}'. {debugContext}");

            Assert.True(int.TryParse(document["status"]?.ToString(), out int statusCode), $"documentInformation.status should be numeric. Actual='{document["status"]}'. {debugContext}");

            string documentType = document["documentType"]?.ToString() ?? string.Empty;
            string? documentId = document["documentId"]?.ToString();
            string? documentToken = document["documentToken"]?.ToString();
            JToken? errors = document["errors"];

            if (statusCode == 200)
            {
                Assert.False(documentId.IsNullOrWhiteSpace(), $"documentInformation.documentId should not be null or empty when status is 200. {debugContext}");
                Assert.False(documentToken.IsNullOrWhiteSpace(), $"documentInformation.documentToken should not be null or empty when status is 200. {debugContext}");

                if (errors is not null && errors.Type != JTokenType.Null)
                {
                    Assert.True(errors.Type == JTokenType.Array, $"documentInformation.errors should be null or an array. Actual type='{errors.Type}'. {debugContext}");
                    Assert.True(!errors.Any(), $"documentInformation.errors should be null or empty when status is 200 but got '{errors}'. {debugContext}");
                }

                return;
            }

            Assert.True(documentToken.IsNullOrWhiteSpace(), $"documentInformation.documentToken should be null/empty when status is not 200. status='{statusCode}'. {debugContext}");
            if (documentType.Equals("TRVDOC", StringComparison.OrdinalIgnoreCase) && statusCode == 404)
            {
                Assert.True(documentId.IsNullOrWhiteSpace(), $"TRVDOC documentId should be null/empty when status is 404. {debugContext}");
                Assert.True(errors == null || errors.Type == JTokenType.Null, $"TRVDOC errors should be null when status is 404. {debugContext}");
                return;
            }

            Assert.Fail($"Only TRVDOC may be returned with a non-200 status. documentType='{documentType}', status='{statusCode}'. {debugContext}");
        }

        private static void AssertDocumentTypeAvailability(JObject document)
        {
            string documentType = document["documentType"]?.ToString() ?? string.Empty;
            int statusCode = document["status"]?.Value<int>() ?? 0;

            if (documentType.Equals("CBKINV", StringComparison.OrdinalIgnoreCase)
                || documentType.Equals("ATOLCERTIFICATE", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Equal(200, statusCode);
            }
            else if (documentType.Equals("TRVDOC", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Contains(statusCode, new[] { 200, 404 });
            }
        }
    }
}
