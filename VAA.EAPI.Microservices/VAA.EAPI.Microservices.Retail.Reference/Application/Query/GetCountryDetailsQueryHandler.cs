using MediatR;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Reference.Domain;
using VAA.EAPI.Microservices.Retail.Reference.Domain.Interface;

namespace VAA.EAPI.Microservices.Retail.Reference.Application.Query
{
    public class GetCountryDetailsQueryHandler : IRequestHandler<GetCountryDetailsQuery, Response<IEnumerable<CountryDetails>>>
    {
        private readonly IScvRefDBRepository _scvRefDBRepository;

        public GetCountryDetailsQueryHandler(IScvRefDBRepository scvRefDBRepository)
        {
            _scvRefDBRepository = scvRefDBRepository ?? throw new ArgumentNullException(nameof(scvRefDBRepository));
        }

        public async Task<Response<IEnumerable<CountryDetails>>> Handle(GetCountryDetailsQuery request, CancellationToken cancellationToken)
        {
            return new Response<IEnumerable<CountryDetails>>()
            {
                Status = Status.OK,
                Value = await _scvRefDBRepository.GetCountryCodeDetails(),
                Errors = null
            };
        }
    }
}
