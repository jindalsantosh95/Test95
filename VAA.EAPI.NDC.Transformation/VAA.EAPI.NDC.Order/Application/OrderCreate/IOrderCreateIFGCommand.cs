using CSharpFunctionalExtensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderViewRS;

namespace VAA.EAPI.NDC.Order.Application.OrderCreate
{
    public interface IOrderCreateIFGCommand
    {
        Task<Result<Response<IATA_OrderViewRS>>> Handle(IATA_OrderCreateRQ request, string subscriptionKey, CancellationToken cancellationToken);
    }
}
