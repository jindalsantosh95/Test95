using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Models;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class ItaFareTypeCodeQueryHandler : IitaFareTypeCodeDetails
    {
        private readonly IitaFareTypeCodeData _itaFareTypeCodeData;

        public ItaFareTypeCodeQueryHandler(IitaFareTypeCodeData ItaFareTypeCodeData)
        {
            _itaFareTypeCodeData = ItaFareTypeCodeData ?? throw new ArgumentNullException(nameof(_itaFareTypeCodeData));
        }

        public async Task<IEnumerable<ItaFareTypeCodeDbEntity>> Handle(string fareTypeCode, string fareSubTypeCode, CancellationToken cancellationToken)
        {
            var fareTypeCodeDetails = await _itaFareTypeCodeData.GetFareTypeCodeData();

            var result = fareTypeCodeDetails.Select(x => new ItaFareTypeCodeDbEntity
            {
                FareType = x.FareType,
                FareTypeCode = x.FareTypeCode,
                FareSubTypeCode = x.FareSubTypeCode,
                AccountCode = x.AccountCode,
                NdcPaxType = x.NdcPaxType,
                ItaPaxType = x.ItaPaxType
            });

            return result;
        }
    }
}
