using CSharpFunctionalExtensions;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.HoHAccount;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Validations;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Application.Query
{
    public class HouseholdDetailsRetrievalQueryHandler(IGroupDetailsService groupDetailsService) : IHouseholdDetailsRetrievalQuery
    {
        private readonly IGroupDetailsService _groupDetailsService = groupDetailsService ?? throw new ArgumentNullException(nameof(groupDetailsService));

        public async Task<Result<Response<HouseholdDetailsResponse>>> Handle(HouseholdDetailsRetrievalQuery request, CancellationToken cancellationToken)
        {
            try
            {

                if (string.IsNullOrWhiteSpace(request.CustomerId))
                {
                    throw new ArgumentNullException(
                        nameof(request),
                        "Request cannot be null.");
                }

                var householdDetailsRequestValidator = new HouseholdDetailsQueryValidator().Validate(request);

                if (householdDetailsRequestValidator.IsValid)
                {
                    return await _groupDetailsService.GetHouseholdAccountDetails(request.CustomerId, cancellationToken);
                }
                else
                {
                    return Result.Failure<Response<HouseholdDetailsResponse>>(Json.Serialize(ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)));
                }
            }
            catch (ArgumentException ex)
            {
                return Result.Failure<Response<HouseholdDetailsResponse>>(ex.Message);
            }
            catch (Exception ex)
            {
                return Result.Failure<Response<HouseholdDetailsResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

    }

}
