using VAA.Facade.EnrolmentFacade.Domain;

namespace VAA.Facade.EnrolmentFacade.Application.Command
{
    public class EnrolmentRequestCommand
    {
        public EnrolmentRequest EnrolmentRequest { get; set; }
        public string Token { get; set; }
        public string OcpKey { get; set; }
        public string ChannelId { get; set; }
        public string ChannelUserId { get; set; }

        public EnrolmentRequestCommand()
        {
        }
    }
}
