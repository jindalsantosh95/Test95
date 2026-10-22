using System.Runtime.Serialization;

namespace VAA.Facade.EnrolmentFacade.Domain.Channel
{
    public class ChannelUser
    {
        [DataMember(Name = "channelUserId")]
        public string ChannelUserId { get; set; }

    }
}
