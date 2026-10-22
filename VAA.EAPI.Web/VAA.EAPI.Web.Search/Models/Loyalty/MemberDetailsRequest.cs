using MediatR;

namespace VAA.EAPI.Web.Search.Models.Loyalty
{
    public class MemberDetailsRequest : IRequest<MemberDetailsResponse>
    {
        public string AuthToken { get; set; }
    }
}
