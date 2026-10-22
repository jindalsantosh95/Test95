using Microsoft.Azure.Functions.Worker;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.ReferenceData.Application.Command.ProductReferenceData;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Triggers.Timer
{
    public class ProductReferenceDataTimer
    {
        private readonly IProductReferenceDataCommandHandler _handler;

        public ProductReferenceDataTimer(IProductReferenceDataCommandHandler handler)
        {
            _handler = handler ?? throw new ArgumentNullException(nameof(handler));
        }

        [Function("ProductReferenceDataTimer")]
        public async Task Run([TimerTrigger("%ProductDetailsPublishSchedule%")] TimerInfo timerInfo, FunctionContext context)
        {
            await _handler.Handle(new ProductReferenceDataCommand(), context.CancellationToken);
        }
    }
}

