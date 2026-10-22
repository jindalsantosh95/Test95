using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.DB_Model.Entities;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Xunit.Attributes;
using VAA.EAPI.Automation.Core.Xunit.Fixtures;
using VAA.EAPI.Automation.MarketingLoyalty.Constants;
using VAA.EAPI.Automation.MarketingLoyalty.Requests.Endpoint;

namespace VAA.EAPI.Automation.MarketingLoyalty.Tests.Microservices.CustomerTransaction
{

    public class RetrieveTransactionDocTests : IClassFixture<TokenFixture>, IClassFixture<CosmosFixture>
    {

        private readonly TokenFixture tokenFixture;
        private readonly CosmosFixture cosmosFixture;

        public static string env = Config.Environment;

        private readonly string customerTransactionCosmosDb;

        private static string ocpSubKey = string.Empty;

        public RetrieveTransactionDocTests(TokenFixture tokenFixture, CosmosFixture cosmosFixture)
        {

            this.tokenFixture = tokenFixture;
            this.cosmosFixture = cosmosFixture;

            this.tokenFixture.AddTokenForCustomerTxnFromKeyVault("customertxn");

            customerTransactionCosmosDb = env.Is("uat") ? Application.CustomerTransactionCosmosDbUAT : Application.CustomerTransactionCosmosDbSIT;

            string cosmosEndpointForCustomerTrx = Application.CustomerTransactionCosmosDbEndpoint;

            this.cosmosFixture.RegisterClient(cosmosEndpointForCustomerTrx, Vault.Default.GetSecretValue("TESTING-Cmdbcustomertransactions-Automation-Key"));
            this.cosmosFixture.RegisterType<Transaction>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<SegmentDetails>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<PassengerDetails>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<FlightDetails>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);
            this.cosmosFixture.RegisterType<CreditDetails>(cosmosEndpointForCustomerTrx, customerTransactionCosmosDb, Application.CustomerTransactionCosmosCollection);

            ocpSubKey = Vault.Default.GetSecretValue("TestingCustomerTxnMSSubscriptionKey");

        }


        [Trait("Category", Traits.CustomerTransactionMSRegression)]
        [Fact, RunOrder(1)]
        public async Task A_RetrieveTransactionDoc_SuccessScenario()
        {

            #region Test Data

            string queryForTransactionDoc = "SELECT TOP 1 * FROM CustomerDetailsCollection c where c.DocType = @docType  order by c.LastUpdatedOn desc";

            QueryDefinition parameterizedQuery = new QueryDefinition(query: queryForTransactionDoc).WithParameter("@docType", "Transaction");

            Transaction transaction = cosmosFixture.CosmosQueryItem<Transaction>(parameterizedQuery);

            string customerId = transaction.CustomerId;

            #endregion

            string flightProductId = "1-1Z9D";

            string flightRedemptionProductId = "1-1Z9X";

            HttpResponseMessage response = Microservice.RetrieveTransactionDoc(tokenFixture.GetToken("customertxn"), ocpSubKey, customerId).EnsureIsSuccessStatusCode();

            dynamic retrieveTransactionDocResult = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());

            dynamic result = retrieveTransactionDocResult.transactionDocuments[0];

            Assert.True(result.customerId.ToString().Equals(customerId),
                $"Expected the CustomerId as {customerId}  but found {result.customerId}");

            Assert.True(result.memberNumber.ToString().Equals(transaction.MemberNumber),
                $"Expected the Member Number as {transaction.MemberNumber}  but found {result.memberNumber}");

            Assert.True(result.transactionValue.ToString().Equals(transaction.TransactionValue),
                $"Expected the Transaction Value as {transaction.TransactionValue}  but found {result.transactionValue}");

            Assert.True(result.partnerId.ToString().Equals(transaction.PartnerId),
               $"Expected the Partner Id Value as {transaction.PartnerId}  but found {result.partnerId}");

            Assert.True(DateTime.Parse(result.transactionDate.ToString()).Date.Equals(transaction.TransactionDate.Value.Date),
              $"Expected the Transaction Date Value as {transaction.TransactionDate.Value.Date} " +
              $" but found {DateTime.Parse(result.transactionDate.ToString()).Date}");

            Assert.True(result.productId.ToString().Equals(transaction.ProductId),
              $"Expected the Product Id Value as {transaction.ProductId}  but found {result.productId}");

            Assert.True(result.transactionId.ToString().Equals(transaction.TransactionId),
              $"Expected the Transaction Id Value as {transaction.TransactionId}  but found {result.transactionId}");

            Assert.True(result.statusCode.ToString().Equals(transaction.StatusCode),
              $"Expected the StatusCode Value as {transaction.StatusCode}  but found {result.statusCode}");

            Assert.True(result.transactionType.ToString().Equals(transaction.TransactionType),
              $"Expected the TransactionType Value as {transaction.TransactionType} " +
              $" but found {result.transactionType}");


            Assert.True(result.transactionSubType.ToString().Equals(transaction.TransactionSubType),
              $"Expected the TransactionSubType Value as {transaction.TransactionSubType} " +
              $" but found {result.transactionSubType}");

