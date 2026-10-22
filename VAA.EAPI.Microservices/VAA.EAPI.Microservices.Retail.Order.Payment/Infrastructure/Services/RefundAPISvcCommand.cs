using MediatR;
using System.Collections.Generic;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Services
{
    public class RefundAPISvcCommand : IRequest<KeyValuePair<bool, string>>
    {
    }
}
