using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository
{
    public class CustomerSearchRepoQuery : IRequest<IEnumerable<object>>
    {
        public string MemberNumber { set; get; }
        public string DocType { set; get; }

        public CustomerSearchRepoQuery() { }

        public CustomerSearchRepoQuery(string memberNumber, string docType)
        {
            MemberNumber = memberNumber;
            DocType = docType;
        }
    }
}
