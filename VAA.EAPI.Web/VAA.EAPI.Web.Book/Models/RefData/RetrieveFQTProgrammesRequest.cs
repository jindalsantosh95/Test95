using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.RefData
{
    public class RetrieveFQTProgrammesRequest : IRequest<IEnumerable<FQTProgramme>>
    {
        public RetrieveFQTProgrammesRequest()
        {

        }
    }
}