using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Helper
{
    public class PaymentChannel
    {
        private static List<Channel> GetChannelValues()
        {
            var channelValues = ConfigReader.ReadAllText(EAPI.Infrastructure.Constants.Constants.FunctionAppDirPath, Constants.Application.ChannelCodePath);
            return Json.Deserialize<List<Channel>>(channelValues);
        }
        public static bool IsValid(string value)
        {
            return GetChannelValues().Any(channels => channels.Value == value);
        }
    }
}
