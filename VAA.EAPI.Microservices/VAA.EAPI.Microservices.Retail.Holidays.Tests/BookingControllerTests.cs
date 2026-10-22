using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Text;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Core.Xunit.Fixtures;
using VAA.EAPI.Infrastructure.Interfaces;
using VAA.EAPI.Infrastructure.Services;
using VAA.EAPI.Microservices.Retail.Holidays.Application;
using VAA.EAPI.Microservices.Retail.Holidays.Domain;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Documents;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips;
using VAA.EAPI.Microservices.Retail.Holidays.Triggers.Http;

namespace VAA.EAPI.Microservices.Retail.Holidays.Tests
{
    public class BookingControllerTests : IClassFixture<HttpClientFactoryFixture>
    {
        private readonly IServiceProvider _serviceProvider;

        public BookingControllerTests(HttpClientFactoryFixture httpClientFactoryFixture)
        {
            ConfigReader.Set("Holidays:RetrieveBookingService:Url", "https://teste-ecs.tbox-test.vholsinternal.co.uk/cart-service/v2/cart/");
            ConfigReader.Set("Holidays:RetrieveDocumentService:Url", "https://teste-rest.tbox-test.vholsinternal.co.uk/document-service/v2/");
            ConfigReader.Set("Holidays:EntraScope", "testResource");
            ConfigReader.Set("Holidays:AuthorizationProvider", "AZURE");
            ConfigReader.Set("Holidays:DocumentDownloadAllowedHosts", "vsolpublic.virginholidays.co.uk");
            ConfigReader.Set("EapiMsClientID", Guid.NewGuid().ToString());
            ConfigReader.Set("EapiMSSecret", Guid.NewGuid().ToString());
            ConfigReader.Set("AAD:Authority", Guid.NewGuid().ToString());
            ConfigReader.Set("AAD:TenantId", Guid.NewGuid().ToString());

            _serviceProvider = new ServiceCollection().AddLogging()
                                                      .AddSingleton<IHttpClientFactory>(httpClientFactoryFixture.Current)
                                                      .AddSingleton<IHttpConnector, HttpConnector>()
                                                      .AddSingleton<IOAuthTokenProvider, Core.Xunit.Azure.OAuthTokenProvider>()
                                                      .AddSingleton<IRetrieveBookingService, RetrieveBookingService>()
                                                      .AddSingleton<IRetrieveDocumentService, RetrieveDocumentService>()
                                                      .AddSingleton<BookingController>()
                                                      .BuildServiceProvider();

            AsyncContext.Current = new() { Context = new Core.Functions.TestFunctionContext() };
        }
        
