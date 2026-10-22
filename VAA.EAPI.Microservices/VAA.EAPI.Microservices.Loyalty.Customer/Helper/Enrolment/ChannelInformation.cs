using System.Collections.Generic;
using System.IO;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Channel;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment
{
    public static class ChannelInformation
    {
        public static List<Channel> GetchannelDetails()
        {
            string channelCodesString = string.Empty;
            using (StreamReader stream = new(@".\Infrastructure\Shared\ChannelConfiguration.json"))
                channelCodesString = stream.ReadToEnd();

            return Json.Deserialize<List<Channel>>(channelCodesString);
        }
    }
}
