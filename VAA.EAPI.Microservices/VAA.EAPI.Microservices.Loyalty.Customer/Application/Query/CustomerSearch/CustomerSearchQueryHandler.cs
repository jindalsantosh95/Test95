using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Validations;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch
{
    public class CustomerSearchQueryHandler : IRequestHandler<CustomerSearchQuery, Result<Response<Infrastructure.Entities.CustomerSearch>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerSearchQueryHandler> _logger;

        public CustomerSearchQueryHandler(IMediator mediator, ILogger<CustomerSearchQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<Infrastructure.Entities.CustomerSearch>>> Handle(CustomerSearchQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (new CustomerSearchRetrieveValidator().Validate(request).IsValid)
                {
                    var customerSearchDocument = await _mediator.Send(new CustomerSearchRepoQuery(request), cancellationToken);
                    return Result.Success(new Response<Infrastructure.Entities.CustomerSearch>
                    {
                        Status = Status.OK,
                        Value = customerSearchDocument
                    });
                }
                else
                {
                    return Result.Success(new Response<Infrastructure.Entities.CustomerSearch>
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogInformation($"Customer Search Application handler Exception: {e.Message} StackTrace : {e.StackTrace}");
                var errorResponse = ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath);
                return Result.Failure<Response<Infrastructure.Entities.CustomerSearch>>(Json.Serialize(errorResponse));
            }
        }
    }
}
