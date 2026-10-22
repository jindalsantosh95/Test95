using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Models.BackendAPI;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.LoopsBackendAPITests
{
    [Collection("TransactionAPITests")]
    [TestCaseOrderer("VAA.EAPI.Automation.Core.Xunit.TestCaseRunOrder", "VAA.EAPI.Automation.Core")]
    public class TransactionAPITests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {
        private readonly TokenFixture tokenFixture;

        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerCosmosDb;
        private readonly string customerTransactionCosmosDb;

        private static string backendAPIEnv = ConfigReader.Get("BackendAPIEnv") ?? "";

        public TransactionAPITests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {
            this.tokenFixture = tokenFixture;

            this.cosmosFixture = cosmosFixture;

            if (!backendAPIEnv.Is("Project-DEV"))
                this.tokenFixture.AddTokenForTransactionApiFromKeyVault("transaction");

            customerCosmosDb = env.Is("uat") ? Application.CustomerCosmosDbUAT : Application.CustomerCosmosDbSIT;
            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));

            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            this.cosmosFixture.RegisterType<FlyingClubMember>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
        }

        [Trait("Category", Traits.LOOPSTransactionAPIRegression)]
        [Fact, RunOrder(1)]
        public async Task TC01_RetrieveRedemptionTransactionUsingCustomerIdandTransactionId()
        {
            #region Test Data

            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @transactionType and c.ProductId = @productId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction").WithParameter("@transactionType", "Redemption").WithParameter("@productId", "1-1Z9X");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            string customerId = txnDoc.CustomerId;

            string transactionId = txnDoc.TransactionId;

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Transaction.RetrieveRedemptionTransaction(tokenFixture.GetToken("transaction"), customerId, transactionId).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            RedemptionTransaction? transactionDoc = JsonConvert.DeserializeObject<RedemptionTransaction>(result[0].ToString());

            #endregion

            #region Validations

            Assert.True(txnDoc.MemberNumber.Equals(transactionDoc.MemberNumber),
                $"For the transaction Id {transactionId} , Expected the MemberNumber to be {txnDoc.MemberNumber} but was {transactionDoc.MemberNumber}");

            Assert.True(txnDoc.TransactionType.Equals(transactionDoc.TransactionType),
                $"For the transaction Id {transactionId} , Expected the Transaction Type to be {txnDoc.TransactionType} but was {transactionDoc.TransactionType}");

            Assert.True(txnDoc.TransactionDate.Value.ToString("yyyy-MM-dd").Equals(Convert.ToDateTime(transactionDoc.TransactionProcessedDate).ToString("yyyy-MM-dd")),
                $"For the transaction Id {transactionId} , Expected the Transaction Date to be {txnDoc.TransactionDate.Value.ToString("yyyy-MM-dd")} but was {Convert.ToDateTime(transactionDoc.TransactionProcessedDate).ToString("yyyy-MM-dd")}");

            Assert.True(txnDoc.ActivityDate.Value.ToString("yyyy-MM-dd").Equals(Convert.ToDateTime(transactionDoc.ActivityDate).ToString("yyyy-MM-dd")),
                $"For the transaction Id {transactionId} , Expected the Activity Date to be {txnDoc.TransactionDate.Value.ToString("yyyy-MM-dd")} but was {Convert.ToDateTime(transactionDoc.TransactionProcessedDate).ToString("yyyy-MM-dd")}");

            Assert.True(txnDoc.TransactionChannel.Equals(transactionDoc.TransactionChannel),
                $"For the transaction Id {transactionId} , Expected the Transaction Channel to be {txnDoc.TransactionChannel} but was {transactionDoc.TransactionChannel}");

            Assert.True("Flight Redemption".Equals(transactionDoc.ProductName),
                $"For the transaction Id {transactionId} , Expected the Product Name to be Flight Redemption but was {transactionDoc.ProductName}");

            Assert.True(txnDoc.StatusCode.Equals(transactionDoc.Status),
                $"For the transaction Id {transactionId} , Expected the Transaction Status to be {txnDoc.StatusCode} but was {transactionDoc.Status}");


            FlightDetail? flightDetail = transactionDoc.FlightDetail;


            Assert.True(txnDoc.FlightDetails.ExternalReservationNumber == flightDetail.BookingReference,
                $"For the transaction Id {transactionId} , Expected the Booking Reference to be {txnDoc.FlightDetails.ExternalReservationNumber} but was {flightDetail.BookingReference}");

            Assert.True(txnDoc.FlightDetails.OperatingFlightNumber == flightDetail.OperatedFlightNumber,
                $"For the transaction Id {transactionId} , Expected the Operating Flight Number to be {txnDoc.FlightDetails.OperatingFlightNumber} but was {flightDetail.OperatedFlightNumber}");

            Assert.True(txnDoc.FlightDetails.MarketingFlightNumber == flightDetail.MarketingFlightNumber,
                $"For the transaction Id {transactionId} , Expected the Marketing Flight Number to be {txnDoc.FlightDetails.MarketingFlightNumber} but was {flightDetail.MarketingFlightNumber}");

            Assert.True(txnDoc.FlightDetails.MarketingBookingClass == flightDetail.MarketingBookingClass,
                $"For the transaction Id {transactionId} , Expected the Marketing Booking Class to be {txnDoc.FlightDetails.MarketingBookingClass} but was {flightDetail.MarketingBookingClass}");

            Assert.True(txnDoc.FlightDetails.Seatnumber == flightDetail.SeatNumber,
                $"For the transaction Id {transactionId} , Expected the Seat Number to be {txnDoc.FlightDetails.Seatnumber} but was {flightDetail.SeatNumber}");

            Assert.True(txnDoc.FlightDetails.OriginAirportcode == flightDetail.OriginAirportCode,
                $"For the transaction Id {transactionId} , Expected the Origin Aiport Code  to be {txnDoc.FlightDetails.OriginAirportcode} but was {flightDetail.OriginAirportCode}");

            Assert.True(txnDoc.FlightDetails.DestinationAirport == flightDetail.DestinationAirportCode,
                $"For the transaction Id {transactionId} , Expected the Destination Aiport Code  to be {txnDoc.FlightDetails.DestinationAirport} but was {flightDetail.DestinationAirportCode}");

            Assert.True(txnDoc.FlightDetails.Ticketnumber == flightDetail.TicketNumber,
                $"For the transaction Id {transactionId} , Expected the TicketNumber  to be {txnDoc.FlightDetails.Ticketnumber} but was {flightDetail.TicketNumber}");

            Assert.True(txnDoc.FlightDetails.BookingClass == flightDetail.BookingClass,
                $"For the transaction Id {transactionId} , Expected the Booking Class to be {txnDoc.FlightDetails.BookingClass} but was {flightDetail.BookingClass}");

            Assert.True(txnDoc.FlightDetails.CabinClass == flightDetail.CabinClass,
                $"For the transaction Id {transactionId} , Expected the Cabin Class to be {txnDoc.FlightDetails.CabinClass} but was {flightDetail.CabinClass}");

            string bookingDate = txnDoc.FlightDetails.BookingDate.HasValue ? txnDoc.FlightDetails.BookingDate.Value.ToString("yyyy-MM-dd") : "";

            Assert.True(bookingDate == Convert.ToDateTime(flightDetail?.TicketIssueDate).ToString("yyyy-MM-dd"),
                $"For the transaction Id {transactionId} , Expected the Booking/TicketIssue Date to be {bookingDate} but was {Convert.ToDateTime(flightDetail.TicketIssueDate).ToString("yyyy-MM-dd")}");

            string departureDate = txnDoc.FlightDetails.DepartureDate.HasValue ? txnDoc.FlightDetails.DepartureDate.Value.ToString("yyyy-MM-dd") : "";


            Assert.True(departureDate == Convert.ToDateTime(flightDetail.DepartureDate).ToString("yyyy-MM-dd"),
                $"For the transaction Id {transactionId} , Expected the Departure Date to be {departureDate} but was {Convert.ToDateTime(flightDetail.DepartureDate).ToString("yyyy-MM-dd")}");


            IEnumerable<Passenger> passengers = transactionDoc.Passengers;

            IEnumerable<PassengerDetails> passengerDetails = txnDoc.FlightDetails.PassengerDetails;

            Assert.True(passengers.Count() == passengerDetails.Count(),
                   $"Passenger Count Does Not Match For the transaction Id {transactionId} " +
                   $"Expected Count to be {passengerDetails.Count()} but was {passengers.Count()}.");

            int i = 0;

            foreach (PassengerDetails passengerDetail in passengerDetails)
            {
                Assert.True(passengerDetail.PassengerName == passengers.ElementAt(i).PassengerName,
                $"For the transaction Id {transactionId} , Expected the Passenger Name to be {passengerDetail.PassengerName} but was {passengers.ElementAt(i).PassengerName}");

                Assert.True(Convert.ToInt32(Math.Round(Convert.ToDouble(passengerDetail.ValueRedeemed))).ToString() == passengers.ElementAt(i).PointsRedeemed,
                $"For the transaction Id {transactionId} , Expected the Points Reedemed Value to be {passengerDetail.ValueRedeemed} but was {passengers.ElementAt(i).PointsRedeemed}");

                Assert.True(passengerDetail.PhysicalTicketNumber == passengers.ElementAt(i).TicketNumber,
                $"For the transaction Id {transactionId} , Expected the TicketNumber to be {passengerDetail.PhysicalTicketNumber} but was {passengers.ElementAt(i).TicketNumber}");

                int j = 0;

                IEnumerable<FlightSegment> flightSegment = transactionDoc.Passengers.ElementAt(i).flightSegments;

                IEnumerable<SegmentDetails> segmentDetails = passengerDetail.SegmentDetails;

                Assert.True(flightSegment.Count() == segmentDetails.Count(),
                   $"Flight Segment Count Does Not Match for the Passenger {passengerDetail.PassengerName}. " +
                   $"Expected Count to be {segmentDetails.Count()} but was {flightSegment.Count()}.");

                foreach (SegmentDetails segmentDetail in segmentDetails)
                {
                    Assert.True(segmentDetail.BookingClass == flightSegment.ElementAt(j).BookingClass,
                    $"For the Passenger {passengerDetail.PassengerName} in the flight Segment ," +
                    $" Expected the Booking Class to be {segmentDetail.BookingClass} but was {flightSegment.ElementAt(j).BookingClass}");

                    Assert.True(segmentDetail.PartnerCode == flightSegment.ElementAt(j).PartnerCode,
                    $"For the Passenger {passengerDetail.PassengerName} in the flight Segment ," +
                    $" Expected the Partner Code to be {segmentDetail.PartnerCode} but was {flightSegment.ElementAt(j).PartnerCode}");

                    Assert.True(segmentDetail.OriginAirportcode == flightSegment.ElementAt(j).OriginAirportCode,
                    $"For the Passenger {passengerDetail.PassengerName} in the flight Segment ," +
                    $" Expected the Origin Airport Code to be {segmentDetail.OriginAirportcode} but was {flightSegment.ElementAt(j).OriginAirportCode}");

                    Assert.True(segmentDetail.DestinationAirportcode == flightSegment.ElementAt(j).DestinationAirportCode,
                    $"For the Passenger {passengerDetail.PassengerName} in the flight Segment ," +
                    $" Expected the Destination Airport Code to be {segmentDetail.DestinationAirportcode} but was {flightSegment.ElementAt(j).DestinationAirportCode}");

                    j++;
                }

                i++;
            }


            #endregion

        }

        [Trait("Category", Traits.LOOPSTransactionAPIRegression)]
        [Fact, RunOrder(2)]
        public async Task TC02_RetrieveAccrualTransactionUsingCustomerIdandTransactionId()
        {
            #region Test Data

            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionType = @transactionType and c.ProductId = @productId order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction").WithParameter("@transactionType", "Accrual").WithParameter("@productId", "1-1Z9D");

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            string customerId = txnDoc.CustomerId;

            string transactionId = txnDoc.TransactionId;

            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Transaction.RetrieveAccrualTransaction(tokenFixture.GetToken("transaction"), customerId, transactionId).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            AccrualTransaction transactionDoc = JsonConvert.DeserializeObject<AccrualTransaction>(result[0].ToString());

            #endregion

            #region Validations

            Assert.True(txnDoc.MemberNumber.Equals(transactionDoc.MemberNumber),
                $"For the transaction Id {transactionId} , Expected the MemberNumber to be {txnDoc.MemberNumber} but was {transactionDoc.MemberNumber}");

            Assert.True(txnDoc.TransactionType.Equals(transactionDoc.TransactionType),
                $"For the transaction Id {transactionId} , Expected the Transaction Type to be {txnDoc.TransactionType} but was {transactionDoc.TransactionType}");

            Assert.True(txnDoc.TransactionDate.Value.ToString("yyyy-MM-dd").Equals(Convert.ToDateTime(transactionDoc.TransactionProcessedDate).ToString("yyyy-MM-dd")),
                $"For the transaction Id {transactionId} , Expected the Transaction Date to be {txnDoc.TransactionDate.Value.ToString("yyyy-MM-dd")} but was {Convert.ToDateTime(transactionDoc.TransactionProcessedDate).ToString("yyyy-MM-dd")}");

            Assert.True(txnDoc.ActivityDate.Value.ToString("yyyy-MM-dd").Equals(Convert.ToDateTime(transactionDoc.ActivityDate).ToString("yyyy-MM-dd")),
                $"For the transaction Id {transactionId} , Expected the Activity Date to be {txnDoc.TransactionDate.Value.ToString("yyyy-MM-dd")} but was {Convert.ToDateTime(transactionDoc.TransactionProcessedDate).ToString("yyyy-MM-dd")}");

            Assert.True(txnDoc.TransactionChannel.Equals(transactionDoc.TransactionChannel),
                $"For the transaction Id {transactionId} , Expected the Transaction Channel to be {txnDoc.TransactionChannel} but was {transactionDoc.TransactionChannel}");



            Requests.Models.BackendAPI.FlightDetails? flightDetail = transactionDoc.FlightDetail;

            Assert.True(txnDoc.FlightDetails.ExternalReservationNumber == flightDetail.BookingReference,
               $"For the transaction Id {transactionId} , Expected the Booking Reference to be {txnDoc.FlightDetails.ExternalReservationNumber} but was {flightDetail.BookingReference}");

            Assert.True(txnDoc.FlightDetails.OriginAirportcode == flightDetail.OriginAirportCode,
                $"For the transaction Id {transactionId} , Expected the Origin Aiport Code  to be {txnDoc.FlightDetails.OriginAirportcode} but was {flightDetail.OriginAirportCode}");

            Assert.True(txnDoc.FlightDetails.DestinationAirport == flightDetail.DestinationAirportCode,
                $"For the transaction Id {transactionId} , Expected the Destination Aiport Code  to be {txnDoc.FlightDetails.DestinationAirport} but was {flightDetail.DestinationAirportCode}");


            IEnumerable<CreditItem> creditItems = transactionDoc.CreditItems;

            IEnumerable<CreditDetails> creditDetails = txnDoc.CreditDetails;

            Assert.True(creditDetails.Count() == creditItems.Count(),
                   $"Credit Detail Count Does Not Match for the Transaction {transactionId}. " +
                   $"Expected Count to be {creditDetails.Count()} but was {creditItems.Count()}.");

            int j = 0;

            foreach (CreditDetails creditDetail in creditDetails)
            {
                Assert.True(creditDetail.PointType == creditItems.ElementAt(j).PointType,
               $"For the transaction Id {transactionId} ," +
               $" Expected the Point Type to be {creditDetail.PointType} but was {creditItems.ElementAt(j).PointType}");

                Assert.True(creditDetail.PointSubType == creditItems.ElementAt(j).PointSubType,
               $"For the txn Id {transactionId} for the Point Type Value {creditDetail.PointType} ," +
               $" Expected the Point Sub Type to be {creditDetail.PointSubType} but was {creditItems.ElementAt(j).PointSubType}");

                Assert.True(Convert.ToInt32(Math.Round(Convert.ToDouble(creditDetail.CreditValue))).ToString() == creditItems.ElementAt(j).CreditedValue,
                $"For the txn Id {transactionId} for the Point Type Value {creditDetail.PointType} ," +
                $" Expected the Credit Value to be {creditDetail.CreditValue} but was {creditItems.ElementAt(j).CreditedValue}");

                j++;
            }




            #endregion
        }

        [Trait("Category", Traits.LOOPSTransactionAPIRegression)]
        [Fact, RunOrder(3)]
        public async Task TC03_CreateAccrualFlightTransactionUsingCustomerId()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string memberNumber = fcmDoc.MemberNumber;

            DateTime randomDate = DataGenerator.RandomPastDate(365);

            string activityDate = randomDate.ToString("yyyy-MM-dd");

            string ticketIssueDate = randomDate.Subtract(TimeSpan.FromDays(30)).ToString("yyyy-MM-dd");

            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();


            #endregion

            #region API Call

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Transaction.CreateAccrualTransaction(tokenFixture.GetToken("transaction"), customerId, bookingReference, activityDate, ticketIssueDate).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            #endregion

            #region Validations

            string transactionId = result.transactionId.ToString();

            Assert.True(!string.IsNullOrEmpty(transactionId),
                $"Expected a valid transaction Id in the response but found {result.ToString()}");

            Assert.True(result.transactionType.ToString().Equals("Accrual"),
               $"Expected the transaction Type in the response to be Accrual but found {result.ToString()}");

            Assert.True(result.transactionStatus.ToString().Equals("Processed"),
               $"Expected the transaction Status in the response to be Processed but found {result.ToString()}");

            Assert.True(result.memberNumber.ToString().Equals(memberNumber),
              $"Expected the MemberNumber in the response to be {memberNumber} but found {result.ToString()}");

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionId = @transactionId order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@transactionId", transactionId);

            int retry = 5;

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            while (txnDoc == null && retry > 0)
            {
                await Task.Delay(500);

                txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

                retry--;
            }

            Assert.True(txnDoc.ActivityDate.Value.ToString("yyyy-MM-dd").Equals(activityDate),
              $"Expected the ActivityDate in the Cosmos Doc to be {activityDate} but found {txnDoc.ActivityDate.Value.ToString("yyyy-MM-dd")}");

            Assert.True(txnDoc.TransactionChannel.Equals("PDM"),
             $"Expected the Transaction Channel in the Cosmos Doc to be PDM but found {txnDoc.TransactionChannel}");

            Assert.True(txnDoc.FlightDetails.DepartureDate.Value.ToString("yyyy-MM-dd").Equals(activityDate),
             $"Expected the DepartureDate in the Cosmos Doc to be {activityDate} but found {txnDoc.FlightDetails.DepartureDate.Value.ToString("yyyy-MM-dd")}");

            Assert.True(txnDoc.StatusCode.Equals("Processed"),
           $"Expected the Transaction Status in the Cosmos Doc to be Processed but found {txnDoc.StatusCode}");


            #endregion
        }

        [Trait("Category", Traits.LOOPSTransactionAPIRegression)]
        [Fact, RunOrder(4)]
        public async Task TC04_CreateRedemptionFlightTransactionUsingCustomerId()
        {
            #region Test Data

            string queryForFCMDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TierCode not in (@tierCode1,@tierCode2) order by c.CreatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForFCMDoc).WithParameter("@docType", "FlyingClubMember").WithParameter("@tierCode1", "Glass").WithParameter("@tierCode2", "VAC");

            FlyingClubMember fcmDoc = cosmosFixture.CosmosQueryItem<FlyingClubMember>(parameterizedQuery);

            string customerId = fcmDoc.CustomerId;

            string memberNumber = fcmDoc.MemberNumber;

            DateTime randomDate = DataGenerator.RandomPastDate(365);

            string accrualActivityDate = randomDate.ToString("yyyy-MM-dd");

            string bookingReference = DataGenerator.RandomString(6, true).ToUpper();

            #endregion

            #region Manual Adjustment API Call and Validations

            HttpResponseMessage response = LoopsBackendAPIEndpoints.Transaction.CreateManualAdjustmentTransaction(tokenFixture.GetToken("transaction"), customerId, accrualActivityDate).EnsureIsSuccessStatusCode();

            dynamic result = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            string transactionId = result.transactionId.ToString();

            Assert.True(!string.IsNullOrEmpty(transactionId),
                $"Expected a valid transaction Id in the Manual Adjustment API response but found {result.ToString()}");

            Assert.True(result.transactionType.ToString().Equals("Accrual"),
               $"Expected the transaction Type in the Manual Adjustment API response to be Accrual but found {result.ToString()}");

            Assert.True(result.transactionStatus.ToString().Equals("Processed"),
               $"Expected the transaction Status in the Manual Adjustment API response to be Processed but found {result.ToString()}");

            Assert.True(result.memberNumber.ToString().Equals(memberNumber),
              $"Expected the MemberNumber in the Manual Adjustment API response to be {memberNumber} but found {result.ToString()}");

            #endregion

            #region Create Redemption API Call and Validations

            string redemptionActivityDate = DataGenerator.RandomFutureDate(365).ToString("yyyy-MM-dd");

            string redemptionTicketIssueDate = DateTime.UtcNow.AddDays(-30).ToString("yyyy-MM-dd");

            HttpResponseMessage httpResponse = LoopsBackendAPIEndpoints.Transaction.CreateRedemptionTransaction(tokenFixture.GetToken("transaction"), customerId, bookingReference, redemptionActivityDate, redemptionTicketIssueDate).EnsureIsSuccessStatusCode();

            dynamic httpResult = JsonConvert.DeserializeObject<dynamic>(await httpResponse.Content.ReadAsStringAsync());

            string redemptionTransactionId = httpResult.transactionId.ToString();

            Assert.True(!string.IsNullOrEmpty(redemptionTransactionId),
                $"Expected a valid transaction Id in the Create Redemption API response but found {httpResult.ToString()}");

            Assert.True(httpResult.transactionType.ToString().Equals("Redemption"),
               $"Expected the transaction Type in the Create Redemption API response to be Redemption but found {httpResult.ToString()}");

            Assert.True(httpResult.transactionStatus.ToString().Equals("Processed"),
               $"Expected the transaction Status in the Create Redemption API response to be Processed but found {httpResult.ToString()}");

            Assert.True(httpResult.memberNumber.ToString().Equals(memberNumber),
              $"Expected the MemberNumber in the Create Redemption API response to be {memberNumber} but found {httpResult.ToString()}");

            string queryForTxnDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType and c.TransactionId = @transactionId order by c._ts desc";

            QueryDefinition paramQuery = new QueryDefinition(query: queryForTxnDoc).WithParameter("@docType", "Transaction").WithParameter("@transactionId", redemptionTransactionId);

            int retry = 5;

            Transaction txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

            while (txnDoc == null && retry > 0)
            {
                await Task.Delay(500);

                txnDoc = cosmosFixture.CosmosQueryItem<Transaction>(paramQuery);

                retry--;
            }

            Assert.True(txnDoc.ActivityDate.Value.ToString("yyyy-MM-dd").Equals(redemptionActivityDate),
              $"Expected the ActivityDate in the Cosmos Doc for the transaction id {redemptionTransactionId} " +
              $"to be {redemptionActivityDate} but found {txnDoc.ActivityDate.Value.ToString("yyyy-MM-dd")}");

            Assert.True(txnDoc.TransactionChannel.Equals("PDM"),
             $"Expected the Transaction Channel in the Cosmos Doc for the transaction id {redemptionTransactionId}" +
             $" to be PDM but found {txnDoc.TransactionChannel}");

            Assert.True(txnDoc.FlightDetails.DepartureDate.Value.ToString("yyyy-MM-dd").Equals(redemptionActivityDate),
             $"Expected the DepartureDate in the Cosmos Doc for the transaction id {redemptionTransactionId}" +
             $" to be {redemptionActivityDate} but found {txnDoc.FlightDetails.DepartureDate.Value.ToString("yyyy-MM-dd")}");

            Assert.True(txnDoc.StatusCode.Equals("Processed"),
           $"Expected the Transaction Status in the Cosmos Doc for the transaction id {redemptionTransactionId}" +
           $" to be Processed but found {txnDoc.StatusCode}");

            #endregion
        }

    }
}
