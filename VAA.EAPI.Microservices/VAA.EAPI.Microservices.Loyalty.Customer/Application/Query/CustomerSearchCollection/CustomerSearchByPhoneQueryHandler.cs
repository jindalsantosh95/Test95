using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchCollection;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearchCollection
{
    public class CustomerSearchByPhoneQueryHandler : IRequestHandler<CustomerSearchByPhoneQuery, Result<Response<IEnumerable<Infrastructure.Entities.CustomerSearchCollection.CustomerSearch>>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerSearchByPhoneQueryHandler> _logger;

        public CustomerSearchByPhoneQueryHandler(IMediator mediator, ILogger<CustomerSearchByPhoneQueryHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<IEnumerable<Infrastructure.Entities.CustomerSearchCollection.CustomerSearch>>>> Handle(CustomerSearchByPhoneQuery request, CancellationToken cancellationToken)
        {
            try
            {
                if (request == null || request.PhoneNumber.IsNullOrWhiteSpace())
                {
                    throw new ArgumentNullException(nameof(request.PhoneNumber));
                }

                return Result.Success(new Response<IEnumerable<Infrastructure.Entities.CustomerSearchCollection.CustomerSearch>>
                {
                    Status = Status.OK,
                    Value = await _mediator.Send(new CustomerSearchColRepoQuery(request), cancellationToken)
                });
            }
            catch (Exception e)
            {
                _logger.LogError(e, $"Customer Search by Phonenumber handler Exception: {e.Message}");
                return Result.Failure<Response<IEnumerable<Infrastructure.Entities.CustomerSearchCollection.CustomerSearch>>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }
    }
}
