using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Configuration.Domain;
using VAA.EAPI.Microservices.Configuration.Infrastructure.Interface;

namespace VAA.EAPI.Microservices.Configuration.Application
{
    public class CorporateQueryHandler : ICorporateDetails
    {
        private readonly ICorporateData _corporateData;

        public CorporateQueryHandler(ICorporateData corporateData)
        {
            _corporateData = corporateData ?? throw new ArgumentNullException(nameof(corporateData));
        }

        public async Task<CorporateData> Handle(string cbaClid, CancellationToken cancellationToken)
        {
           
            var list = await _corporateData.GetCorporateData(cancellationToken);
            var entity = list.FirstOrDefault(x => x.CbaClid.ToString() == cbaClid);
            
            if (entity == null)
            {
                return null;
            }

            return (CorporateData)entity;
        }
    }
}