            Assert.True(result.statementText.ToString().Equals(transaction.StatementText),
           $"Expected the Statement Text Value as {transaction.StatementText} " +
           $" but found {result.statementText}");

            string txnType = transaction.TransactionType;



            result = retrieveTransactionDocResult?.transactionDocuments[0]?.flightDetails;

            if (transaction.ProductId.IsAny(flightProductId, flightRedemptionProductId) && result is not null)
            {

                //Validating Flight Details Section

                Assert.True(result?.bookingClass?.ToString() == (transaction.FlightDetails?.BookingClass),
                $"Expected the Flight Booking Class Value as {transaction.FlightDetails.BookingClass} " +
                $" but found {result.bookingClass}");

                Assert.True(result?.destinationAirport?.ToString() == transaction?.FlightDetails?.DestinationAirport,
                $"Expected the Flight Destination Airport Value as {transaction?.FlightDetails?.DestinationAirport} " +
                $" but found {result?.destinationAirport}");

                Assert.True(result?.originAirportcode?.ToString() == transaction?.FlightDetails?.OriginAirportcode,
                $"Expected the Flight Origin Airport Code Value as {transaction?.FlightDetails?.OriginAirportcode} " +
                $" but found {result?.originAirportcode}");

                Assert.True(result?.externalReservationNumber?.ToString() == transaction?.FlightDetails?.ExternalReservationNumber,
                $"Expected the Flight externalReservationNumber or PNR Value as {transaction?.FlightDetails?.ExternalReservationNumber} " +
                $" but found {result?.externalReservationNumber}");

                Assert.True(result?.operatingFlightNumber?.ToString() == transaction?.FlightDetails?.OperatingFlightNumber,
                $"Expected the Operating Flight Number Value as {transaction?.FlightDetails?.OperatingFlightNumber} " +
                $" but found {result?.operatingFlightNumber}");

                Assert.True(DateTimeOffset.Parse(result?.departureDate?.ToString()) == transaction?.FlightDetails?.DepartureDate,
                $"Expected the Flight Departure Date Value as {transaction?.FlightDetails?.DepartureDate} " +
                $" but found {result?.departureDate}");

                Assert.True(result?.distance?.ToString() == transaction?.FlightDetails?.Distance,
                $"Expected the Distance Value as {transaction?.FlightDetails?.Distance} " +
                $" but found {result?.distance}");
            }


            result = retrieveTransactionDocResult.transactionDocuments[0]?.flightDetails?.passengerDetails;

            List<PassengerDetails> passengerDetails = transaction.FlightDetails.PassengerDetails;

            if (transaction.ProductId.Is(flightRedemptionProductId) && result is not null)
            {

                for (int i = 0; i < result.Count; i++)
                {
                    //Validating the Passenger Details Section

                    foreach (PassengerDetails passenger in passengerDetails)
                    {

                        Assert.True(result[i].passengerName.ToString().Equals(passenger.PassengerName),
                        $"Expected the Passenger Name Value as {passenger.PassengerName} " +
                        $" but found {result[i].passengerName}");


                        List<SegmentDetails> segmentDetails = passenger.SegmentDetails;

                        //Validating the Segment Details Section
                        for (int j = 0; j < segmentDetails.Count; j++)
                        {
                            Assert.True(result[i].segmentDetails[0].bookingClass.ToString().Equals
                            (segmentDetails[i].BookingClass),
                            $"Expected the Booking Class Value in the Segment as {segmentDetails[i].BookingClass} " +
                                $" but found {result[i].segmentDetails[0].bookingClass}");

                            Assert.True(result[i].segmentDetails[0].originAirportcode.ToString().Equals
                            (segmentDetails[i].OriginAirportcode),
                            $"Expected the Origin Airport Code Value as {segmentDetails[i].OriginAirportcode} " +
                                $" but found {result[i].segmentDetails[0].originAirportcode}");

                            Assert.True(result[i].segmentDetails[0].destinationAirportcode.ToString().Equals
                            (segmentDetails[i].DestinationAirportcode),
                            $"Expected the Destination Airport Code Value as {segmentDetails[i].DestinationAirportcode} " +
                                $" but found {result[i].segmentDetails[0].destinationAirportcode}");

                        }
                    }
                }
            }

            result = retrieveTransactionDocResult?.transactionDocuments[0]?.creditDetails;

            if (result is not null)
            {
                int count = retrieveTransactionDocResult.transactionDocuments[0].creditDetails.Count;

                if (txnType.Equals("Accrual"))
                {

                    List<CreditDetails> creditDetails = transaction.CreditDetails;

                    if (count.Equals(creditDetails.Count))
                    {

                        for (int i = 0; i < creditDetails.Count; i++)
                        {

                            Assert.True(result[i].pointType.ToString().Equals
                               (creditDetails[i].PointType),
                               $"Expected the Point Type Value as {creditDetails[i].PointType} " +
                                   $" but found {result[i].pointType}");

                            Assert.True(result[i].creditValue.ToString().Equals
                               (creditDetails[i].CreditValue),
                               $"Expected the Credit Value as {creditDetails[i].CreditValue} " +
                                   $" but found {result[i].creditValue}");
                        }
                    }

                }
            }
        }
    }
}
