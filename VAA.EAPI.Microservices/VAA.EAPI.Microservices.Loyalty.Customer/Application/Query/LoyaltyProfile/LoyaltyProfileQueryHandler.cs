using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.InflightProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.LoyaltyProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.InflightProfile;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Exceptions.LoyaltyProfile.CustomException;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.LoyaltyProfile
{
    public class LoyaltyProfileQueryCommandHandler : IRequestHandler<LoyaltyProfileQuery, Result<Response<CustomerProfile>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<LoyaltyProfileQueryCommandHandler> _logger;

        public LoyaltyProfileQueryCommandHandler(IMediator mediator, ILogger<LoyaltyProfileQueryCommandHandler> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<Result<Response<CustomerProfile>>> Handle(LoyaltyProfileQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Loyalty Profile Application handler");
            string guid = Guid.NewGuid().ToString();

            //It will be taken care as part of Validation Task
            //var validationErrors = Validations.InflightProfile.ValidatePayload.Validate(request, guid);
            //if (validationErrors.Count > 0)
            //{
            //    _logger.LogInformation($"Loyalty Profile Application handler || ErrorId {guid} || Validation Error");
            //    return Result.Success(new Response<CustomerProfile>
            //    {
            //        Errors = validationErrors,
            //        Status = Status.BadRequest
            //    });
            //}
            try
            {
                var loyaltyProfileDetails = await _mediator.Send(new LoyaltyProfileRepoQuery(request?.MemberNumber), cancellationToken);
                if (loyaltyProfileDetails.Customers.Count > 0)
                {
                    var customer = loyaltyProfileDetails.Customers.First();
                    var customerProfile = await _mediator.Send(new CustomerProfileCommand(customer.CustomerId, customer.Status), CancellationToken.None);
                    if (customerProfile.IsSuccess && customerProfile.Value.Status == Status.OK)
                    {
                        _logger.LogInformation("Loyalty Profile Application handler || Success response from Loyalty Customer Profile");
                        return Result.Success(new Response<CustomerProfile>
                        {
                            Value = customerProfile.Value.Value,
                            Status = Status.OK
                        });
                    }
                    else throw new LoyaltyProfileException(Constants.ErrorCodes.CustomerProfileBusinessException);
                }
                else
                {
                    //to be taken care as part of exception handling
                    throw new LoyaltyProfileException(Constants.ErrorCodes.CustomerProfileBusinessException);
                }
            }


            catch (LoyaltyProfileException lex)
            {
                var errorResponse = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(lex.Message), guid, Constants.ErrorCodes.InflightExceptionFilePath);
                _logger.LogInformation($"Loyalty Profile Application handler || ErrorId {guid} || Business exception from Loyalty Customer Profile");
                return Result.Success(new Response<CustomerProfile>
                {
                    Errors = errorResponse,
                    Status = Status.BusinessException
                });
            }

            catch (Exception e)
            {
                _logger.LogInformation($"Loyalty Profile Application handler || ErrorId {guid} ||  Exception: {e.Message} StackTrace : {e.StackTrace}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, guid, EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                return Result.Failure<Response<CustomerProfile>>(Json.Serialize(errorResponse));
            }
        }
    }
}