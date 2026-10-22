using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Notification.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Notification.Helper
{
    public class ChannelValidator
    {
        public static bool ValdidateChannel(string channel)
        {
            var channelDetails = GetChannelDetails();
            return channelDetails.Any(c => c.Value.Equals(channel));
        }

        private static List<ChannelConfiguration> GetChannelDetails()
        {
            var channelConfig = ConfigReader.ReadAllText(Infrastructure.Constants.Constants.FunctionAppDirPath, Constants.Application.ChannelConfigFilePath);
            return Json.Deserialize<List<ChannelConfiguration>>(channelConfig);

        }
    }
}
