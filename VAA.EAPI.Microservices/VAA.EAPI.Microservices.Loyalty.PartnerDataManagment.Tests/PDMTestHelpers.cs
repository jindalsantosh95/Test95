using Microsoft.Azure.Functions.Worker.Http;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Tests
{
    public static class PDMTestHelpers
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new Dictionary<string, string>()
            {
                {"AzureWebJobsScriptRoot", ".\\" },               
                {"RetroDurationinDays", "365" },
                {"NonAccrualVSClasses","A,P,G" }
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);;
        }

        public static Infrastructure.DataModels.RetroCreditFlownDetails FetchRetroCreditFlownDetails(string status="Free", string ffpCarrier="LY", string ffpAccountNumber= "123444")
        {
            return new Infrastructure.DataModels.RetroCreditFlownDetails
            {
                FdNoShow = "N",
                FFPAccountNumber = ffpAccountNumber,
                FFPCarrierCode = ffpCarrier,
                FlownCabinClass = "C",
                MarketingAirlineCode = "LY",
                FirstName = "ABC",
                LastName = "EFG",
                MarketingCarrierBookingClass = "C",
                MarketingFlightNum = "301",
                OperatingAirAuthNumber = "12121212112",
                OperatingAirlineCode = "VA",
                OperatingCarierBookingClass = "C",
                OperatingFlightNum = "301",
                OrginPort = "DEL",
                DepartureDate = DateTime.Now.AddDays(-10).ToString("yyyyMMdd"),
                DestinationPort = "LHR",
                ProcessingStatus = status,
                TicketNumber = "AD12121212"
            };
        }

        public static async Task<T> ReadResponseData<T>(HttpResponseData responseData)
        {
            responseData.Body.Seek(0, SeekOrigin.Begin);
            var reader = new StreamReader(responseData.Body);
            var responseBody = await reader.ReadToEndAsync();
            return Json.Deserialize<T>(responseBody);
        }
    }
}
