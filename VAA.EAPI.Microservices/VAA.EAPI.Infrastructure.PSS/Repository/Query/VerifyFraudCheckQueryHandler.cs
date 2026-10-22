using MediatR;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.PSS.Mapper;
using VAA.EAPI.Infrastructure.PSS.Services.Interfaces;

namespace VAA.EAPI.Infrastructure.PSS.Repository.Query
{
    public class VerifyFraudCheckQueryHandler : IRequestHandler<VerifyFraudCheckQuery, string>
    {
        private readonly ICreditCardV2 _iCreditCardV2;

        public VerifyFraudCheckQueryHandler(ICreditCardV2 iCreditCardV2)
        {
            _iCreditCardV2 = iCreditCardV2 ?? throw new ArgumentNullException(nameof(iCreditCardV2));
        }

        public async Task<string> Handle(VerifyFraudCheckQuery request, CancellationToken cancellationToken)
        {
            var response = await _iCreditCardV2.RetrieveEnhancedCreditCardAcceptance(CreditCardV2Mapper.MapRetrieveEnhancedCreditCardAcceptanceRequest(request));
            return CreditCardV2Mapper.MapRetrieveEnhancedCreditCardAcceptanceResponse(response);
        }
    }
}
