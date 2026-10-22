using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using VAA.EAPI.Automation.Core;
using VAA.EAPI.Automation.Core.Extensions;
using VAA.EAPI.Automation.Core.Helpers;
using VAA.EAPI.Automation.MyAccount.Requests.Endpoint;

namespace VAA.EAPI.Automation.MyAccount.Tests.MicroserviceTests.MissingPoints
{
    public class ReferenceMicroServiceTests
    {
        public static string env = Config.Environment;

        private static string connectionString = Vault.Default.GetSecretValue("MarketingLoyalty-avateundbCommonReferenceDataConnectionString");

        [Theory]
        [InlineData("LHR", "DEL")]
        [InlineData("LHR", "JFK")]
        [InlineData("LHR", "ATL")]
        [Trait("Category", "ReferenceMicroServiceTests")]
        [Trait("Category", Constants.Traits.SharedServicesSignOff)]
        [Trait("SharedService", "ReferenceMS")]
        [Trait("SharedServiceConsumer", "MyAccount")]
        public async Task ValidateDistanceBetweenValidSourceAndDestination(string source, string destination)
        {
            System.Net.Http.HttpResponseMessage getDistanceResponse = Microservice.GetSegmentDistance(env, source, destination).EnsureIsSuccessStatusCode();

            string distanceResult = (await getDistanceResponse.Content.ReadAsStringAsync()).Split('"')[1];

            Assert.True(!distanceResult.IsNullOrEmpty(), $"Response returned is {distanceResult}");

            string queryForGetSegmentDistance = $"select DistanceInMiles from [dbo].[Distance] where Origin ='{source}' and Destination ='{destination}'";

            string distance = (string)SqlHelper.ExecuteScalar(queryForGetSegmentDistance, connectionString);

            Assert.True(distance.Has(distanceResult), $"Validation failed expected {distance}, found {distanceResult} between source {source} & destination {destination}");
        }

