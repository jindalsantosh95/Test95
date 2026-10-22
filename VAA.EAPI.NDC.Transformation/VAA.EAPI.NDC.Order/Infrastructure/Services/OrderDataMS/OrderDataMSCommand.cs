using CSharpFunctionalExtensions;
using MediatR;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;

namespace VAA.EAPI.NDC.Order.Infrastructure.Services.OrderDataMS
{
    public class OrderDataMSCommand : IRequest<Result<Unit>>
    {
        public OrderDocument OrderDocument { get; set; }
    }
}