        [Fact]
        public void Constructor_Throws_When_RetrieveBookingService_IsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BookingController(null, _serviceProvider.GetService<IRetrieveDocumentService>()));
        }

        [Fact]
        public void Constructor_Throws_When_RetrieveDocumentService_IsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new BookingController(_serviceProvider.GetService<IRetrieveBookingService>(), null));
        }

        [Theory]
        [InlineData("")]
        [InlineData("not-json")]
        [InlineData("null")]
        public async Task RetrieveDocument_Returns_BadRequest_When_Body_Is_Invalid(string body)
        {
            var controller = _serviceProvider.GetRequiredService<BookingController>();
            var request = new Core.Functions.TestHttpRequestData(
                new Core.Functions.TestFunctionContext(),
                new Uri("http://localhost/retrieveDocument"),
                new MemoryStream(Encoding.UTF8.GetBytes(body)),
                "POST");

            var result = await controller.RetrieveDocument(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveDocument_Returns_Ok_When_Document_Is_Retrieved()
        {
            var controller = _serviceProvider.GetRequiredService<BookingController>();
            var requestBody = new RetrieveDocumentDownloadRequest
            {
                BookingId = "booking-123",
                DocumentId = "document-123",
                DocumentUrl = "https://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-123/INVOICE/document-123.pdf"
            };
            var request = new Core.Functions.TestHttpRequestData(
                new Core.Functions.TestFunctionContext(),
                new Uri("http://localhost/retrieveDocument"),
                new MemoryStream(Encoding.UTF8.GetBytes(Json.Serialize(requestBody))),
                "POST");

            var result = await controller.RetrieveDocument(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Theory]
        [InlineData(null, HttpStatusCode.BadRequest)]
        [InlineData("https://127.0.0.1/document-123.pdf", HttpStatusCode.InternalServerError)]
        public async Task RetrieveDocument_Returns_Controlled_Status_When_Service_Rejects_Request(string? documentUrl, HttpStatusCode expectedStatus)
        {
            var controller = _serviceProvider.GetRequiredService<BookingController>();
            var requestBody = new RetrieveDocumentDownloadRequest
            {
                BookingId = "booking-123",
                DocumentId = "document-123",
                DocumentUrl = documentUrl
            };
            var request = new Core.Functions.TestHttpRequestData(
                new Core.Functions.TestFunctionContext(),
                new Uri("http://localhost/retrieveDocument"),
                new MemoryStream(Encoding.UTF8.GetBytes(Json.Serialize(requestBody))),
                "POST");

            var result = await controller.RetrieveDocument(request, CancellationToken.None);

            Assert.Equal(expectedStatus, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveDocument_Returns_Ok_When_TravelBox_FileName_Is_Opaque()
        {
            var controller = _serviceProvider.GetRequiredService<BookingController>();
            var requestBody = new RetrieveDocumentDownloadRequest
            {
                BookingId = "booking-123",
                DocumentId = "document-456",
                DocumentUrl = "https://vsolpublic.virginholidays.co.uk/document-access-api/document/booking-123/INVOICE/document-123.pdf"
            };
            var request = new Core.Functions.TestHttpRequestData(
                new Core.Functions.TestFunctionContext(),
                new Uri("http://localhost/retrieveDocument"),
                new MemoryStream(Encoding.UTF8.GetBytes(Json.Serialize(requestBody))),
                "POST");

            var result = await controller.RetrieveDocument(request, CancellationToken.None);

            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveBooking_Returns_Ok_When_Booking_Found()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();
            var requestHoliday = new TripsDetailsRequest()
            {
                BookingReference = "6996245",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "test",
                Using = RequestOptionCodeType.HOLIDAYS

            };
            
            var requestJson =Json.Serialize(requestHoliday);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            // Create a mock HttpRequestData
            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");
            

            // Act
            var result = await controller.RetrieveBooking(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveBooking_Returns_NotFound_When_Booking_NotAccessible()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestJson = "{ \"BookingReference\": \"6836036\", \"Surname\": \"TEST\", \"DateOfBirth\": \"1980-01-01\" }";
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            // Create a mock HttpRequestData
            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveBooking(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveBooking_Returns_BadRequest_When_Booking_Cancelled()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestHoliday = new TripsDetailsRequest()
            {
                BookingReference = "6836034",
                DateOfBirth = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(-30)),
                Surname = "test",
                Using = RequestOptionCodeType.HOLIDAYS

            };
            
            var requestJson = Json.Serialize(requestHoliday);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            // Create a mock HttpRequestData
            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveBooking(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveBooking_Returns_BadRequest_When_Booking_IsQuote()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestJson = "{ \"BookingReference\": \"6836033\", \"Surname\": \"TEST\", \"DateOfBirth\": \"1980-01-01\" }";
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            // Create a mock HttpRequestData
            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveBooking(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveDocumentList_Returns_Ok_When_Documents_Found()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestBody = new RetrieveDocumentRequest()
            {
                BookingReference = "6996164"
            };

            var requestJson = Json.Serialize(requestBody);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveDocumentList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveDocumentList_Returns_NotFound_When_No_Documents()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestBody = new RetrieveDocumentRequest()
            {
                BookingReference = "0000000"
            };

            var requestJson = Json.Serialize(requestBody);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveDocumentList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task RetrievePendingDocumentList_Returns_Ok_When_Documents_Found()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestBody = new RetrieveDocumentRequest()
            {
                BookingReference = "6842564"
            };

            var requestJson = Json.Serialize(requestBody);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrievePendingDocumentList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RetrievePendingDocumentList_Returns_NotFound_When_No_Documents()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestBody = new RetrieveDocumentRequest()
            {
                BookingReference = "0000000"
            };

            var requestJson = Json.Serialize(requestBody);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrievePendingDocumentList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Controller_Returns_Ok_When_Documents_Found()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestBody = new RetrieveDocumentLinkListRequest
            {
                DocumentLinkRequests =
                [
                    new DocumentLinkRequest { DocumentId = 48508731, BookingId = "6996164" },
            new DocumentLinkRequest { DocumentId = 48508732, BookingId = "6996164" }
                ]
            };

            var requestJson = Json.Serialize(requestBody);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveDocumentDownloadLinkList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Controller_Returns_NotFound_When_Request_Is_Empty()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestBody = new RetrieveDocumentLinkListRequest
            {
                DocumentLinkRequests = []
            };

            var requestJson = Json.Serialize(requestBody);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveDocumentDownloadLinkList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Controller_Returns_NotFound_When_Request_Is_Null()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestBody = new RetrieveDocumentLinkListRequest
            {
                DocumentLinkRequests = null
            };

            var requestJson = Json.Serialize(requestBody);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveDocumentDownloadLinkList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);
        }

        [Fact]
        public async Task RetrieveDocumentDownloadLinkList_Controller_Returns_Ok_When_Mixed_Results()
        {
            // Arrange
            var controller = _serviceProvider.GetService<BookingController>();

            var requestBody = new RetrieveDocumentLinkListRequest
            {
                DocumentLinkRequests =
                [
                    new DocumentLinkRequest { DocumentId = 48508731, BookingId = "6996164" }, // valid - expected success
            new DocumentLinkRequest { DocumentId = 48508700, BookingId = "6996164" }  // invalid - expected failure
                ]
            };

            var requestJson = Json.Serialize(requestBody);
            var bodyBytes = Encoding.UTF8.GetBytes(requestJson);
            var bodyStream = new MemoryStream(bodyBytes);

            var context = new Core.Functions.TestFunctionContext();
            var request = new Core.Functions.TestHttpRequestData(context, new Uri("http://localhost"), bodyStream, "POST");

            // Act
            var result = await controller.RetrieveDocumentDownloadLinkList(request);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        }
    }
}
