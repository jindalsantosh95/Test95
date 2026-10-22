using Microsoft.Extensions.DependencyInjection;
using VAA.EAPI.Domain.Entities.Common;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Retail.Order.Data.Application.Query.SvcOrderStore;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.FlightBookings.Entity;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public sealed class ScvBookingQueryHandlerTests : IDisposable
    {
        [Theory]
        [InlineData("ActualStartDeliveryDateTime")]
        [InlineData("PlannedStartDeliveryDateTime")]
        [InlineData("ActualEndDeliveryDateTime")]
        [InlineData("PlannedEndDeliveryDateTime")]
        public void StorageModel_PreservesPersistedAncillaryDeliveryTimestamps(string timestampProperty)
        {
            var storedDocument = JObject.Parse($$"""
                { "OrderID": "ABC12331AUG25",
                  "CustomerDetails": [{ "FirstName": "Jane", "LastName": "Doe" }],
                  "SegmentDetails": [{ "ScheduledArrivalTime": { "UTC_Date_Time": "2026-10-01T15:00:00Z" } }],
                  "AncillaryOrders": [{ "{{timestampProperty}}": {
                    "UTC_Date_Time": "2026-10-01T15:00:00Z",
                    "Local_Date_Time": "2026-10-01T16:00:00"
                  } }] }
                """);
            var booking = Json.Deserialize<OrderDoc>(Json.Serialize(storedDocument));

            var json = Json.Serialize(booking);
            var response = Json.Deserialize<JObject>(json);
            var ancillary = Assert.IsType<JObject>(Assert.Single(Assert.IsType<JArray>(response.GetValue("AncillaryOrders", StringComparison.OrdinalIgnoreCase))));
            var timestamp = Assert.IsType<JObject>(ancillary.GetValue(timestampProperty, StringComparison.OrdinalIgnoreCase));
            Assert.Equal("2026-10-01T15:00:00Z", timestamp.GetValue("UTCDateTime", StringComparison.OrdinalIgnoreCase)?.Value<string>());
            Assert.Equal("2026-10-01T16:00:00", timestamp.GetValue("LocalDateTime", StringComparison.OrdinalIgnoreCase)?.Value<string>());
            Assert.DoesNotContain("UTC_Date_Time", json);
            Assert.DoesNotContain("Local_Date_Time", json);
        }

        [Theory]
        [InlineData("2026-10-01T15:00:00Z")]
        [InlineData("2099-10-09T15:00:00Z")]
        [InlineData("")]
        public async Task Lookup_ReadsPersistedScvTimestampNamesWithoutFilteringBookings(string arrival)
        {
            var storedDocument = JObject.Parse($$"""
                { "OrderID": "ABC12331AUG25",
                  "CustomerDetails": [{ "FirstName": "Jane", "LastName": "Doe" }],
                  "OrderDetails": {
                    "CreationDateTime": { "UTC_Date_Time": "2025-08-31T09:00:00Z" },
                    "AmendedDateTime": { "UTC_Date_Time": "2025-09-01T09:00:00Z" },
                    "CancelledDateTime": { "Local_Date_Time": "" }
                  },
                  "SegmentDetails": [{
                    "ScheduledArrivalTime": { "UTC_Date_Time": "{{arrival}}", "Local_Date_Time": "2026-10-01T16:00:00" },
                    "ScheduledDepartureTime": { "UTC_Date_Time": "2026-10-01T10:00:00Z", "Local_Date_Time": "2026-10-01T11:00:00" }
                  }] }
                """);
            // Use the same JSON round-trip as SvcOrderRepository, including date-valued JTokens.
            var booking = Json.Deserialize<OrderDoc>(Json.Serialize(storedDocument));
            Assert.Equal(arrival, Assert.Single(booking.SegmentDetails).ScheduledArrivalTime.UTCDateTime);
            Assert.Equal("2026-10-01T16:00:00", Assert.Single(booking.SegmentDetails).ScheduledArrivalTime.LocalDateTime);
            Assert.Equal("2026-10-01T10:00:00Z", Assert.Single(booking.SegmentDetails).ScheduledDepartureTime.UTCDateTime);
            Assert.Equal("2025-08-31T09:00:00Z", booking.OrderDetails.CreationDateTime.UTCDateTime);
            Assert.Equal("2025-09-01T09:00:00Z", booking.OrderDetails.AmendedDateTime.UTCDateTime);
            Assert.Equal("", booking.OrderDetails.CancelledDateTime.LocalDateTime);

            var result = await CreateHandler(new BookingRepository([booking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Single(result.Value);
            Assert.DoesNotContain("UTC_Date_Time", Json.Serialize(booking));
            Assert.DoesNotContain("Local_Date_Time", Json.Serialize(booking));
        }

        [Fact]
        public async Task Lookup_MapsPassengersAndFlightsToTheExistingTripResponse()
        {
            var booking = MatchingBooking();
            booking.SegmentDetails = [new SegmentDetail
            {
                SegmentNumber = "1", ProductId = "flight-1", OriginAirportCode = "LHR", DestinationAirportCode = "JFK",
                MarketingCarrier = "VS", MarketingFlightNumber = "3", OperatingCarrier = "VS", OperatingFlightNumber = "3",
                ScheduledDepartureTime = new ScheduledDepartureTime { UTCDateTime = "2026-10-01T10:00:00Z", LocalDateTime = "2026-10-01T11:00:00" },
                ScheduledArrivalTime = new ScheduledArrivalTime { UTCDateTime = "2026-10-01T15:00:00Z", LocalDateTime = "2026-10-01T11:00:00" },
                Legs = [Json.Deserialize<Leg>("""{ "OriginAirportCode": "LHR", "Destination_Airport_Code": "JFK", "ScheduledArrivalTime": { "UTC_Date_Time": "2026-10-01T15:00:00Z" } }""")]
            }];
            var result = await CreateHandler(new BookingRepository([booking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            var response = Assert.Single(result.Value);
            Assert.Equal("Jane", Assert.Single(response.Passengers!).FirstName);
            var product = Assert.Single(response.Product!);
            Assert.Equal("ABC123", product.Reference?.PnrLocator);
            Assert.Equal(new DateTime(2025, 8, 31), product.Reference?.BookingDate);
            var slice = Assert.Single(product.Slices!);
            Assert.False(slice.CheckInEligible);
            var segment = Assert.Single(slice.Segments!);
            Assert.Equal("LHR", segment.Origin?.Code);
            Assert.Equal("JFK", segment.Destination?.Code);
            Assert.Equal("3", segment.MarketingCarrier?.FlightNumber);
            Assert.Equal(new DateTime(2026, 10, 1, 15, 0, 0, DateTimeKind.Utc), segment.ScheduledArrivalUtcDateTime);
            Assert.Equal(new DateTime(2026, 10, 1, 11, 0, 0), segment.ScheduledDepartureLocalDateTime);
            Assert.Equal("JFK", Assert.Single(segment.Legs!).Destination?.Code);
        }

        [Fact]
        public async Task Lookup_ExcludesAncillaryPaymentRecordsAndPaymentTypes()
        {
            var booking = MatchingBooking();
            booking.AncillaryOrders = [new AncillaryOrder
            {
                ServicePayments = [new ServicePayment { PurchaseId = "payment-only" }],
                AncillaryOrderItems = [new AncillaryOrderItem { FormOfPaymentTypeCode = "payment-only", FormOfPaymentCategoryCode = "payment-only", PurchaseId = "payment-only" }]
            }];
            booking.Fulfillment = new Fulfillment { Tickets = [new Ticket { FormOfPaymenttype = "payment-only" }] };

            var result = await CreateHandler(new BookingRepository([booking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            var json = Json.Serialize(Assert.Single(result.Value));
            Assert.DoesNotContain("payment-only", json);
            var properties = JObject.Parse(json).Descendants().OfType<JProperty>().Select(property => property.Name.ToLowerInvariant()).ToArray();
            foreach (var name in new[] { "servicepayments", "purchaseid", "formofpaymenttype", "formofpaymentcategorycode", "formofpaymenttypecode" })
                Assert.DoesNotContain(name, properties);
        }

        [Fact]
        public void LookupQuery_TargetsTheCompositeIdentifierInTheStoredDocument()
        {
            // Preserve the casing/shape of SCV fixtures while using synthetic data.
            var storedDocument = JObject.Parse("""
                { "DocType": "Order", "OrderID": "ABC12331AUG25",
                  "OrderDetails": { "OrderId": "ABC12331AUG25" } }
                """);
            var predicate = System.Text.RegularExpressions.Regex.Match(
                Constants.Application.ScvOrderByOrderIdQuery, @"c\.(\w+)\s*=\s*@OrderId\b");

            Assert.True(predicate.Success);
            // JObject indexing is case-sensitive, just like Cosmos property access.
            Assert.Equal("ABC12331AUG25", storedDocument[predicate.Groups[1].Value]?.Value<string>());
            Assert.Equal("ABC12331AUG25", storedDocument.ToObject<OrderDoc>()?.OrderID);
        }

        [Theory]
        [InlineData("group")]
        [InlineData(" GROUP ")]
        public async Task Lookup_MapsGroupClassificationAndAllPassengersWhenOneMatches(string bookingType)
        {
            var booking = MatchingBooking();
            booking.CustomerDetails = [
                new CustomerDetail { FirstName = "Jane", LastName = "Doe" },
                new CustomerDetail { FirstName = "Other", LastName = "Traveller", Bookingtype = bookingType }
            ];
            var result = await CreateHandler(new BookingRepository([booking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            var response = Assert.Single(result.Value);
            Assert.Equal("group", response.ReservationType);
            Assert.Equal(new[] { "Jane", "Other" }, response.Passengers!.Select(passenger => passenger.FirstName));
            Assert.Equal(new[] { "Jane", "Other" }, Assert.Single(response.Product!).Passengers!.Select(passenger => passenger.FirstName));
        }

        [Fact]
        public async Task Lookup_ProjectsExistingTripContractWithoutStorageOrPaymentData()
        {
            var booking = MatchingBooking();
            booking.Id = "storage-only";
            booking.DocType = "Order";
            booking.Rid = "storage-only";
            booking.Self = "storage-only";
            booking.Etag = "storage-only";
            booking.Attachments = "storage-only";
            booking.Ts = 42;
            booking.OrderHistorydetails = new OrderHistorydetails
            {
                HistoryActionItem = [new HistoryActionItem { ActionCode = "UPDATE", OrdinalNumber = "1", FreeformText = "payment-only" }],
                HistoryCreditOrSignature = [new HistoryCreditOrSignature { CreditText = "payment-only", OrdinalNumber = "2" }],
                HistoryPnrCreation = new HistoryPnrCreation { CreditText = "payment-only", OrdinalNumber = "3" },
                OtherServiceInformation = [new OtherServiceInformation { AirlineCode = "VS", OsiNumber = "4", FreeformText = "payment-only" }],
                Remarks = [new Remark { RemarkType = "GENERAL", RemarkNumber = "5", FreeformText = "payment-only" }]
            };
            booking.OrderDetails = new OrderDetail
            {
                AmendedDateTime = Json.Deserialize<AmendedDateTime>("""
                    { "LocalDateTime": "2026-09-01T10:00:00", "UTCDateTime": "2026-09-01T09:00:00Z" }
                    """),
                FarePrice = new FarePrice { ApprovalCode = [new { CreditCardNumber = "payment-only" }], FarePriceDetails = [new FarePriceDetail { TotalFareAmount = "100", Tax = [new { CreditCardNumber = "payment-only" }] }] },
                StoredFare = [new { CreditCardNumber = "payment-only" }],
                FareQuote = [new { CreditCardNumber = "payment-only" }]
            };
            booking.Fulfillment = new Fulfillment
            {
                Tickets = [new Ticket { TicketNumber = "SYNTHETIC-123", CreditCardNumber = "payment-only", CreditCardExpiration = "payment-only", CardHolderNameOrOther = "payment-only", FormOfPayment = "payment-only", OriginalFormOfPayment = "payment-only", ApprovalCode = "payment-only", FreeformText = "payment-only" }]
            };

            var result = await CreateHandler(new BookingRepository([booking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            var response = Assert.Single(result.Value);
            var json = Json.Serialize(response);
            var properties = JObject.Parse(json).Descendants().OfType<JProperty>().Select(property => property.Name.ToLowerInvariant()).ToArray();
            foreach (var name in new[] { "doctype", "rid", "self", "etag", "attachments", "ts", "creditcardnumber", "cardholdernameorother", "creditcardexpiration", "formofpayment", "originalformofpayment", "approvalcode", "storedfare", "farequote" })
                Assert.DoesNotContain(name, properties);
            Assert.DoesNotContain("payment-only", json);
            Assert.DoesNotContain("storage-only", json);
            Assert.DoesNotContain("freeformtext", properties);
            Assert.DoesNotContain("credittext", properties);
            Assert.IsType<VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.TripDetails.TripDetailsResponse>(response);
            Assert.Equal("ABC12331AUG25", response.Id);
            Assert.Equal("Jane", Assert.Single(response.Passengers!).FirstName);
            var product = Assert.Single(response.Product!);
            Assert.False(product.CheckInEligible);
            Assert.Single(product.Slices!);
            Assert.Equal("payment-only", Assert.Single(booking.Fulfillment.Tickets).CreditCardNumber);
        }

        [Theory]
        [InlineData("31AUG25", true)]
        [InlineData("31FEB25", false)]
        [InlineData("2025-08-31", false)]
        public void RequestValidation_EnforcesFlightDateFormat(string date, bool expected)
        {
            var request = new ScvBookingLookupRequest { Pnr = "ABC123", FlightDate = date, FirstName = "Jane", LastName = "Doe" };
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            Assert.Equal(expected, System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                request, new System.ComponentModel.DataAnnotations.ValidationContext(request), results, true));
        }

        [Theory]
        [InlineData(-1, true)]
        [InlineData(0, true)]
        [InlineData(1, true)]
        public void RequestValidation_AcceptsPastTodayAndFutureFlightDates(int days, bool expected)
        {
            var request = new ScvBookingLookupRequest
            {
                Pnr = "ABC123",
                FirstName = "Jane",
                LastName = "Doe",
                FlightDate = DateTime.UtcNow.Date.AddDays(days).ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture)
            };
            var results = new List<System.ComponentModel.DataAnnotations.ValidationResult>();
            Assert.Equal(expected, System.ComponentModel.DataAnnotations.Validator.TryValidateObject(
                request, new System.ComponentModel.DataAnnotations.ValidationContext(request), results, true));
        }

        private readonly List<ServiceProvider> _providers = [];

        private IScvBookingQuery CreateHandler(IScvBookingRepository repository)
        {
            // The shared xUnit harness does not replace the Cosmos SDK. Keep the
            // storage boundary deterministic while executing the real handler/map.
            var services = new ServiceCollection()
                .AddSingleton<IScvBookingRepository>(repository)
                .AddScoped<IScvBookingQuery, ScvBookingQueryHandler>()
                .BuildServiceProvider();
            _providers.Add(services);
            return services.GetRequiredService<IScvBookingQuery>();
        }

        public void Dispose()
        {
            foreach (var provider in _providers)
                provider.Dispose();
        }

        [Theory]
        [InlineData("2020-10-08T11:59:59Z")]
        [InlineData("2099-10-08T12:00:01Z")]
        [InlineData("2026-10-08")]
        [InlineData("invalid")]
        [InlineData("")]
        [InlineData(null)]
        public async Task Lookup_DoesNotFilterBySegmentArrival(string? finalArrival)
        {
            var booking = MatchingBooking();
            booking.SegmentDetails = [
                new SegmentDetail { ScheduledDepartureTime = new ScheduledDepartureTime { LocalDateTime = "2026-10-01T11:00:00" }, ScheduledArrivalTime = new ScheduledArrivalTime { UTCDateTime = "2020-10-01T15:00:00Z" } },
                new SegmentDetail { ScheduledDepartureTime = new ScheduledDepartureTime { LocalDateTime = "2026-10-01T11:00:00" }, ScheduledArrivalTime = new ScheduledArrivalTime { UTCDateTime = finalArrival } }
            ];
            var result = await CreateHandler(new BookingRepository([booking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Single(result.Value);
        }

        [Fact]
        public async Task Lookup_ReturnsCancelledBookingButExcludesMissingFlightDates()
        {
            var cancelled = MatchingBooking();
            cancelled.BookingCancelledIndicator = "true";
            var undated = MatchingBooking();
            undated.SegmentDetails = [];
            var missingSegments = MatchingBooking();
            missingSegments.SegmentDetails = null;
            var result = await CreateHandler(new BookingRepository([cancelled, undated, missingSegments]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Single(result.Value);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(1)]
        public async Task Lookup_AcceptsPastTodayAndFutureFlightDates(int days)
        {
            var departure = DateTime.UtcNow.Date.AddDays(days);
            var booking = MatchingBooking();
            booking.SegmentDetails.First().ScheduledDepartureTime.LocalDateTime = departure.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture) + "T11:00:00";
            var result = await CreateHandler(new BookingRepository([booking])).HandleAsync("ABC123",
                departure.ToString("ddMMMyy", System.Globalization.CultureInfo.InvariantCulture), "Jane", "Doe", CancellationToken.None);
            Assert.Single(result.Value);
        }

        [Fact]
        public async Task Lookup_ExcludesSamePnrWithDifferentFlightDate()
        {
            var otherBooking = MatchingBooking();
            otherBooking.OrderID = "ABC12330AUG25";
            otherBooking.SegmentDetails.First().ScheduledDepartureTime.LocalDateTime = "2026-10-02T11:00:00";
            var result = await CreateHandler(new BookingRepository([MatchingBooking(), otherBooking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Equal("ABC12331AUG25", Assert.Single(result.Value).Id);
        }

        [Theory]
        [InlineData("31FEB25")]
        [InlineData("2025-08-31")]
        [InlineData(" ")]
        public async Task Lookup_RejectsInvalidFlightDateBeforeReadingRepository(string date)
        {
            var repository = new BookingRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => CreateHandler(repository)
                .HandleAsync("ABC123", date, "Jane", "Doe", CancellationToken.None));
            Assert.Null(repository.Pnr);
        }

        private static OrderDoc MatchingBooking() => new()
        {
            OrderID = "ABC12331AUG25",
            CustomerDetails = [new CustomerDetail { FirstName = "Jane", LastName = "Doe" }],
            SegmentDetails = [new SegmentDetail { ScheduledDepartureTime = new ScheduledDepartureTime { LocalDateTime = "2026-10-01T11:00:00" }, ScheduledArrivalTime = new ScheduledArrivalTime { UTCDateTime = "2026-10-01T15:00:00Z" } }]
        };

        [Fact]
        public async Task Lookup_NormalizesPnrAndMatchesNamesOnSamePassenger()
        {
            var booking = new OrderDoc
            {
                OrderID = "ABC12331AUG25",
                CustomerDetails = [new CustomerDetail { FirstName = "Jane", LastName = "Doe" }],
                SegmentDetails = [new SegmentDetail { ScheduledDepartureTime = new ScheduledDepartureTime { LocalDateTime = "2026-10-01T11:00:00" }, ScheduledArrivalTime = new ScheduledArrivalTime { UTCDateTime = "2026-10-01T15:00:00Z" } }],
                Fulfillment = new Fulfillment()
            };
            var repository = new BookingRepository([booking]);
            using var cancellation = new CancellationTokenSource();

            var result = await CreateHandler(repository).HandleAsync(" abc123 ", " 01oct26 ", " jane ", " DOE ", cancellation.Token);

            Assert.Equal(Status.OK, result.Status);
            var response = Assert.Single(result.Value);
            Assert.Equal(booking.OrderID, response.Id);
            Assert.Equal("Jane", Assert.Single(response.Passengers!).FirstName);
            Assert.Single(Assert.Single(response.Product!).Slices!);
            Assert.Equal("ABC123", repository.Pnr);
            Assert.Equal(cancellation.Token, repository.CancellationToken);
        }

        [Fact]
        public async Task Lookup_DoesNotMatchNamesAcrossPassengersOrReturnOtherReusedPnrs()
        {
            var differentPassengers = MatchingBooking();
            differentPassengers.CustomerDetails = [new CustomerDetail { FirstName = "Jane", LastName = "Smith" }, new CustomerDetail { FirstName = "John", LastName = "Doe" }];
            var missingPassengers = MatchingBooking();
            missingPassengers.CustomerDetails = null;
            var wrongPassenger = MatchingBooking();
            wrongPassenger.CustomerDetails = [new CustomerDetail { FirstName = "Other", LastName = "Person" }];
            var repository = new BookingRepository([differentPassengers, missingPassengers, wrongPassenger]);

            var result = await CreateHandler(repository).HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);

            Assert.Empty(result.Value);
        }

        [Fact]
        public async Task Lookup_ReturnsEmptyListWhenMissing()
        {
            var result = await CreateHandler(new BookingRepository([])).HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Equal(Status.OK, result.Status);
            Assert.Empty(result.Value);
        }

        [Theory]
        [InlineData(" ", "Jane", "Doe")]
        [InlineData("ABC123", " ", "Doe")]
        [InlineData("ABC123", "Jane", " ")]
        public async Task Lookup_RejectsMissingCredentialsBeforeReadingRepository(string pnr, string firstName, string lastName)
        {
            var repository = new BookingRepository([]);
            await Assert.ThrowsAsync<ArgumentException>(() => CreateHandler(repository).HandleAsync(pnr, "31AUG25", firstName, lastName, CancellationToken.None));
            Assert.Null(repository.Pnr);
        }

        [Theory]
        [InlineData("ABC12330AUG25")]
        [InlineData("ABC12331AUG25")]
        public async Task Lookup_ReturnsNothingForMultipleMatchingBookings(string secondOrderId)
        {
            var second = MatchingBooking();
            second.OrderID = secondOrderId;
            var repository = new BookingRepository([MatchingBooking(), second]);
            var result = await CreateHandler(repository)
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Empty(result.Value);
            Assert.Equal(0, repository.FullReadCount);
        }

        [Theory]
        [InlineData("2026-10-01T00:30:00", true)]
        [InlineData("2026-09-30T23:30:00", false)]
        [InlineData(null, false)]
        [InlineData("", false)]
        public async Task Lookup_UsesLocalDepartureDateInsteadOfUtcOrArrival(string? localDeparture, bool expected)
        {
            var booking = MatchingBooking();
            booking.SegmentDetails.First().ScheduledDepartureTime = new ScheduledDepartureTime
            {
                LocalDateTime = localDeparture,
                UTCDateTime = "2026-09-30T23:30:00Z"
            };
            var result = await CreateHandler(new BookingRepository([booking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Equal(expected ? 1 : 0, result.Value.Count);
        }

        [Fact]
        public async Task Lookup_AcceptsAnyMatchingSegmentOnlyOnce()
        {
            var booking = MatchingBooking();
            var segment = Assert.Single(booking.SegmentDetails);
            booking.SegmentDetails = [new SegmentDetail(), segment, segment];
            var result = await CreateHandler(new BookingRepository([booking]))
                .HandleAsync("ABC123", "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Single(result.Value);
        }

        [Theory]
        [InlineData("A")]
        [InlineData("ABC")]
        [InlineData(" abc ")]
        [InlineData("ABC1234")]
        public async Task Lookup_RejectsInvalidPnrLengthBeforeReadingRepository(string pnr)
        {
            var repository = new BookingRepository([MatchingBooking()]);
            var result = await CreateHandler(repository)
                .HandleAsync(pnr, "01OCT26", "Jane", "Doe", CancellationToken.None);
            Assert.Empty(result.Value);
            Assert.Equal(0, repository.ReadCount);
        }

        private sealed class BookingRepository(IReadOnlyCollection<OrderDoc> bookings) : IScvBookingRepository
        {
            public int ReadCount { get; private set; }
            public int FullReadCount { get; private set; }
            public string? Pnr { get; private set; }
            public CancellationToken CancellationToken { get; private set; }

            public Task<IReadOnlyCollection<string>> GetOrderIdsByPnrAsync(string pnr, CancellationToken cancellationToken)
            {
                ReadCount++;
                Pnr = pnr;
                CancellationToken = cancellationToken;
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult<IReadOnlyCollection<string>>(bookings.Select(booking => booking.OrderID).ToArray());
            }

            public Task<IReadOnlyCollection<string>> GetMatchingOrderIdsAsync(string orderId, string flightDate, string firstName, string lastName, CancellationToken cancellationToken)
            {
                ReadCount++;
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult<IReadOnlyCollection<string>>(bookings.Where(booking => booking.OrderID == orderId &&
                    booking.SegmentDetails?.Any(segment => segment.ScheduledDepartureTime?.LocalDateTime?.StartsWith(flightDate + "T", StringComparison.Ordinal) == true) == true &&
                    booking.CustomerDetails?.Any(customer => customer.FirstName?.Trim().ToUpperInvariant() == firstName.ToUpperInvariant() &&
                        customer.LastName?.Trim().ToUpperInvariant() == lastName.ToUpperInvariant()) == true)
                    .Select(booking => booking.OrderID).ToArray());
            }

            public Task<IReadOnlyCollection<OrderDoc>> GetByOrderIdAsync(string pnr, CancellationToken cancellationToken)
            {
                ReadCount++;
                FullReadCount++;
                CancellationToken = cancellationToken;
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult<IReadOnlyCollection<OrderDoc>>(bookings.Where(booking => booking.OrderID == pnr).ToArray());
            }
        }
    }
}
