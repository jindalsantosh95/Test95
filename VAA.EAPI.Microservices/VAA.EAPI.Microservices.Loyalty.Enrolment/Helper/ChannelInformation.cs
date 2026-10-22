using System.Collections.Generic;
using System.IO;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Channel;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Helper
{
    public static class ChannelInformation
    {
        public static List<Channel> GetchannelDetails()
        {
            string channelCodesString = string.Empty;
            using (StreamReader stream = new(@".\Infrastructure\Shared\ChannelConfiguration.json"))
                channelCodesString = stream.ReadToEnd();

            return Newtonsoft.Json.JsonConvert.DeserializeObject<List<Channel>>(channelCodesString);
        }
        public static bool IsValiationRequired()
        {
            return false;
        }
    }
}
