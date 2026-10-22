using CSharpFunctionalExtensions;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class FareFamilyQueryHandler : IRequestHandler<FareFamilyQuery, IEnumerable<FareFamilyList>>
    {
        private readonly IFareFamilyListService _fareFamilyListService;
        public FareFamilyQueryHandler(IFareFamilyListService FareFamilyListService)
        {
            _fareFamilyListService = FareFamilyListService ?? throw new ArgumentNullException(nameof(_fareFamilyListService));
        }

        public async Task<IEnumerable<FareFamilyList>> Handle(FareFamilyQuery request, CancellationToken cancellationToken)
        {
            var fareFamilies = await _fareFamilyListService.GetFareFamily();
            return fareFamilies.Select(x => (FareFamilyList)x);
        }
    }
}
