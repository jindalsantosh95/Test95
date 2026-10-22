using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Validations;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch
{
    public class CustomerSearchByEmailQueryHandler : IRequestHandler<CustomerSearchByEmailQuery, Result<Response<CustomerTierInfo>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerSearchByEmailQueryHandler> _logger;

        public CustomerSearchByEmailQueryHandler(IMediator mediator, ILogger<CustomerSearchByEmailQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<CustomerTierInfo>>> Handle(CustomerSearchByEmailQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null || request.EmailID.IsNullOrWhiteSpace())
                {
                    throw new ArgumentNullException(nameof(request.EmailID));
                }

                var checkEmailResponse = await _mediator.Send(new CheckEmailQuery() { Email = request.EmailID });

                return Result.Success(new Response<CustomerTierInfo>
                {
                    Status = Status.OK,
                    Value = new CustomerTierInfo
                    {
                        Tier = checkEmailResponse.Tier ?? string.Empty
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Customer Search By Email handler Exception: {ex.Message} StackTrace : {ex.StackTrace}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                return Result.Failure<Response<CustomerTierInfo>>(Json.Serialize(errorResponse));
            }
        }
    }
}
