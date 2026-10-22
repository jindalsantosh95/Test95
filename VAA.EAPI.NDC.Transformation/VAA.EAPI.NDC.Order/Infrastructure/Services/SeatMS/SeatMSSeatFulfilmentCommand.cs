using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.SeatMS
{
    public class SeatMSSeatFulfilmentCommand : IRequest<SeatFulfilmentResponse>
    {
        public SeatFulfilmentRequest SeatFulfilmentRequest { get; set; }
    }
}
