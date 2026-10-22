using MediatR;
using VAA.EAPI.Microservices.Retail.Order.Data.Domain.Entities.RefundUpdate;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Repositories.RefundUpdate
{
    public class RefundUpdateRepoCommand : IRequest<Unit>
    {
        public VoidOrderUpdateData VoidOrderUpdateData { get; set; }

        public RefundUpdateRepoCommand(VoidOrderUpdateData voidOrderUpdateData)
        {
            VoidOrderUpdateData = voidOrderUpdateData;
        }
    }
}
