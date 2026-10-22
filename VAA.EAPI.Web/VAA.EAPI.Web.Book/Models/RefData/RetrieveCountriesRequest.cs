using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Web.Book.Models.RefData
{
    public class RetrieveCountriesRequest : IRequest<IEnumerable<Country>>
    {
        public RetrieveCountriesRequest()
        {
        }
    }
}
