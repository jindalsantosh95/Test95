using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public static class ValidateChannel
    {
        public static bool Validate(string channel)
        {
            return ChannelInformation.GetchannelDetails().Any(channels => channels.Code == channel);
        }

    }
}
