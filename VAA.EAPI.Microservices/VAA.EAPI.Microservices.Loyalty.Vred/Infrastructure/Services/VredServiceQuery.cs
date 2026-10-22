using MediatR;
using VAA.EAPI.Microservices.Loyalty.Vred.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Infrastructure.Services
{
    public class VredServiceQuery : IRequest<VRedEnrolmentResponse>
    {
        public MemberEnrolmentVRedRequest Request { get; set; }
        public string EnrolmentChannel { get; set; }

        public VredServiceQuery(MemberEnrolmentVRedRequest memberEnrolmentVRedRequest, string enrolmentChannel)
        {
            Request = memberEnrolmentVRedRequest;
            EnrolmentChannel = enrolmentChannel;
        }
    }
}
