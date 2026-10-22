using MediatR;
using System.Collections.Generic;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Reference.Domain;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class GetCountryDetailsQuery : IRequest<Response<IEnumerable<CountryDetails>>>
    {
    }
}
