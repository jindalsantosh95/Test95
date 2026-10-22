using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.OrderData;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.ReferenceMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.OrderDataMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS;

namespace VAA.Facade.NDC.IFGFacade.Tests
{
    internal sealed class ThrowingAirportDetailsQuery(Exception exception) : IAirportDetailsQuery
    {
        public Task<IEnumerable<AirportDetail>> Handle() => Task.FromException<IEnumerable<AirportDetail>>(exception);
    }

    internal sealed class ThrowingCurrencyDetailsQuery(Exception exception) : ICurrencyDetailsQuery
    {
        public Task<IEnumerable<Currency>> Handle() => Task.FromException<IEnumerable<Currency>>(exception);
    }

    internal sealed class ThrowingOrderDataQuery(Exception exception) : IOrderDataQuery
    {
        public Task<OrderRS> Handle(string orderId, CancellationToken cancellationToken) => Task.FromException<OrderRS>(exception);
    }

    internal sealed class ThrowingAuthorizeCommand(Exception exception) : IPaymentMSAuthorizeCommand
    {
        public Task<PaymentMSResponse> Handle(AuthorizePaymentRequest request) => Task.FromException<PaymentMSResponse>(exception);
    }

    internal sealed class ThrowingCaptureCommand(Exception exception) : IPaymentMSCaptureCommand
    {
        public Task<PaymentMSResponse> Handle(CapturePaymentRequest request) => Task.FromException<PaymentMSResponse>(exception);
    }

    internal sealed class ThrowingRefundCommand(Exception exception) : IPaymentMSRefundCommand
    {
        public Task<PaymentMSResponse> Handle(RefundPaymentRequest request) => Task.FromException<PaymentMSResponse>(exception);
    }
}
