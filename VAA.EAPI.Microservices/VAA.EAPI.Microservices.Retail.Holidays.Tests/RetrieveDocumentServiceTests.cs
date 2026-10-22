using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Holidays.Application;
using VAA.EAPI.Microservices.Retail.Holidays.Domain;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents;

namespace VAA.EAPI.Microservices.Retail.Holidays.Tests
{
    public sealed class RetrieveDocumentServiceTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public RetrieveDocumentServiceTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("Holidays:RetrieveDocumentService:Url", "https://teste-rest.tbox-test.vholsinternal.co.uk/document-service/v2/");
            ConfigReader.Set("Holidays:EntraScope", "testResource");
            ConfigReader.Set("Holidays:AuthorizationProvider", "AZURE");
            ConfigReader.Set("EapiMsClientID", Guid.NewGuid().ToString());
            ConfigReader.Set("EapiMSSecret", Guid.NewGuid().ToString());
            ConfigReader.Set("AAD:Authority", Guid.NewGuid().ToString());
            ConfigReader.Set("AAD:TenantId", Guid.NewGuid().ToString());
            ConfigReader.Set("Holidays:DocumentDownloadAllowedHosts", "vsolpublic.virginholidays.co.uk");

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IOAuthTokenProvider, Core.Xunit.Azure.OAuthTokenProvider>()
                                                      .AddSingleton<IRetrieveDocumentService, RetrieveDocumentService>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }

        [Fact]
        public async Task RetrieveDocumentService_Success()
        {
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;
            var request = new RetrieveDocumentRequest()
            {
                BookingReference = "6996164",
            };

            var result = await retrieveDocumentService.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Documents);
            Assert.NotEmpty(result.Documents);
            Assert.Equal(6, result.Documents.Count());

            // ── Doc 1 - Booking Invoice (CBKINV) ──────────────────────────────────────
            var bookingInvoice = result.Documents.First();
            Assert.NotNull(bookingInvoice);
            Assert.Equal("Mrs Johanne Stenhouse", bookingInvoice.LeadPassengerName);
            Assert.Equal(new DateOnly(2026, 01, 13), bookingInvoice.BookingDate);
            Assert.Equal(new DateOnly(2026, 10, 08), bookingInvoice.DepartureDate);
            Assert.Equal(51185994, bookingInvoice.DocumentId);
            Assert.Equal(56998610, bookingInvoice.DocumentStoreId);
            Assert.Equal("CBKINV", bookingInvoice.DocumentType);
            Assert.Equal("Booking Invoice", bookingInvoice.DocumentName);
            Assert.NotNull(bookingInvoice.DocumentGroups);
            Assert.Empty(bookingInvoice.DocumentGroups);
            Assert.Equal("- NOT DISPATCHED -", bookingInvoice.DispatchStatus);
            Assert.Equal("", bookingInvoice.DispatchMethods);
            Assert.Equal("C", bookingInvoice.DocumentCategory);
            Assert.Equal("BK", bookingInvoice.DocumentSubCategory);
            Assert.True(bookingInvoice.QualityChecked);
            Assert.False(bookingInvoice.DeletedFromStorage);
            Assert.Equal(new DateTime(2026, 01, 13, 20, 54, 27), bookingInvoice.CreatedDateTime);
            Assert.NotNull(bookingInvoice.DispatchDetails);
            Assert.Empty(bookingInvoice.DispatchDetails);
            Assert.Null(bookingInvoice.DispatchSummary);
            Assert.True(bookingInvoice.QualityCheckedInfo);
            Assert.Equal(6996164, bookingInvoice.BookingId);
            Assert.Equal("CONFIRMED", bookingInvoice.BookingStatus);
            Assert.Equal("FIRM", bookingInvoice.OptionStatus);
            Assert.Null(bookingInvoice.ItemNumber);
            Assert.Null(bookingInvoice.ProductCode);
            Assert.Null(bookingInvoice.SupplierCode);
            Assert.Null(bookingInvoice.SupplierName);
            Assert.Null(bookingInvoice.Supplier);

            // ── Doc 2 - ATOL Certificate (ATOLCERTIFICATE) ────────────────────────────
            var atolCertificate = result.Documents.ElementAt(1);
            Assert.NotNull(atolCertificate);
            Assert.Equal(51185995, atolCertificate.DocumentId);
            Assert.Equal("ATOLCERTIFICATE", atolCertificate.DocumentType);
            Assert.Equal("ATOL Certificate", atolCertificate.DocumentName);
            Assert.Equal("- NOT DISPATCHED -", atolCertificate.DispatchStatus);
            Assert.Empty(atolCertificate.DocumentGroups);
            Assert.Empty(atolCertificate.DispatchDetails);
            Assert.Null(atolCertificate.DispatchSummary);
            Assert.Null(atolCertificate.ItemNumber);
            Assert.Null(atolCertificate.Supplier);

            // ── Doc 3 - Customer Notification (CUSNOTIF) ──────────────────────────────
            var customerNotification = result.Documents.ElementAt(2);
            Assert.NotNull(customerNotification);
            Assert.Equal(51185996, customerNotification.DocumentId);
            Assert.Equal("CUSNOTIF", customerNotification.DocumentType);
            Assert.Equal("Customer Notification", customerNotification.DocumentName);
            Assert.Equal("SUC-1", customerNotification.DispatchStatus);
            Assert.Equal("CCM", customerNotification.DispatchMethods);
            Assert.Equal("CCM - jo.stenhouse@sky.com", customerNotification.DispatchSummary);
            Assert.Empty(customerNotification.DocumentGroups);
            Assert.NotEmpty(customerNotification.DispatchDetails);
            Assert.Equal(1, customerNotification.DispatchDetails.Count());
            Assert.Null(customerNotification.ItemNumber);
            Assert.Null(customerNotification.Supplier);

            var cusNotifDispatch = customerNotification.DispatchDetails.First();
            Assert.Equal(1, cusNotifDispatch.DispatchId);
            Assert.Equal(51185996, cusNotifDispatch.DocumentId);
            Assert.Equal(21778691, cusNotifDispatch.DispatchQueueId);
            Assert.Equal(new DateTime(2026, 01, 13, 20, 53, 00), cusNotifDispatch.ToDispatchDateTime);
            Assert.Equal(new DateTime(2026, 01, 13, 21, 02, 43), cusNotifDispatch.DispatchedDateTime);
            Assert.False(cusNotifDispatch.SendAutomatically);
            Assert.False(cusNotifDispatch.Posted);
            Assert.Equal("CCM", cusNotifDispatch.DispatchMethod);
            Assert.Equal("SUCCESS", cusNotifDispatch.DispatchStatus);
            Assert.Equal("jo.stenhouse@sky.com", cusNotifDispatch.Recipient);
            Assert.Equal("FIRST-NOTIFICATION", cusNotifDispatch.TrackingNo);
            Assert.Equal("AD", cusNotifDispatch.Dispatcher);
            Assert.Equal(-1, cusNotifDispatch.Collate);

            // ── Doc 4 - Customer Notification Email (CUSNOTIFEML) ────────────────────
            var customerNotificationEmail = result.Documents.ElementAt(3);
            Assert.NotNull(customerNotificationEmail);
            Assert.Equal(51192643, customerNotificationEmail.DocumentId);
            Assert.Equal(0, customerNotificationEmail.DocumentStoreId);
            Assert.Equal("CUSNOTIFEML", customerNotificationEmail.DocumentType);
            Assert.Equal("Customer Notification - Email", customerNotificationEmail.DocumentName);
            Assert.Equal("SUC-1", customerNotificationEmail.DispatchStatus);
            Assert.Equal("CCM", customerNotificationEmail.DispatchMethods);
            Assert.Equal("CCM - jo.stenhouse@sky.com", customerNotificationEmail.DispatchSummary);
            Assert.Equal(new DateTime(2026, 01, 14, 17, 44, 59), customerNotificationEmail.CreatedDateTime);
            Assert.Null(customerNotificationEmail.ItemNumber);
            Assert.Null(customerNotificationEmail.Supplier);

            var cusNotifEmailDispatch = customerNotificationEmail.DispatchDetails.First();
            Assert.Equal(1, cusNotifEmailDispatch.DispatchId);
            Assert.Equal(51192643, cusNotifEmailDispatch.DocumentId);
            Assert.Equal(0, cusNotifEmailDispatch.DispatchQueueId);
            Assert.Null(cusNotifEmailDispatch.ToDispatchDateTime);
            Assert.Equal(new DateTime(2026, 01, 13, 21, 04, 01), cusNotifEmailDispatch.DispatchedDateTime);
            Assert.Equal("CCM", cusNotifEmailDispatch.DispatchMethod);
            Assert.Equal("SUCCESS", cusNotifEmailDispatch.DispatchStatus);
            Assert.Equal("jo.stenhouse@sky.com", cusNotifEmailDispatch.Recipient);
            Assert.Null(cusNotifEmailDispatch.TrackingNo);
            Assert.Equal(-1, cusNotifEmailDispatch.Collate);

            // ── Doc 5 - Supplier Notification Accommodation (SUPNOACC) ───────────────
            var supplierNotifAccom = result.Documents.ElementAt(4);
            Assert.NotNull(supplierNotifAccom);
            Assert.Equal(51186560, supplierNotifAccom.DocumentId);
            Assert.Equal("SUPNOACC", supplierNotifAccom.DocumentType);
            Assert.Equal("Supplier Notification Accommodation", supplierNotifAccom.DocumentName);
            Assert.Equal("SUC-2", supplierNotifAccom.DispatchStatus);
            Assert.Equal("EML", supplierNotifAccom.DispatchMethods);
            Assert.Equal("EML - xyz@virginholidays.com", supplierNotifAccom.DispatchSummary);
            Assert.Equal("S", supplierNotifAccom.DocumentCategory);
            Assert.Equal("SU", supplierNotifAccom.DocumentSubCategory);
            Assert.Equal(1, supplierNotifAccom.ItemNumber);
            Assert.Equal("HTL", supplierNotifAccom.ProductCode);
            Assert.Equal("H1740", supplierNotifAccom.SupplierCode);
            Assert.Equal("Holiday Inn Club Vacations at Orange Lake Resort", supplierNotifAccom.SupplierName);
            Assert.NotNull(supplierNotifAccom.Supplier);
            Assert.Equal("H1740", supplierNotifAccom.Supplier.Code);
            Assert.Equal("Holiday Inn Club Vacations at Orange Lake Resort", supplierNotifAccom.Supplier.Name);
            Assert.NotNull(supplierNotifAccom.DocumentGroups);
            Assert.Equal(1, supplierNotifAccom.DocumentGroups.Count());
            Assert.Equal("SUPNOT", supplierNotifAccom.DocumentGroups.First().Code);
            Assert.Equal("Supplier Notifications", supplierNotifAccom.DocumentGroups.First().Name);
            Assert.Equal(2, supplierNotifAccom.DispatchDetails.Count());

            var supAccomDispatch1 = supplierNotifAccom.DispatchDetails.First();
            Assert.Equal(1, supAccomDispatch1.DispatchId);
            Assert.Equal(51186560, supAccomDispatch1.DocumentId);
            Assert.Equal(21778926, supAccomDispatch1.DispatchQueueId);
            Assert.Equal(new DateTime(2026, 01, 12, 23, 12, 00), supAccomDispatch1.ToDispatchDateTime);
            Assert.Equal(new DateTime(2026, 01, 13, 23, 34, 36), supAccomDispatch1.DispatchedDateTime);
            Assert.Equal("EML", supAccomDispatch1.DispatchMethod);
            Assert.Equal("SUCCESS", supAccomDispatch1.DispatchStatus);
            Assert.Equal("xyz@virginholidays.com", supAccomDispatch1.Recipient);
            Assert.Equal("AD", supAccomDispatch1.Dispatcher);
            Assert.Equal(-1, supAccomDispatch1.Collate);

            var supAccomDispatch2 = supplierNotifAccom.DispatchDetails.ElementAt(1);
            Assert.Equal(2, supAccomDispatch2.DispatchId);
            Assert.Equal(21778927, supAccomDispatch2.DispatchQueueId);
            Assert.Equal(new DateTime(2026, 01, 13, 23, 34, 37), supAccomDispatch2.DispatchedDateTime);
            Assert.Equal("EML", supAccomDispatch2.DispatchMethod);
            Assert.Equal("SUCCESS", supAccomDispatch2.DispatchStatus);
            Assert.Equal("xyz@virginholidays.com", supAccomDispatch2.Recipient);

            // ── Doc 6 - Supplier Notification Car (SUPNOCAR) ──────────────────────────
            var supplierNotifCar = result.Documents.ElementAt(5);
            Assert.NotNull(supplierNotifCar);
            Assert.Equal(51186596, supplierNotifCar.DocumentId);
            Assert.Equal("SUPNOCAR", supplierNotifCar.DocumentType);
            Assert.Equal("Supplier Notification Car", supplierNotifCar.DocumentName);
            Assert.Equal("SUC-1", supplierNotifCar.DispatchStatus);
            Assert.Equal("EML", supplierNotifCar.DispatchMethods);
            Assert.Equal("EML - xyz@virginholidays.com", supplierNotifCar.DispatchSummary);
            Assert.Equal(1, supplierNotifCar.ItemNumber);
            Assert.Equal("CAR", supplierNotifCar.ProductCode);
            Assert.Equal("HERTZ", supplierNotifCar.SupplierCode);
            Assert.Equal("Hertz", supplierNotifCar.SupplierName);
            Assert.NotNull(supplierNotifCar.Supplier);
            Assert.Equal("HERTZ", supplierNotifCar.Supplier.Code);
            Assert.Equal("Hertz", supplierNotifCar.Supplier.Name);
            Assert.Equal(1, supplierNotifCar.DocumentGroups.Count());
            Assert.Equal("SUPNOT", supplierNotifCar.DocumentGroups.First().Code);
            Assert.Equal(1, supplierNotifCar.DispatchDetails.Count());

            var supCarDispatch = supplierNotifCar.DispatchDetails.First();
            Assert.Equal(1, supCarDispatch.DispatchId);
            Assert.Equal(51186596, supCarDispatch.DocumentId);
            Assert.Equal(21778961, supCarDispatch.DispatchQueueId);
            Assert.Equal(new DateTime(2026, 01, 12, 23, 14, 00), supCarDispatch.ToDispatchDateTime);
            Assert.Equal(new DateTime(2026, 01, 13, 23, 34, 59), supCarDispatch.DispatchedDateTime);
            Assert.Equal("EML", supCarDispatch.DispatchMethod);
            Assert.Equal("SUCCESS", supCarDispatch.DispatchStatus);
            Assert.Equal("xyz@virginholidays.com", supCarDispatch.Recipient);
            Assert.Equal("AD", supCarDispatch.Dispatcher);
            Assert.Equal(-1, supCarDispatch.Collate);
        }

        [Fact]
        public async Task RetrieveDocumentService_AllDocs_Success()
        {
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;
            var request = new RetrieveDocumentRequest()
            {
                BookingReference = "6997658",
            };

            var result = await retrieveDocumentService.Handle(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.Documents);
            Assert.NotEmpty(result.Documents);
            Assert.Equal(10, result.Documents.Count());
            
            var oneOfDocument = result.Documents.ElementAt(2);
            Assert.NotNull(oneOfDocument);
            Assert.True(oneOfDocument.DocumentId > 0);
            Assert.NotNull(oneOfDocument.DocumentType);
            Assert.NotNull(oneOfDocument.DocumentName);
            Assert.NotNull(oneOfDocument.DispatchStatus);
            Assert.NotNull(oneOfDocument.DispatchMethods);
            Assert.NotNull(oneOfDocument.DispatchSummary);
            Assert.NotEmpty(oneOfDocument.DispatchDetails);
        }

        [Fact]
        public async Task RetrieveDocument_ReturnsBoundedPdfWithSafeMetadata()
        {
            ConfigReader.Set("Holidays:DocumentDownloadAllowedHosts", "vsolpublic.virginholidays.co.uk:443");
            var retrieveDocumentService = _serviceProvider.GetRequiredService<IRetrieveDocumentService>();

            var result = await retrieveDocumentService.RetrieveDocument(new RetrieveDocumentDownloadRequest
            {
                BookingId = "booking-123",
                DocumentId = "document-123",
                DocumentUrl = "https://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-123/INVOICE/document-123.pdf"
            }, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("document-123", result.DocumentId);
            Assert.NotNull(result.Document);
            Assert.NotEmpty(result.Document);
            Assert.Equal("application/pdf", result.ContentType);
            Assert.Equal("document-123.pdf", result.FileName);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public async Task RetrieveDocument_UsesDefaultTimeout_WhenConfiguredTimeoutIsNotPositive(int configuredTimeoutSeconds)
        {
            ConfigReader.Set("Holidays:DocumentDownloadTimeoutSeconds", configuredTimeoutSeconds.ToString());
            try
            {
                var retrieveDocumentService = _serviceProvider.GetRequiredService<IRetrieveDocumentService>();

                var result = await retrieveDocumentService.RetrieveDocument(new RetrieveDocumentDownloadRequest
                {
                    BookingId = "booking-123",
                    DocumentId = "document-123",
                    DocumentUrl = "https://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-123/INVOICE/document-123.pdf"
                }, CancellationToken.None);

                Assert.NotNull(result);
                Assert.NotEmpty(result.Document);
            }
            finally
            {
                ConfigReader.Set("Holidays:DocumentDownloadTimeoutSeconds", "30");
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(-1)]
        public void RetrieveDocument_UsesThirtySecondDefault_WhenConfiguredTimeoutIsNotPositive(int configuredTimeoutSeconds)
        {
            Assert.Equal(30, RetrieveDocumentService.NormalizeTimeoutSeconds(configuredTimeoutSeconds));
        }

        [Fact]
        public async Task RetrieveDocument_RejectsUnsafeTargetBeforeDownloading()
        {
            var retrieveDocumentService = _serviceProvider.GetRequiredService<IRetrieveDocumentService>();

            await Assert.ThrowsAsync<InvalidOperationException>(() => retrieveDocumentService.RetrieveDocument(new RetrieveDocumentDownloadRequest
            {
                BookingId = "booking-123",
                DocumentId = "document-123",
                DocumentUrl = "http://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-123/INVOICE/document-123.pdf"
            }, CancellationToken.None));
        }

        [Theory]
        [InlineData("https://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-123/INVOICE/")]
        [InlineData("https://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-123/INVOICE/%2F")]
        public async Task RetrieveDocument_RejectsUrlWithoutDocumentFileName(string documentUrl)
        {
            var retrieveDocumentService = _serviceProvider.GetRequiredService<IRetrieveDocumentService>();

            await Assert.ThrowsAsync<ArgumentException>(() => retrieveDocumentService.RetrieveDocument(new RetrieveDocumentDownloadRequest
            {
                BookingId = "booking-123",
                DocumentId = "document-123",
                DocumentUrl = documentUrl
            }, CancellationToken.None));
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkService_Success()
        {
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;
            var docId = 48508731;

            var result = await retrieveDocumentService.RetrieveDocumentDownloadLink(new DocumentLinkRequest { DocumentId = docId }, CancellationToken.None);

            Assert.NotNull(result);

            // verify download link is mapped and is a valid url
            Assert.NotNull(result.Response.DownloadLink);
            Assert.NotEmpty(result.Response.DownloadLink);
            Assert.True(Uri.TryCreate(result.Response.DownloadLink, UriKind.Absolute, out var uri));
            Assert.Equal("https", uri.Scheme);
            Assert.Equal("teste-rest.tbox-test.vholsinternal.co.uk", uri.Host);
            Assert.EndsWith(".pdf", result.Response.DownloadLink);

            // verify document id is set by caller after cast
            Assert.Equal(docId, result.Request.DocumentId);
        }

        [Fact]
        public async Task RetrievePendingDocumentList_Success()
        {
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;
            var request = new RetrieveDocumentRequest()
            {
                BookingReference = "6842564",
            };
            var result = await retrieveDocumentService.RetrievePendingDocumentList(request, CancellationToken.None);

            Assert.NotNull(result);
            Assert.NotNull(result.PendingDocuments);
            Assert.NotEmpty(result.PendingDocuments);

            // first item - full item with all fields populated
            var first = result.PendingDocuments.First();
            Assert.NotNull(first);
            Assert.Equal("pdf", first.Format);
            Assert.Equal("TRVDOC", first.DocumentType);
            Assert.Equal("CCM", first.DispatchMethods);
            Assert.Equal("traveldoc-template", first.Template);
            Assert.Equal(new DateTime(2026, 07, 22, 01, 11, 00), first.DispatchDateTime);
            Assert.NotNull(first.Description);
            Assert.NotEmpty(first.Description);

            // first item attributes
            Assert.NotNull(first.Attributes);
            Assert.NotEmpty(first.Attributes);
            Assert.Equal(3, first.Attributes.Count());

            var firstAttr = first.Attributes.First();
            Assert.Equal("DOC_STATUS", firstAttr.Name);
            Assert.Equal("N", firstAttr.Value);

            var secondAttr = first.Attributes.ElementAt(1);
            Assert.Equal("DOC_WAIT", secondAttr.Name);
            Assert.Equal("true", secondAttr.Value);

            var thirdAttr = first.Attributes.ElementAt(2);
            Assert.Equal("GEN_STATUS", thirdAttr.Name);
            Assert.Equal("PENDING", thirdAttr.Value);

            // second item - partial item with optional fields null
            var second = result.PendingDocuments.ElementAt(1);
            Assert.NotNull(second);
            Assert.Null(second.Format);
            Assert.Equal("TRVDOCEML", second.DocumentType);
            Assert.Equal("DNT", second.DispatchMethods);
            Assert.Null(second.Template);
            Assert.Equal(new DateTime(2026, 01, 28, 13, 20, 10), second.DispatchDateTime);
            Assert.NotNull(second.Description);
            Assert.NotEmpty(second.Description);

            // second item attributes
            Assert.NotNull(second.Attributes);
            Assert.NotEmpty(second.Attributes);
            Assert.Equal(3, second.Attributes.Count());

            var secondItemFirstAttr = second.Attributes.First();
            Assert.Equal("DOC_STATUS", secondItemFirstAttr.Name);
            Assert.Equal("N", secondItemFirstAttr.Value);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLink_Returns_Success_Item_When_Document_Found()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var request = new DocumentLinkRequest
            {
                DocumentId = 48508731,
                BookingId = "6996164"
            };

            // Act
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLink(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            // verify request is echoed back
            Assert.NotNull(result.Request);
            Assert.Equal(request.DocumentId, result.Request.DocumentId);
            Assert.Equal(request.BookingId, result.Request.BookingId);

            // verify response
            Assert.NotNull(result.Response);
            Assert.NotNull(result.Response.DownloadLink);
            Assert.NotEmpty(result.Response.DownloadLink);
            Assert.True(Uri.TryCreate(result.Response.DownloadLink, UriKind.Absolute, out _));

            // verify response status
            Assert.NotNull(result.Response.ResponseStatus);
            Assert.Equal(200, result.Response.ResponseStatus.HttpStatusCode);
            Assert.Null(result.Response.ResponseStatus.Errors);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLink_Returns_Failed_Item_When_Document_Not_Found()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var request = new DocumentLinkRequest
            {
                DocumentId = 48508700,
                BookingId = "6996164"
            };

            // Act
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLink(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            // verify request is echoed back
            Assert.NotNull(result.Request);
            Assert.Equal(request.DocumentId, result.Request.DocumentId);
            Assert.Equal(request.BookingId, result.Request.BookingId);

            // verify response
            Assert.NotNull(result.Response);
            Assert.Null(result.Response.DownloadLink);

            // verify response status
            Assert.NotNull(result.Response.ResponseStatus);
            Assert.Equal(500, result.Response.ResponseStatus.HttpStatusCode);
            Assert.NotNull(result.Response.ResponseStatus.Errors);
            Assert.NotEmpty(result.Response.ResponseStatus.Errors);

            // verify error details
            var error = result.Response.ResponseStatus.Errors.First();
            Assert.NotNull(error.Message);
            Assert.NotEmpty(error.Message);
            Assert.NotNull(error.DeveloperMessage);
            Assert.NotEmpty(error.DeveloperMessage);
            Assert.NotNull(error.ErrorSource);
            Assert.Equal("Source System", error.ErrorSource);
        }


        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Returns_All_Success_When_All_Documents_Found()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var requests = new List<DocumentLinkRequest>
                {
                    new () { DocumentId = 48508731, BookingId = "6996164" },
                    new () { DocumentId = 48508732, BookingId = "6996164" }
                };

            // Act
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLinkList(new RetrieveDocumentLinkListRequest() { DocumentLinkRequests = requests }, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.HolidaysDocumentLinkResponses);
            Assert.NotEmpty(result.HolidaysDocumentLinkResponses);
            Assert.Equal(2, result.HolidaysDocumentLinkResponses.Count());

            Assert.All(result.HolidaysDocumentLinkResponses, item =>
            {
                // verify request echoed back
                Assert.NotNull(item.Request);
                Assert.NotNull(item.Request.BookingId);

                // verify response
                Assert.NotNull(item.Response);
                Assert.NotNull(item.Response.DownloadLink);
                Assert.NotEmpty(item.Response.DownloadLink);
                Assert.True(Uri.TryCreate(item.Response.DownloadLink, UriKind.Absolute, out _));

                // verify response status
                Assert.NotNull(item.Response.ResponseStatus);
                Assert.Equal(200, item.Response.ResponseStatus.HttpStatusCode);
                Assert.Null(item.Response.ResponseStatus.Errors);
            });
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Returns_All_Failed_When_No_Documents_Found()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var requests = new List<DocumentLinkRequest>
            {
                new() { DocumentId = 48508700, BookingId = "6996164" },
                new() { DocumentId = 48508701, BookingId = "6996164" }
            };

            // Act
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLinkList(new RetrieveDocumentLinkListRequest() { DocumentLinkRequests = requests }, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.HolidaysDocumentLinkResponses);
            Assert.NotEmpty(result.HolidaysDocumentLinkResponses);
            Assert.Equal(2, result.HolidaysDocumentLinkResponses.Count());

            Assert.All(result.HolidaysDocumentLinkResponses, item =>
            {
                // verify request echoed back
                Assert.NotNull(item.Request);

                // verify response
                Assert.NotNull(item.Response);
                Assert.Null(item.Response.DownloadLink);

                // verify response status
                Assert.NotNull(item.Response.ResponseStatus);
                Assert.Equal(500, item.Response.ResponseStatus.HttpStatusCode);
                Assert.NotNull(item.Response.ResponseStatus.Errors);
                Assert.NotEmpty(item.Response.ResponseStatus.Errors);

                var error = item.Response.ResponseStatus.Errors.First();
                Assert.NotNull(error.Message);
                Assert.NotEmpty(error.Message);
                Assert.NotNull(error.DeveloperMessage);
                Assert.Equal("Source System", error.ErrorSource);
            });
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Returns_Mixed_Results_When_Some_Documents_Found()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var requests = new List<DocumentLinkRequest>
                {
                    new () { DocumentId = 48508731, BookingId = "6996164" }, // valid - expected success
                    new () { DocumentId = 48508700, BookingId = "6996164" }  // invalid - expected failure
                };

            // Act
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLinkList(new RetrieveDocumentLinkListRequest() { DocumentLinkRequests = requests }, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.HolidaysDocumentLinkResponses);
            Assert.Equal(2, result.HolidaysDocumentLinkResponses.Count());

            // verify success item
            var successItem = result.HolidaysDocumentLinkResponses
                .First(x => x.Request.DocumentId == 48508731);

            Assert.NotNull(successItem.Request);
            Assert.Equal(48508731, successItem.Request.DocumentId);
            Assert.Equal("6996164", successItem.Request.BookingId);
            Assert.NotNull(successItem.Response);
            Assert.NotNull(successItem.Response.DownloadLink);
            Assert.True(Uri.TryCreate(successItem.Response.DownloadLink, UriKind.Absolute, out _));
            Assert.Equal(200, successItem.Response.ResponseStatus.HttpStatusCode);

            // verify failed item
            var failedItem = result.HolidaysDocumentLinkResponses
                .First(x => x.Request.DocumentId == 48508700);

            Assert.NotNull(failedItem.Request);
            Assert.Equal(48508700, failedItem.Request.DocumentId);
            Assert.Equal("6996164", failedItem.Request.BookingId);
            Assert.NotNull(failedItem.Response);
            Assert.Null(failedItem.Response.DownloadLink);
            Assert.Equal(500, failedItem.Response.ResponseStatus.HttpStatusCode);
            Assert.NotEmpty(failedItem.Response.ResponseStatus.Errors);
            Assert.Equal("Source System", failedItem.Response.ResponseStatus.Errors.First().ErrorSource);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Returns_Null_When_Request_Is_Empty()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var requests = new List<DocumentLinkRequest>();

            // Act
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLinkList(new RetrieveDocumentLinkListRequest() { DocumentLinkRequests = requests }, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Returns_Null_When_Request_Is_Null()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            // Act
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLinkList(null, CancellationToken.None);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Returns_Results_When_BookingId_Is_Null()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var requests = new List<DocumentLinkRequest>
                {
                    new() { DocumentId = 48508731, BookingId = null },  // optional bookingId
                    new() { DocumentId = 48508700, BookingId = null }   // optional bookingId
                };

            // Act
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLinkList(new RetrieveDocumentLinkListRequest() { DocumentLinkRequests = requests }, CancellationToken.None);

            // Assert
            Assert.NotNull(result);
            Assert.NotNull(result.HolidaysDocumentLinkResponses);
            Assert.Equal(2, result.HolidaysDocumentLinkResponses.Count());

            Assert.All(result.HolidaysDocumentLinkResponses, item =>
            {
                Assert.NotNull(item.Request);
                Assert.Null(item.Request.BookingId);
                Assert.NotNull(item.Response);
                Assert.NotNull(item.Response.ResponseStatus);
            });
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLink_Throws_ArgumentNullException_When_Request_Is_Null()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await retrieveDocumentService.RetrieveDocumentDownloadLink(null, CancellationToken.None));
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLink_Throws_ArgumentNullException_When_DocumentId_Is_Null()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var request = new DocumentLinkRequest
            {
                DocumentId = default, // null for int?
                BookingId = "6996164"
            };
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
                await retrieveDocumentService.RetrieveDocumentDownloadLink(request, CancellationToken.None));
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLink_Returns_Failed_Item_When_TBX_Is_Not_Reachable()
        {
            // Arrange
            var retrieveDocumentService = _serviceProvider.GetService<IRetrieveDocumentService>()!;

            var request = new DocumentLinkRequest
            {
                DocumentId = 11,
                BookingId = "6996164"
            };

            // Act - TBX not reachable will throw HttpRequestException caught by catch block
            var result = await retrieveDocumentService.RetrieveDocumentDownloadLink(request, CancellationToken.None);

            // Assert
            Assert.NotNull(result);

            // verify request is echoed back correctly
            Assert.NotNull(result.Request);
            Assert.Equal(request.DocumentId, result.Request.DocumentId);
            Assert.Equal(request.BookingId, result.Request.BookingId);

            // verify catch block response
            Assert.NotNull(result.Response);
            Assert.Null(result.Response.DownloadLink);

            // verify response status
            Assert.NotNull(result.Response.ResponseStatus);
            Assert.Equal(500, result.Response.ResponseStatus.HttpStatusCode);
            Assert.NotNull(result.Response.ResponseStatus.Errors);
            Assert.NotEmpty(result.Response.ResponseStatus.Errors);

            // verify error details from catch block
            var error = result.Response.ResponseStatus.Errors.First();
            Assert.Equal("UnknownError", error.Code);
            Assert.NotNull(error.Message);
            Assert.NotEmpty(error.Message);
            Assert.Contains("Error Message:", error.Message);
            Assert.Contains($"{request.DocumentId}", error.Message);
            Assert.NotNull(error.DeveloperMessage);
            Assert.Contains($"{request.DocumentId}", error.DeveloperMessage);
            Assert.Equal("Source System", error.ErrorSource);
        }
    }
}
