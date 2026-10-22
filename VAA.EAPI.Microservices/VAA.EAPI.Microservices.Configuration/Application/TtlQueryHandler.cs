using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class TtlQueryHandler : ITtlRules
    {
        private readonly ITtlRulesData _ttlRulesData;

        public TtlQueryHandler(ITtlRulesData ttlRulesData)
        {
            _ttlRulesData = ttlRulesData ?? throw new ArgumentNullException(nameof(ttlRulesData));
        }

        public async Task<IEnumerable<TtlRulesData>> Handle(string fareTypeCode, string fareSubTypeCode, CancellationToken cancellationToken)
        {
            var ttlRulesDetails = await _ttlRulesData.GetTtlRulesData();
            
            if (string.IsNullOrWhiteSpace(fareTypeCode) || !int.TryParse(fareTypeCode, out int fareType))
            {
                return Enumerable.Empty<TtlRulesData>();
            }

            var result = ttlRulesDetails.Where(x => x.FareSubTypeCode == fareSubTypeCode && x.FareTypeCode == fareType).Select(x => new TtlRulesData
            {               
                FareTypeCode = x.FareTypeCode,
                FareSubTypeCode = x.FareSubTypeCode,
                MaxDepartureDate = x.MaxDepartureDate,
                MinDepartureDate = x.MinDepartureDate,
                OperatingCarrier= x.OperatingCarrier,
                TTLBeforeDeparture = x.TTLBeforeDeparture,
                TTLAfterBooking = x.TTLAfterBooking
            });

            return result;
        }
    }
}
