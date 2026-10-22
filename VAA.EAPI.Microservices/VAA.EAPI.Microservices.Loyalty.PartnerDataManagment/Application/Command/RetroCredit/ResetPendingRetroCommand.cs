using MediatR;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Application.Command.RetroCredit
{
    public class ResetPendingRetroCommand : IRequest<string>
    {
        public string Days { get; set; }
    }

}