using System.Globalization;
using Microsoft.Azure.Cosmos;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using ScvCustomerDetail = VAA.EAPI.Automation.Core.DB_Model.Entities.Order.CustomerDetail;
using ScvHistoricTripOrder = VAA.EAPI.Automation.Core.DB_Model.Entities.Order.Order;
using ScvSegmentDetail = VAA.EAPI.Automation.Core.DB_Model.Entities.Order.SegmentDetail;

namespace VAA.EAPI.Automation.Trips.Helpers
{
    public sealed class ScvHistoricTripDataSource
    {
        private const string ScvAutomationKey = "TESTING-OrderDB-Automation-Key";
        private readonly CosmosFixture _cosmosFixture;

        public ScvHistoricTripDataSource(CosmosFixture cosmosFixture, string environment)
        {
            if (!environment.Is("sit"))
            {
                throw new InvalidOperationException(
                    $"SCV historic-trip test data is configured only for SIT, not '{environment}'.");
            }

            if (Config.ScvHistoricTrips.AccountEndpoint.IsNullOrWhiteSpace())
                throw new InvalidOperationException("ScvAccountEndpoint must be configured for historic-trip test data.");
            if (Config.ScvHistoricTrips.Database.IsNullOrWhiteSpace())
                throw new InvalidOperationException("ScvDatabase must be configured for historic-trip test data.");
            if (Config.ScvHistoricTrips.Collection.IsNullOrWhiteSpace())
                throw new InvalidOperationException("ScvCollection must be configured for historic-trip test data.");

            _cosmosFixture = cosmosFixture;
            string cosmosKey = Vault.Default.GetSecretValue(ScvAutomationKey);

            if (cosmosKey.IsNullOrWhiteSpace())
            {
                throw new InvalidOperationException($"Required Key Vault secret '{ScvAutomationKey}' was null or empty.");
            }

            _cosmosFixture.RegisterClient(Config.ScvHistoricTrips.AccountEndpoint, cosmosKey);
            _cosmosFixture.RegisterType<ScvHistoricTripOrder>(
                Config.ScvHistoricTrips.AccountEndpoint,
                Config.ScvHistoricTrips.Database,
                Config.ScvHistoricTrips.Collection);
        }

