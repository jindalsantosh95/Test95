using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Exceptions;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.PSS.Repository;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain.FlightCustomers;
using VAA.EAPI.Microservices.Retail.Order.Search.Mapper.FlightBookingCustomer;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Application.Query.FlightBookingCustomer
{
    public class FlightBookingCustomerQueryHandler : IRequestHandler<FlightBookingCustomerQuery, Result<Response<FlightCustomer>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<FlightBookingCustomerQueryHandler> _logger;

        public FlightBookingCustomerQueryHandler(IMediator mediator, ILogger<FlightBookingCustomerQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<FlightCustomer>>> Handle(FlightBookingCustomerQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Flight booking customer history retrieval started for {PnrLocator} and {OrderId}", request?.Pnr, request?.OrderId);
            try
            {
                List<Customer> customers = new();
                var pnrRetrieveResponse = await _mediator.Send(new PNRRetrieveHistoryRepoQuery(request.Pnr), cancellationToken);
                var customersDetails = CustomerDetailMapper.PassengerToCustomerMapper(pnrRetrieveResponse?.PnrType?.Passengers, pnrRetrieveResponse.PnrType?.InfantInArms ?? new List<InfantInArms>(), pnrRetrieveResponse?.PnrType?.Ptcs);

                foreach (var customer in customersDetails)
                {
                    var sessionId = await _mediator.Send(new PNRSetupContextRepoQuery { RecordLocator = request.Pnr }, cancellationToken);
                    try
                    {
                        var passengerV4Response = await _mediator.Send(new PassengerV4ManageMultipleTravelDocsAndSecureFlightRepoQuery() { Customer = customer, SessionId = sessionId, RetrieveFlag = true }, cancellationToken);

                        passengerV4Response.CustId = customer.CustId;
                        passengerV4Response.Ptc = customer.Ptc;
                        customers.Add(passengerV4Response);
                    }
                    catch (ExternalServiceIntegrationException)
                    {
                        _logger.LogError("Delta Passenger V4 travel-document retrieval failed for {PnrLocator} and {OrderId}", request?.Pnr, request?.OrderId);
                        return Result.Failure<Response<FlightCustomer>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
                    }
                }

                if (customers.Count > 0)
                {
                    return Result.Success(new Response<FlightCustomer>
                    {
                        Status = Status.OK,
                        Value = FlightCustomerBookingRSMapper.CustomerDetailMapper(customers)
                    });
                }

                return Result.Success(new Response<FlightCustomer>
                {
                    Status = Status.Error,
                    Value = new FlightCustomer()
                });
            }
            catch (Exception ex)
            {
                _logger.LogError("Delta flight booking customer processing failed for {PnrLocator} and {OrderId}; exception type {ExceptionType}", request?.Pnr, request?.OrderId, ex.GetType().Name);
                return Result.Failure<Response<FlightCustomer>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
