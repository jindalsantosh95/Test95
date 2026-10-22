using System.Collections.Generic;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Flight.Services.Tests
{
    public class FlightServiceMsEnvironmentSetup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
               { "DeltaCertificateName", "NdcDLSoaCertificate" },
                { "ApplicationId", "ND" },
                { "TestLab", "TVTH" },
                { "Air4PnrCommonEndPoint", "https://webservices.vs01.vs.air4.com:31063/pnrcommon/v6" },
                { "SpecialServiceRequestV4EndPoint", "https://webservices.vs01.vs.air4.com:31052/specialservicerequest/v4" }      
            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
