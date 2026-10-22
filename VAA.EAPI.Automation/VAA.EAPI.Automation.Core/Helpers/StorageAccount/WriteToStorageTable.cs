using VAA.EAPI.Automation.Core.DB_Model.AzureStorageTable;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.TestDataConfig;
using VAA.EAPI.Infrastructure.AzureStorage;

namespace VAA.EAPI.Automation.Core.Helpers.StorageAccount
{
    public class WriteToAzureStorage
    {
        public static string connectionString = Vault.Default.GetSecretValue("AzureStoragePnrCreationsConnectionString");

        public static async Task AddPNRToAzureStorageTable(TestConfig config)
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>();
            tableStorageService.TableName = "PNRs";
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new CreatedPnrsTableEntity()
            {
                Origin = config.Search.Origin.ToString(),
                Destination = config.Search.Destination.ToString(),
                Journey = config.Search.JourneyType.ToString(),
                DepartureDate = DateTime.Parse(config.Book.Order.DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                CardNumber = (config.Book.Payment.PaymentType != "PayPal") ? ("`" + config.Book.Payment.CardDetails.CardNumber.ToString()) : "PayPal",
                PaymentCardType = (config.Book.Payment.PaymentType != "PayPal") ? config.Book.Payment.CardDetails.PaymentCardType.ToString() : "PayPal",
                ThreeDS = (config.Book.Payment.PaymentType != "PayPal") ? (!config.Book.Payment.CardDetails.IsThreeDs ? "Non3DS" : "3DS") : "",
                RowKey = config.Book.Order.PNR,
                Email = config.Book.Order.EmailId,
                FirstName = config.Book.Order.LeadPassengerFirstName,
                LastName = config.Book.Order.LeadPassengerLastName,
                Currency = config.Book.Cost.Currency,
                OutBoundCabinClass = config.Book.Order.OutBoundCabinClass,
                InBoundCabinClass = config.Book.Order.InBoundCabinClass,
                TotalPrice = config.Book.Cost.TotalPrice.ToString(),
                PartitionKey = config.Internal.TestCaseName,
                PRI = config.Book.Order.PRI,
                ChildPRI = config.Book.Order.ChildPRI,
                Env = Config.Environment,
                InsurancePrice = config.Book.Cost.InsurancePrice,
                SeatUpdate = config.Book.Order.FlightPassengerSeatMap,
                Passengers = config.Book.Order.PassengerParty.ToString(),
                PassengersCount = config.Book.Order.PassengersCount.ToString(),

                //Flying Club details
                FlyingClubNumber = config.Book.Order.FlyingClubNumber,

                FCPointsBeforeRedeem = int.TryParse(config.Book.Order.PreBookingFCPoints?.ToString(), out int fcBefore) ? fcBefore : null,
                FCPointsAfterRedeem = int.TryParse(config.Book.Order.FcPointsPostBooking?.ToString(), out int fcAfter) ? fcAfter : null,
                FlightPoints = int.TryParse(config.Book.Order.FlightPoints?.ToString(), out int flightPts) ? flightPts : null,
                TierCode = config.Book.Order.FqtTierCode
            });
        }

