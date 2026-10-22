using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Query;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Constants;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Infrastructure.Interface;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Mapper;
using VAA.EAPI.Microservices.Loyalty.AddressLookup.Validation;

namespace VAA.EAPI.Microservices.Loyalty.AddressLookup.Application.Command.GbgService
{
    public class AddressLookupService : IAddressLookupService
    {
        private readonly IGbgService _gbgService;

        public AddressLookupService(IGbgService gbgService)
        {
            _gbgService = gbgService ?? throw new ArgumentNullException(nameof(gbgService));
        }

        public async Task<Response<IEnumerable<AddressDetail>>> GetAllAddresses(string countryCode, string postalCode, string addressLine)
        {
            var validationResult = ValidateAddressDetailsRequestV2.AddressDetailsRequestValidator(new AddressLookupQueryV2(countryCode, postalCode, addressLine));

            if (validationResult.Value.Status == Status.OK)
            {
                List<AddressDetail> addressDetails = new();
                try
                {
                    var gbgFindResponse = await _gbgService.Find($"{addressLine} {postalCode}", countryCode, new CancellationToken());

                    if (gbgFindResponse?.Items?.Any() == true)
                    {
                        var retrieveTasks = gbgFindResponse.Items.Where(item => item.Type == "Address").Take(ConfigReader.Get<int>(Constants.Environment.GbgSearchRecordLimit))
                                            .Select(async item =>
                                            {
                                                var addressResult = await _gbgService.Retrieve(item.Id, new CancellationToken());
                                                if (addressResult != null)
                                                {
                                                    return AddressDetailDataMapper.GetAddressDetails(addressResult);
                                                }
                                                return null;
                                            });

                        var gbgRetrieveResult = await Task.WhenAll(retrieveTasks);
                        addressDetails.AddRange(gbgRetrieveResult.Where(r => r != null).SelectMany(r => r));

                        if (addressDetails.Any())
                        {
                            return new Response<IEnumerable<AddressDetail>>
                            {
                                Status = Status.OK,
                                Value = addressDetails
                            };
                        }
                        return new Response<IEnumerable<AddressDetail>>() { Status = Status.BusinessException, Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.NoAddressFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) };

                    }
                    return new Response<IEnumerable<AddressDetail>>() { Status = Status.BusinessException, Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.NoAddressFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) };
                }
                catch (Exception)
                {
                    return new Response<IEnumerable<AddressDetail>>()
                    {
                        Status = Status.OK,
                        Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.GbgServiceError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    };
                }
            }
            else
            {
                return new Response<IEnumerable<AddressDetail>>
                {
                    Errors = validationResult.Value.Errors,
                    Status = Status.BusinessException

                };
            }
        }

        public async Task<Response<IEnumerable<AddressDetail>>> FindAddresses(string addressText, string countryCode)
        {
            try
            {
                var gbgFindResponse = await _gbgService.Find(addressText, countryCode, new CancellationToken());
                if (gbgFindResponse != null)
                {
                    return new Response<IEnumerable<AddressDetail>>
                    {
                        Status = Status.OK,
                        Value = AddressDetailDataMapper.GetAddressDetails(gbgFindResponse)
                    };
                }
                return new Response<IEnumerable<AddressDetail>>() { Status = Status.BusinessException, Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.NoAddressFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) };
            }
            catch (Exception)
            {
                return new Response<IEnumerable<AddressDetail>>()
                {
                    Status = Status.OK,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.GbgServiceError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }

        public async Task<Response<IEnumerable<AddressDetail>>> RetrieveAddress(string addressId)
        {
            try
            {
                var gbgRetrieveResponse = await _gbgService.Retrieve(addressId, new CancellationToken());
                if (gbgRetrieveResponse != null)
                {
                    return new Response<IEnumerable<AddressDetail>>
                    {
                        Status = Status.OK,
                        Value = AddressDetailDataMapper.GetAddressDetails(gbgRetrieveResponse)
                    };
                }
                return new Response<IEnumerable<AddressDetail>>() { Status = Status.BusinessException, Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.NoAddressFound, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath) };
            }
            catch (Exception)
            {
                return new Response<IEnumerable<AddressDetail>>()
                {
                    Status = Status.OK,
                    Errors = ErrorMapper.MapTechnicalExceptionToEAPIException(ErrorCodes.GbgServiceError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                };
            }
        }
    }
}
