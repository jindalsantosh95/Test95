using MediatR;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.RulesEngine.Domain.Entities.EarnCalculator;

namespace VAA.EAPI.Microservices.Loyalty.RulesEngine.Application.Command
{
    public class EarnAirlineAccrualCalculatorCommand : IRequest<Response<EarnAirlineAccrualCalculatorResponse>>
    {
        public EarnAirlineAccrualCalculatorRequest EarnAirlineAccrualCalculatorRequest { get; set; }

        public EarnAirlineAccrualCalculatorCommand(EarnAirlineAccrualCalculatorRequest earnAirlineAccrualCalculatorRequest)
        {
            EarnAirlineAccrualCalculatorRequest = earnAirlineAccrualCalculatorRequest;
        }
    }
}
