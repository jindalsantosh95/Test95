using Microsoft.Azure.Functions.Worker.Http;
using System.Linq;
using VAA.Facade.EnrolmentFacade.Application.Command;
using VAA.Facade.EnrolmentFacade.Domain;

namespace VAA.Facade.EnrolmentFacade.Mapper
{
    public class EnrolmentRequestCommandMapper
    {
        public static EnrolmentRequestCommand Mapper(EnrolmentRequest enrolmentRequest, HttpRequestData req)
        {
            return new EnrolmentRequestCommand()
            {
                ChannelId = req.Headers.Any(k => string.Equals(k.Key, "channelid")) ? req.Headers.GetValues("channelid").Single() : null,
                ChannelUserId = req.Headers.Any(k => string.Equals(k.Key, "channeluserid")) ? req?.Headers.GetValues("channeluserid").FirstOrDefault() : null,
                EnrolmentRequest = enrolmentRequest,
                OcpKey = req.Headers.Any(k => string.Equals(k.Key, "ocp-apim-subscription-key")) ? req?.Headers.GetValues("ocp-apim-subscription-key").FirstOrDefault() : null,
                Token = req.Headers.Any(k => string.Equals(k.Key, "Authorization")) ? req?.Headers.GetValues("Authorization").FirstOrDefault() : null,

            };

        }
    }
}
