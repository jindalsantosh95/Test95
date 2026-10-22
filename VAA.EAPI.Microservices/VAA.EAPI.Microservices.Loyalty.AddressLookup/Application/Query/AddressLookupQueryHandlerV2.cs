using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity.VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Entity;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Constants;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Validation;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Query
{
    public class AddressLookupQueryHandlerV2 : IRequestHandler<AddressLookupQueryV2, Result<Response<List<AddressDetail>>>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<AddressLookupQueryHandlerV2> _logger;

        public AddressLookupQueryHandlerV2(IMediator mediator, ILogger<AddressLookupQueryHandlerV2> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Response<List<AddressDetail>>>> Handle(AddressLookupQueryV2 request, CancellationToken cancellationToken)
        {
            var validationResult = ValidateAddressDetailsRequestV2.AddressDetailsRequestValidator(request);

            if (validationResult.Value.Status == Status.OK)
            {
                try
                {
                    List<CompleteAddressDetails> completeAddressDetails = await _mediator.Send(new AddressDetailsRepoCommandV2(EnumOperations.GetEnumDescription((Country)Enum.Parse(typeof(Country), request.CountryCode)), request.PostCode, request.AddressLine1), cancellationToken);
                    List<AddressDetail> addressDetails = completeAddressDetails != null ? AddressDetailDataMapper.GetAddressDetails(completeAddressDetails, request) : null;

                    if (addressDetails == null || addressDetails.Count == 0)
                        return new Response<List<AddressDetail>>() { Status = Status.BusinessException, Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.NoAddressFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) };

                    return new Response<List<AddressDetail>>()
                    {
                        Status = Status.OK,
                        Value = addressDetails
                    };
                }

                catch (Exception ex)
                {
                    _logger.LogError(ex, $"ValidateAddress-Error--WesalRepository.wesalResponse- {ex.Message}");
                    return Result.Failure<Response<List<AddressDetail>>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.GbgServiceError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
            }
            else
            {
                return Result.Success(new Response<List<AddressDetail>>
                {
                    Errors = validationResult.Value.Errors,
                    Status = Status.BusinessException

                });
            }
        }
    }
}