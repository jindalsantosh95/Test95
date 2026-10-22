using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Entities
{
    public class VRedEnrolmentResponse : IRequest<MemberEnrolmentVRedRequest>
    {
        public Dictionary<string, string> Enrollments { get; set; }
        public VRedErrorResponse ErrorResponse { get; set; }
    }
}
