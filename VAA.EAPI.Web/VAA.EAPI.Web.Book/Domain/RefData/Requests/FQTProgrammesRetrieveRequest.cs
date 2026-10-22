using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Web.Book.Models.RefData;

namespace VAA.EAPI.Web.Book.Domain.RefData.Requests
{
    public class FQTProgrammesRetrieveRequest : IRequest<IEnumerable<FQTProgramme>>
    {
    }
}