        [Theory]
        [InlineData("LHR", "CCU")]
        [InlineData("LHR", "LGW")]
        [InlineData("MAS", "ATL")]
        [Trait("Category", "ReferenceMicroServiceTests")]
        public async Task ValidateDistanceBetweenInvalidSourceAndDestination(string source, string destination)
        {
            System.Net.Http.HttpResponseMessage getDistanceResponse = Microservice.GetSegmentDistance(env, source, destination);

            Assert.True(getDistanceResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from reference ms, returned {getDistanceResponse.ReadResponseAsString()}");

            JArray distanceResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await getDistanceResponse.Content.ReadAsStringAsync());

            string errorCode = "10178b";
            string errorMessage = "Route does not exist";

            Assert.True(distanceResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {distanceResult}");

            Assert.True(distanceResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {distanceResult}");

        }

        [Theory]
        [InlineData("LHR", "ATL", "J")]
        [InlineData("LHR", "JNB", "D")]
        [InlineData("LHR", "DEL", "C")]
        [InlineData("LHR", "JFK", "I")]
        [InlineData("LHR", "AUS", "Z")]
        [InlineData("LHR", "BOS", "G")]
        [InlineData("LHR", "ATL", "W")]
        [InlineData("LHR", "JNB", "S")]
        [InlineData("LHR", "DEL", "K")]
        [InlineData("LHR", "JFK", "H")]
        [InlineData("LHR", "AUS", "P")]
        [InlineData("LHR", "BOS", "Y")]
        [InlineData("LHR", "ATL", "B")]
        [InlineData("LHR", "JNB", "R")]
        [InlineData("LHR", "DEL", "L")]
        [InlineData("LHR", "JFK", "M")]
        [InlineData("LHR", "AUS", "E")]
        [InlineData("LHR", "BOS", "Q")]
        [Trait("Category", "ReferenceMicroServiceTests")]
        public async Task ValidateBookingDetails(string source, string destination, string bookingClass)
        {
            System.Net.Http.HttpResponseMessage getBookingDetailsResponse = Microservice.GetBookingDetails(env, source, destination, bookingClass).EnsureIsSuccessStatusCode();

            JArray bookingDetailsResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await getBookingDetailsResponse.Content.ReadAsStringAsync());

            string queryForGetCabinClass = $"SELECT CabinClass  FROM [dbo].[PartnerBookingClass] where PartnerID ='1-1XVO' and BookingClass = '{bookingClass}'";

            string cabinClass = (string)SqlHelper.ExecuteScalar(queryForGetCabinClass, connectionString);

            string queryForGetSegmentDistance = $"select DistanceInMiles from [dbo].[Distance] where Origin ='{source}' and Destination ='{destination}'";

            string distance = (string)SqlHelper.ExecuteScalar(queryForGetSegmentDistance, connectionString);

            string queryForGetOriginCountryCode = $"select OriginCountryCode from [dbo].[Distance] where Origin ='{source}' and Destination ='{destination}'";

            string originCountryCode = (string)SqlHelper.ExecuteScalar(queryForGetOriginCountryCode, connectionString);

            string queryForGetDestinationCountryCode = $"select DestinationCountryCode from [dbo].[Distance] where Origin ='{source}' and Destination ='{destination}'";

            string destinationCountryCode = (string)SqlHelper.ExecuteScalar(queryForGetDestinationCountryCode, connectionString);

            Assert.True(cabinClass.Equals(bookingDetailsResult[0]["cabinClass"].ToString()), $"Cabin Class value not matched expected {cabinClass} but actual {bookingDetailsResult[0]["cabinClass"].ToString()}");

            Assert.True("BOOKINGCLASS".Equals(bookingDetailsResult[0]["bookClass"].ToString()), $"Booking Class value not matched expected BOOKINGCLASS but actual {bookingDetailsResult[0]["bookClass"].ToString()}");

            Assert.True(originCountryCode.Equals(bookingDetailsResult[0]["originCountryCode"].ToString()), $"Origin country code value not matched expected {originCountryCode} but actual {bookingDetailsResult[0]["originCountryCode"].ToString()}");

            Assert.True(destinationCountryCode.Equals(bookingDetailsResult[0]["destinationCountryCode"].ToString()), $"Destination Airport code value not matched expeccted {destinationCountryCode} but actual {bookingDetailsResult[0]["destinationCountryCode"].ToString()}");

            Assert.True(distance.Has(bookingDetailsResult[0]["distanceInMiles"].ToString()), $"Distance value not matched expeccted {distance} but actual {bookingDetailsResult[0]["distanceInMiles"].ToString()}");
        }

        [Theory]
        [InlineData("LHR", "CCU", "J")]
        [InlineData("LHR", "LGW", "W")]
        [InlineData("MAS", "ATL", "L")]
        [Trait("Category", "ReferenceMicroServiceTests")]
        public async Task ValidateBookingDetailsForInvalidSourceAndDestination(string source, string destination, string bookingClass)
        {
            System.Net.Http.HttpResponseMessage getBookingDetailsResponse = Microservice.GetBookingDetails(env, source, destination, bookingClass);

            Assert.True(getBookingDetailsResponse.StatusCode == System.Net.HttpStatusCode.UnprocessableEntity, $"422 was not returned from reference ms, returned {getBookingDetailsResponse.ReadResponseAsString()}");

            JArray bookingDetailsResult = (JArray)JsonConvert.DeserializeObject<dynamic>(await getBookingDetailsResponse.Content.ReadAsStringAsync());

            string errorCode = "10173";
            string errorMessage = "Error occurred while validating booking details";

            Assert.True(bookingDetailsResult[0]["message"].ToString().Equals(errorMessage), $"Error message is wrong in response, response is {bookingDetailsResult}");

            Assert.True(bookingDetailsResult[0]["code"].ToString().Equals(errorCode), $"Error code is not correct, response is {bookingDetailsResult}");

        }
    }
}
