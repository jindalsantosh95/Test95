using MediatR;
using VAA.EAPI.Web.Book.Models.RefData;

namespace VAA.EAPI.Web.Book.Domain.RefData.Requests
{
    public class CountriesRetrieveRequest : IRequest<System.Collections.Generic.IEnumerable<Country>>
    {
    }
}
