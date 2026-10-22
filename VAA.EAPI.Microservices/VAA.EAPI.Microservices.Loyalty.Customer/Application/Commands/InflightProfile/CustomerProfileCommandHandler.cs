using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Validations.InflightProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Exceptions.InflightCustomer.CustomException;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.InflightProfile
{
    public class CustomerProfileCommandHandler : IRequestHandler<CustomerProfileCommand, Result<Response<CustomerProfile>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerProfileCommandHandler> _logger;

        public CustomerProfileCommandHandler(IMediator mediator, ILogger<CustomerProfileCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<Response<CustomerProfile>>> Handle(CustomerProfileCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Customer Profile Application handler");
            string guid = Guid.NewGuid().ToString();
            var validationErrors = ValidatePayload.Validate(request, guid);
            if (validationErrors.Count > 0)
            {
                _logger.LogInformation($"Customer Profile Application handler || ErrorId {guid} || Validation Error");
                return Result.Success(new Response<CustomerProfile>
                {
                    Errors = validationErrors,
                    Status = Status.BadRequest
                });
            }
            try
            {
                CustomerProfile customerProfile = await _mediator.Send(new CustomerProfileRepoQuery(request?.CustomerId), cancellationToken);
                _logger.LogInformation("Customer Profile Application handler || Success response from Inflight Customer Profile");
                return Result.Success(new Response<CustomerProfile>
                {
                    Value = customerProfile,
                    Status = Status.OK
                });
            }

            catch (CustomerProfileException cex)
            {
                var errorResponse = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(cex.Message), guid, ErrorCodes.InflightExceptionFilePath);
                _logger.LogInformation($"Customer Profile Application handler || ErrorId {guid} || Business exception from Inflight Customer Profile");
                return Result.Success(new Response<CustomerProfile>
                {
                    Errors = errorResponse,
                    Status = Status.BusinessException
                });
            }

            catch (Exception e)
            {
                _logger.LogInformation($"Customer Profile Application handler || ErrorId {guid} ||  Exception: {e.Message} StackTrace : {e.StackTrace}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, guid, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                return Result.Failure<Response<CustomerProfile>>(Json.Serialize(errorResponse));
            }
        }
    }
}