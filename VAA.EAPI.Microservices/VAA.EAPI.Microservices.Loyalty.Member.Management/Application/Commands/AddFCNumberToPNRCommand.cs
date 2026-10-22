using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.SSRToPNR;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Commands
{
    public class AddFCNumberToPNRCommand 
    {
        public string FileName { get; set; }
        public IEnumerable<MemberDetailsToAddSSR> MemberDeatilsToAddSSR { get; set; }
    }
}
