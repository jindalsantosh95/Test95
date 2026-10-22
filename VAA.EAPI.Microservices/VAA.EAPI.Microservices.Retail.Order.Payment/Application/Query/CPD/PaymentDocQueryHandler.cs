using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.CPD;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Retail.Order.Payment.Mapper.CPD;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Application.Query.CPD
{
    public class PaymentDocQueryHandler : IRequestHandler<PaymentDocQuery, Response<IEnumerable<PaymentTransactionDocument>>>
    {
        private readonly IMediator _mediator;

        public PaymentDocQueryHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<Response<IEnumerable<PaymentTransactionDocument>>> Handle(PaymentDocQuery request, CancellationToken cancellationToken)
        {
            var capturedPaymentDocs = await _mediator.Send(new PaymentDocRepoQuery { OrderId = request.OrderId, Query = ConfigReader.Get(Constants.Environment.GetPaymentDocQuery) }, cancellationToken);
            var capturedResults = Json.Deserialize<IEnumerable<PaymentTransactionDocument>>(Json.Serialize(capturedPaymentDocs));

            var refundedPaymentDocs = await _mediator.Send(new PaymentDocRepoQuery { OrderId = request.OrderId, Query = ConfigReader.Get(Constants.Environment.GetRefundPaymentDocQuery) }, cancellationToken);
            var refundResults = refundedPaymentDocs.Select(doc => PaymentDocumentMapper.RefundPaymentDocMapper(Json.Deserialize<RefundTransactionDocument>(Json.Serialize(doc))));

            var result = capturedResults.Concat(refundResults);

            return new Response<IEnumerable<PaymentTransactionDocument>> { Status = Status.OK, Value = result };

        }
    }
}