        public static async Task AddPNRsForMobileAppToAzureStorageTable(TestConfig config, string tableName = "MobileAppPnrs")
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>();
            tableStorageService.TableName = tableName;
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new CreatedPnrsTableEntity()
            {
                PartitionKey = Config.Environment,
                RowKey = config.Book.Order.PNR,
                ReservationType = config.Search.IsRewardSearch ? "AWARD" : "REVENUE",
                Origin = config.Search.Origin.ToString(),
                Destination = config.Search.Destination.ToString(),
                Journey = config.Search.JourneyType.ToString(),
                OutBoundCabinClass = config.Book.Order.OutBoundCabinClass,
                DepartureDate = DateTime.Parse(config.Book.Order.DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                IsSeats = config.Internal.IsSeats,
                IsDirect = config.Book.Order.IsDirectFlight,
                IsCodeShare = config.Book.Order.IsCodeShare,
                FlightJourney = string.Join(", ", config.Book.Order.OutboundSectorInformation.ToString(), config.Book.Order.InboundSectorInformation?.ToString() ?? string.Empty),
                FirstName = config.Book.Order.LeadPassengerFirstName,
                LastName = config.Book.Order.LeadPassengerLastName,
                Email = config.Book.Order.EmailId,
                Passengers = config.Book.Order.PassengerParty.ToString(),
                PassengersCount = config.Book.Order.PassengersCount.ToString(),
                FlightType = config.Search.FlightType.ToString(),
                Currency = config.Book.Cost.Currency,
                TotalPrice = config.Book.Cost.TotalPrice.ToString(),
                PRI = config.Book.Order.PRI.ToString(),
                FlyingClubNumber = config.Book.Order.FlyingClubNumber,
                TierCode = config.Book.Order.FqtTierCode ?? string.Empty,
                SeatUpdate = config.Book.Order.FlightPassengerSeatMap,
                PaymentCardType = config.Book.Payment.PaymentType.Is("PayPal")
                    ? "PayPal"
                    : config.Book.Payment.CardDetails.PaymentCardType.ToString(),
                CardNumber = config.Book.Payment.PaymentType.Is("PayPal")
                    ? "PayPal"
                    : MaskCard(config.Book.Payment.CardDetails.CardNumber?.ToString())
            });
        }

        public static async Task AddLoyaltyAccountPNRsForMobileAppToAzureStorageTable(TestConfig config, string tableName = "MobileAppPnrsForLoyaltyAccounts")
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>();
            tableStorageService.TableName = tableName;
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new CreatedPnrsTableEntity()
            {
                PartitionKey = Config.Environment,
                RowKey = config.Book.Order.PNR,
                ReservationType = config.Search.IsRewardSearch ? "AWARD" : "REVENUE",
                FQTProgram = config.Book.Passenger.FQTProgramme,
                FirstName = config.Book.Order.LeadPassengerFirstName,
                LastName = config.Book.Order.LeadPassengerLastName,
                Origin = config.Search.Origin.ToString(),
                Destination = config.Search.Destination.ToString(),
                Journey = config.Search.JourneyType.ToString(),
                DepartureDate = DateTime.Parse(config.Book.Order.DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                IsSeats = config.Internal.IsSeats,
                FlightType = config.Search.FlightType.ToString(),
                PassengersCount = config.Book.Order.PassengersCount.ToString(),
                FlightJourney = string.Join(", ", config.Book.Order.OutboundSectorInformation.ToString(), config.Book.Order.InboundSectorInformation?.ToString() ?? string.Empty),
                Passengers = config.Book.Order.PassengerParty.ToString(),
                OutBoundCabinClass = config.Book.Order.OutBoundCabinClass
            });
        }

        public static async Task AddOCIPNRToAzureStorageTable(TestConfig config, string tableName = "OCIPNRs")
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>();
            tableStorageService.TableName = tableName;
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new CreatedPnrsTableEntity()
            {
                Origin = config.Search.Origin.ToString(),
                Destination = config.Search.Destination.ToString(),
                Journey = config.Search.JourneyType.ToString(),
                DepartureDate = DateTime.Parse(config.Book.Order.DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                Airline = config.Book.Order.Airline,
                FlightNumber = config.Book.Order.FlightNumber,
                CardNumber = (config.Book.Payment.PaymentType != "PayPal") ? ("`" + config.Book.Payment.CardDetails.CardNumber.ToString()) : "PayPal",
                PaymentCardType = (config.Book.Payment.PaymentType != "PayPal") ? config.Book.Payment.CardDetails.PaymentCardType.ToString() : "PayPal",
                ThreeDS = (config.Book.Payment.PaymentType != "PayPal") ? (!config.Book.Payment.CardDetails.IsThreeDs ? "Non3DS" : "3DS") : "",
                RowKey = config.Book.Order.PNR,
                Email = config.Book.Order.EmailId,
                FirstName = config.Book.Order.LeadPassengerFirstName,
                LastName = config.Book.Order.LeadPassengerLastName,
                Currency = config.Book.Cost.Currency,
                OutBoundCabinClass = config.Book.Order.OutBoundCabinClass,
                InBoundCabinClass = config.Book.Order.InBoundCabinClass,
                TotalPrice = config.Book.Cost.TotalPrice.ToString(),
                PartitionKey = config.Internal.TestCaseName.ToString(),
                OrderNo = config.Book.Order.PRI.Substring(0, 20).ToString(),
                PRI = config.Book.Order.PRI.ToString(),
                Env = Config.Environment,
                FlyingClubNumber = config.Book.Order.FlyingClubNumber,
                FCPointsBeforeRedeem = int.TryParse(config.Book.Order.PreBookingFCPoints?.ToString(), out int fcBefore) ? fcBefore : null,
                FCPointsAfterRedeem = int.TryParse(config.Book.Order.FcPointsPostBooking?.ToString(), out int fcAfter) ? fcAfter : null,
                FlightPoints = int.TryParse(config.Book.Order.FlightPoints?.ToString(), out int flightPts) ? flightPts : null,
                TierCode = config.Book.Order.FqtTierCode,
                InsurancePrice = config.Book.Cost.InsurancePrice,
                SeatUpdate = config.Book.Order.FlightPassengerSeatMap,
                Passengers = config.Book.Order.PassengerParty.ToString(),
                PassengersCount = config.Book.Order.PassengersCount.ToString(),
                FlightJourney = string.Join(", ", config.Book.Order.OutboundSectorInformation.ToString(), config.Book.Order.InboundSectorInformation?.ToString() ?? string.Empty),
                CheckInStatus = string.Empty,
                BaggageSelectionType = string.Empty
            });
        }

        public static async Task AddOciMobileAppPnrsToStorageTable(TestConfig config, string tableName = "OnlineCheckInMobileAppData")
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>();
            tableStorageService.TableName = tableName;
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new CreatedPnrsTableEntity()
            {
                Origin = config.Search.Origin.ToString(),
                Destination = config.Search.Destination.ToString(),
                Journey = config.Search.JourneyType.ToString(),
                DepartureDate = DateTime.Parse(config.Book.Order.DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                Airline = config.Book.Order.Airline,
                FlightNumber = config.Book.Order.FlightNumber,
                CardNumber = (config.Book.Payment.PaymentType != "PayPal") ? ("`" + config.Book.Payment.CardDetails.CardNumber.ToString()) : "PayPal",
                PaymentCardType = (config.Book.Payment.PaymentType != "PayPal") ? config.Book.Payment.CardDetails.PaymentCardType.ToString() : "PayPal",
                ThreeDS = (config.Book.Payment.PaymentType != "PayPal") ? (!config.Book.Payment.CardDetails.IsThreeDs ? "Non3DS" : "3DS") : "",
                RowKey = config.Book.Order.PNR,
                Email = config.Book.Order.EmailId,
                FirstName = config.Book.Order.LeadPassengerFirstName,
                LastName = config.Book.Order.LeadPassengerLastName,
                Currency = config.Book.Cost.Currency,
                OutBoundCabinClass = config.Book.Order.OutBoundCabinClass,
                InBoundCabinClass = config.Book.Order.InBoundCabinClass,
                TotalPrice = config.Book.Cost.TotalPrice.ToString(),
                PartitionKey = config.Internal.TestCaseName.ToString(),
                OrderNo = config.Book.Order.PRI.Substring(0, 20).ToString(),
                PRI = config.Book.Order.PRI.ToString(),
                Env = Config.Environment,
                FlyingClubNumber = config.Book.Order.FlyingClubNumber,
                FCPointsBeforeRedeem = int.TryParse(config.Book.Order.PreBookingFCPoints?.ToString(), out int fcBefore) ? fcBefore : null,
                FCPointsAfterRedeem = int.TryParse(config.Book.Order.FcPointsPostBooking?.ToString(), out int fcAfter) ? fcAfter : null,
                FlightPoints = int.TryParse(config.Book.Order.FlightPoints?.ToString(), out int flightPts) ? flightPts : null,
                TierCode = config.Book.Order.FqtTierCode,
                InsurancePrice = config.Book.Cost.InsurancePrice,
                SeatUpdate = config.Book.Order.FlightPassengerSeatMap,
                Passengers = config.Book.Order.PassengerParty.ToString(),
                PassengersCount = config.Book.Order.PassengersCount.ToString(),
                FlightJourney = string.Join(", ", config.Book.Order.OutboundSectorInformation.ToString(), config.Book.Order.InboundSectorInformation?.ToString() ?? string.Empty),
                CheckInStatus = string.Empty,
                BaggageSelectionType = string.Empty
            });
        }

        public static async Task AddSSRToAzureStorageTable(TestConfig config, List<SSRDetail> ssrDetails)
        {
            TableStorageService<SSRTableEntity> tableStorageService = new TableStorageService<SSRTableEntity>();
            tableStorageService.TableName = "SSRDetails";
            tableStorageService.ConnectionString = connectionString;

            foreach (VAA.EAPI.Automation.Core.TestDataConfig.SSRDetail ssrDetail in ssrDetails)
            {
                await tableStorageService.InsertOrMergeAsync(new SSRTableEntity()
                {
                    // Use PNR and SSRCode combination as RowKey to ensure uniqueness
                    RowKey = $"{config.Book.Order.PNR}_{ssrDetail.SSRCode}_{Guid.NewGuid()}",
                    // Use test case name as PartitionKey
                    PartitionKey = config.Internal.TestCaseName.ToString(),
                    // Booking details
                    PNR = config.Book.Order.PNR,
                    // SSR specific details
                    PassengerResourceId = ssrDetail.PassengerResourceId,
                    GivenNames = ssrDetail.GivenNames,
                    Surname = ssrDetail.Surname,
                    FlightNumber = ssrDetail.FlightNumber,
                    Origin = ssrDetail.Origin,
                    Destination = ssrDetail.Destination,
                    SSRCode = ssrDetail.SSRCode,
                    SSRName = ssrDetail.SSRName,
                    IsSelected = ssrDetail.IsSelected
                });
            }
        }

        public static async Task AddPNRsForSeatScenariosToAzureStorageTable(TestConfig config, string tableName = "MobileAppPnrsForSeatScenarios")
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>();
            tableStorageService.TableName = tableName;
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new CreatedPnrsTableEntity()
            {
                PartitionKey = Config.Environment,
                RowKey = config.Book.Order.PNR,
                ReservationType = config.Search.IsRewardSearch ? "AWARD" : "REVENUE",
                FirstName = config.Book.Order.LeadPassengerFirstName,
                LastName = config.Book.Order.LeadPassengerLastName,
                Origin = config.Search.Origin.ToString(),
                Destination = config.Search.Destination.ToString(),
                Journey = config.Search.JourneyType.ToString(),
                DepartureDate = DateTime.Parse(config.Book.Order.DepartureDate).ToString("yyyy-MM-ddTHH:mm:ss", System.Globalization.CultureInfo.InvariantCulture),
                IsSeats = config.Internal.IsSeats,
                FlyingClubNumber = config.Book.Order.FlyingClubNumber,
                TierCode = config.Book.Order.FqtTierCode ?? string.Empty,
                FlightType = config.Search.FlightType.ToString(),
                PassengersCount = config.Book.Order.PassengersCount.ToString(),
                FlightJourney = string.Join(", ", config.Book.Order.OutboundSectorInformation.ToString(), config.Book.Order.InboundSectorInformation?.ToString() ?? string.Empty),
                Passengers = config.Book.Order.PassengerParty.ToString(),
                OutBoundCabinClass = config.Book.Order.OutBoundCabinClass,
                Currency = config.Book.Cost.Currency,
                TotalPrice = config.Book.Cost.TotalPrice.ToString(),
                PRI = config.Book.Order.PRI,
                PaymentCardType = config.Book.Payment.PaymentType.Is("PayPal")
                    ? "PayPal"
                    : config.Book.Payment.CardDetails.PaymentCardType.ToString(),
                CardNumber = config.Book.Payment.PaymentType.Is("PayPal")
                    ? "PayPal"
                    : MaskCard(config.Book.Payment.CardDetails.CardNumber?.ToString())
            });
        }
        private static string MaskCard(string? number)
        {
            if (string.IsNullOrEmpty(number))
                return "XXXXXXXX";
            return "XXXXXXXX" + number.Substring(Math.Max(0, number.Length - 8));
        }

        public static async Task AddSeatConfirmationDataForPaymentTestingToStorageTable(Dictionary<string, object> testData)
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>();
            tableStorageService.TableName = "SeatsTestDataForDownstreamValidations";
            tableStorageService.ConnectionString = connectionString;

            await tableStorageService.InsertOrMergeAsync(new CreatedPnrsTableEntity()
            {
                PartitionKey = Config.Environment,
                RowKey = testData["PNR"].ToString(),
                ReservationType = "REVENUE",
                PassengersCount = testData["PassengerCount"].ToString(),
                SeatConfirmationResponse = testData["SeatConfirmationJson"].ToString(),
                TotalPrice = testData["TotalAmount"].ToString(),
                PRI = testData["PaymentReferenceId"].ToString(),
                IsSeats = true
            });
        }

        public static async Task UpdatePNRCheckInStatusToAzureStorageTable(string partitionKey, string pnrLocator, string checkInStatus, string baggageSelectionType = "")
        {
            TableStorageService<CreatedPnrsTableEntity> tableStorageService = new TableStorageService<CreatedPnrsTableEntity>
            {
                TableName = "OCIPNRs",
                ConnectionString = connectionString
            };

            try
            {
                // Retrieve the existing entity
                CreatedPnrsTableEntity entity = await tableStorageService.RetrieveAsync(partitionKey: partitionKey, rowKey: pnrLocator)
                             ?? throw new InvalidOperationException($"PNR '{pnrLocator}' not found in table storage.");

                // Update only the required field
                entity.CheckInStatus = checkInStatus;

                if (!baggageSelectionType.IsNullOrEmpty())
                {
                    entity.BaggageSelectionType = baggageSelectionType;
                }

                // Persist changes
                await tableStorageService.InsertOrMergeAsync(entity);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException(
                    $"Error updating check-in status for PNR '{pnrLocator}'.", ex);
            }
        }
    }
}