        public ScvHistoricTripTestData GetBooking()
        {
            List<ScvHistoricTripOrder> candidates = QueryBookings();

            foreach (ScvHistoricTripOrder order in candidates)
            {
                ScvHistoricTripTestData? testData = TryCreateTestData(order);
                if (testData is not null && HasUniqueMatch(testData))
                {
                    ScvHistoricTripOrder? exactOrder = QueryByOrderId(testData.OrderId);
                    return exactOrder is null
                        ? throw new InvalidOperationException("The selected SCV order could not be retrieved by its exact OrderID.")
                        : TryCreateTestData(exactOrder)
                            ?? throw new InvalidOperationException("The exact SCV order no longer satisfies the historic-trip lookup rules.");
                }
            }

            int missingCustomerCount = candidates.Count(order =>
                order.CustomerDetails?.Any(customer =>
                    !customer.FirstName.IsNullOrWhiteSpace() && !customer.LastName.IsNullOrWhiteSpace()) != true);
            int groupBookingCount = candidates.Count(order =>
                order.CustomerDetails?.Any(customer => customer.Bookingtype?.Trim().Is("group") == true) == true);
            int missingFlightFieldsCount = candidates.Count(order =>
                order.SegmentDetails?.Any(segment =>
                    !segment.MarketingFlightNumber.IsNullOrWhiteSpace() &&
                    !segment.OriginAirportCode.IsNullOrWhiteSpace() &&
                    !segment.DestinationAirportCode.IsNullOrWhiteSpace()) != true);
            int invalidCompositeIdCount = candidates.Count(order =>
                order.OrderID.IsNullOrWhiteSpace() ||
                order.OrderID.Length <= 7 ||
                !DateTime.TryParseExact(order.OrderID[^7..], "ddMMMyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime _));

            throw new InvalidOperationException(
                $"No usable SCV booking was found among {candidates.Count} candidates. " +
                $"Aggregate rejections: missingCustomer={missingCustomerCount}, groupBooking={groupBookingCount}, " +
                $"missingFlightFields={missingFlightFieldsCount}, invalidCompositeId={invalidCompositeIdCount}. " +
                "The record must have a valid composite OrderID, passenger and local departure data, and one unambiguous match.");
        }

        private List<ScvHistoricTripOrder> QueryBookings()
        {
            QueryDefinition query = new QueryDefinition(
                "SELECT TOP 1000 * FROM c WHERE IS_STRING(c.OrderID) " +
                "AND LENGTH(c.OrderID) > 7 AND ARRAY_LENGTH(c.CustomerDetails) > 0");

            return _cosmosFixture.CosmosQueryItems<ScvHistoricTripOrder>(query, maxItemCount: 1000).ToList();
        }

        private bool HasUniqueMatch(ScvHistoricTripTestData testData)
        {
            string flightDate = DateTime.ParseExact(testData.FlightDate, "ddMMMyy", CultureInfo.InvariantCulture)
                .ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + "T";
            QueryDefinition query = new QueryDefinition(
                "SELECT c.OrderID FROM c WHERE c.DocType = @docType AND STARTSWITH(c.OrderID, @pnr) " +
                "AND EXISTS (SELECT VALUE s FROM s IN c.SegmentDetails WHERE STARTSWITH(s.ScheduledDepartureTime.Local_Date_Time, @flightDate)) " +
                "AND EXISTS (SELECT VALUE p FROM p IN c.CustomerDetails WHERE STRINGEQUALS(TRIM(p.FirstName), @firstName, true) AND STRINGEQUALS(TRIM(p.LastName), @lastName, true))")
                .WithParameter("@docType", "Order")
                .WithParameter("@pnr", testData.Pnr.Trim().ToUpperInvariant())
                .WithParameter("@flightDate", flightDate)
                .WithParameter("@firstName", testData.GivenName.Trim())
                .WithParameter("@lastName", testData.Surname.Trim());
            List<ScvHistoricTripOrder> matches = _cosmosFixture.CosmosQueryItems<ScvHistoricTripOrder>(query).ToList();
            return matches.Count == 1 && matches[0].OrderID == testData.OrderId;
        }

        private ScvHistoricTripOrder? QueryByOrderId(string orderId)
        {
            QueryDefinition query = new QueryDefinition(
                    "SELECT * FROM c WHERE c.OrderID = @orderId")
                .WithParameter("@orderId", orderId);

            return _cosmosFixture.CosmosQueryItems<ScvHistoricTripOrder>(query).FirstOrDefault();
        }

        private static ScvHistoricTripTestData? TryCreateTestData(
            ScvHistoricTripOrder order)
        {
            if (order.OrderID.IsNullOrWhiteSpace() ||
                order.CustomerDetails is null ||
                order.CustomerDetails.Any(customer => customer.Bookingtype?.Trim().Is("group") == true) ||
                order.SegmentDetails is null ||
                order.SegmentDetails.Count == 0)
            {
                return null;
            }

            ScvCustomerDetail? customer = order.CustomerDetails.FirstOrDefault(passenger =>
                !passenger.FirstName.IsNullOrWhiteSpace() && !passenger.LastName.IsNullOrWhiteSpace());
            ScvSegmentDetail? segment = order.SegmentDetails.FirstOrDefault(item =>
                !item.MarketingFlightNumber.IsNullOrWhiteSpace() &&
                !item.OriginAirportCode.IsNullOrWhiteSpace() &&
                !item.DestinationAirportCode.IsNullOrWhiteSpace() &&
                DateTime.TryParse(item.ScheduledDepartureTime?.Local_Date_Time, CultureInfo.InvariantCulture, DateTimeStyles.None, out _));

            if (customer is null || segment is null || order.OrderID.Length <= 7)
            {
                return null;
            }

            string pnr = order.OrderID[..^7];
            string bookingDate = order.OrderID[^7..];
            if (!DateTime.TryParseExact(
                    bookingDate,
                    "ddMMMyy",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out _))
            {
                return null;
            }

            return new ScvHistoricTripTestData
            {
                OrderId = order.OrderID,
                Pnr = pnr,
                FlightDate = DateTime.Parse(segment.ScheduledDepartureTime.Local_Date_Time, CultureInfo.InvariantCulture).ToString("ddMMMyy", CultureInfo.InvariantCulture).ToUpperInvariant(),
                GivenName = customer.FirstName,
                Surname = customer.LastName,
                MarketingFlightNumber = segment.MarketingFlightNumber,
                OriginAirportCode = segment.OriginAirportCode,
                DestinationAirportCode = segment.DestinationAirportCode
            };
        }

    }
}
