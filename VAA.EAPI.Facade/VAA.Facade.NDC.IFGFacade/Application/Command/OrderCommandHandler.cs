using CSharpFunctionalExtensions;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.NDC.IFGFacade.Domain;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Mapper.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Models.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.OrderDataMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.PaymentMS;
using VAA.Facade.NDC.IFGFacade.Infrastructure.Services.ReferenceMS;
using VAA.Facade.NDC.IFGFacade.Mapper;
using VAA.Facade.NDC.IFGFacade.Validations;

namespace VAA.Facade.NDC.IFGFacade.Application.Command
{
    public class OrderCommandHandler(IPaymentMSCaptureCommand paymentMSCaptureCommand, IAirportDetailsQuery airportDetailsQuery, ICurrencyDetailsQuery currencyDetailsQuery, IOrderDataQuery orderDataQuery, Microsoft.Extensions.Logging.ILogger<OrderCommandHandler> logger = null) : IOrderCommand
    {
        private readonly IPaymentMSCaptureCommand _paymentMSCaptureCommand = paymentMSCaptureCommand;
        private readonly IAirportDetailsQuery _airportDetailsQuery = airportDetailsQuery;
        private readonly ICurrencyDetailsQuery _currencyDetailsQuery = currencyDetailsQuery;
        private readonly IOrderDataQuery _orderDataQuery = orderDataQuery;
        private readonly Microsoft.Extensions.Logging.ILogger<OrderCommandHandler> _logger = logger ?? Microsoft.Extensions.Logging.Abstractions.NullLogger<OrderCommandHandler>.Instance;

        public async Task<Result<Response<PaymentResponse>>> Handle(OrderRequest orderRequest, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = PaymentRequestValidations.ValidateOrderRequest(orderRequest);

                if (validationResult.Any())
                {
                    return new Response<PaymentResponse>() { Status = Status.BadRequest, Errors = [.. validationResult] };
                }

                var airportDetails = await _airportDetailsQuery.Handle();

                var currencyDetails = await _currencyDetailsQuery.Handle();

                var pnr = orderRequest.Orders?.FirstOrDefault().FlightRecords.FirstOrDefault().PassengerRecord.Pnr;
                var orderDataRS = await _orderDataQuery.Handle(pnr, cancellationToken);

                var capturePaymentMSRequest = PaymentRequestMapper.MapCaptureRequest(orderRequest, airportDetails, currencyDetails, orderDataRS);
                var paymentMSResponse = await _paymentMSCaptureCommand.Handle(capturePaymentMSRequest);

                var paymentResponse = PaymentResponseMapper.MapPaymentResponse(paymentMSResponse, false);

                if (paymentResponse.Errors == null)
                    return new Response<PaymentResponse>() { Status = Status.OK, Value = paymentResponse };
                else
                    return new Response<PaymentResponse>() { Status = Status.BadRequest, Errors = [.. paymentResponse.Errors] };
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (System.Exception exception)
            {
                _logger.Log(
                    Microsoft.Extensions.Logging.LogLevel.Error,
                    new Microsoft.Extensions.Logging.EventId(0, nameof(OrderCommandHandler)),
                    "Order command failed.",
                    exception,
                    static (state, ex) => state);

                return new Response<PaymentResponse>() { Status = Status.Error, Errors = [new() { Message = "Order command failed." }] };

            }
        }
    }
}
