using MediatR;
using System.Net.Http;

namespace VAA.EAPI.Microservices.Loyalty.Identity.Infrastructure.Services
{
    public class VerifyMemberServiceCommand : IRequest<string>
    {
        public string Api { get; set; }
        public FormUrlEncodedContent FormUrlEncodedContent { get; set; }

        public VerifyMemberServiceCommand(string api, FormUrlEncodedContent formUrlEncodedContent)
        {
            Api = api;
            FormUrlEncodedContent = formUrlEncodedContent;
        }
    }
}
