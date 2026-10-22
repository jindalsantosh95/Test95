using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Channel;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Validations
{
    public static class ValidateChannel
    {
        public static bool Validate(string channel)
        {
            List<Channel> channeldetails = Helper.ChannelInformation.GetchannelDetails();

            bool isValidChannel = channeldetails.Any(channels => channels.Code == channel);

            return isValidChannel;
        }
    }
}
