using Newtonsoft.Json.Linq;

namespace VAA.EAPI.Automation.Book.Tests.NewBookFlow.Microservices
{
    public class MicroserviceAssertions
    {
        public static void LopaSeatAssertions(dynamic lopaResponse, string equipmentCode, string fleetTypeCode)
        {
            Assert.True(LopaSeatEquimentCodeAssertions(lopaResponse, equipmentCode, out string message), message);

            Assert.True(LopaSeatfleetTypeCodeAssertions(lopaResponse, fleetTypeCode, out string fleetMessage), fleetMessage);
        }

        public static bool LopaSeatEquimentCodeAssertions(dynamic lopaResponse, string equipmentCode, out string message)
        {
            message = string.Empty;

            var equipmentCodeResponse = (JArray)lopaResponse;

            foreach (var item in equipmentCodeResponse)
            {
                if (item["equipmentCode"].ToString() != equipmentCode)
                {
                    message = $"equipmentCodeResponse is wrong in Lopa seat {equipmentCodeResponse} and expected price difference not matched {equipmentCode} \n Response---\n\t {item["classType"]}";
                    return false;
                }
            }
            return true;
        }

        public static bool LopaSeatfleetTypeCodeAssertions(dynamic lopaResponse, string fleetTypeCode, out string message)
        {
            message = string.Empty;

            var fleetTypeCodeResponse = (JArray)lopaResponse;

            foreach (var item in fleetTypeCodeResponse)
            {
                var fleetTypeCodeRespitem = item["details"][1].ToString();

                fleetTypeCodeRespitem = fleetTypeCodeRespitem.Substring(0, 3);

                if (fleetTypeCodeRespitem != fleetTypeCode)
                {
                    message = $"fleetTypeCodeResponse is wrong in Lopa seat {fleetTypeCodeRespitem} and expected price difference not matched {fleetTypeCode} \n Response---\n\t {item["classType"]}";
                    return false;
                }
            }
            return true;
        }

    }
}
