using CSharpFunctionalExtensions;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.ErrorHandling;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Entities;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Interface;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.FCNumberGeneration.Repository
{
    public class FCNumberGenerationRepository : IFCNumberGenerationRepository
    {
        private readonly IFCNumberGenerationService _fCNumberGenerationService;
        private readonly ILogger<FCNumberGenerationRepository> _logger;
        public FCNumberGenerationRepository(IFCNumberGenerationService fCNumberGenerationService, ILogger<FCNumberGenerationRepository> logger)
        {
            _fCNumberGenerationService = fCNumberGenerationService ?? throw new ArgumentNullException(nameof(fCNumberGenerationService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<Result<Response<EnrolmentResponse>>> GenerateFlyingClubNumber()
        {
            Result<Response<EnrolmentResponse>> response = new Result<Response<EnrolmentResponse>>();
            System.Collections.Generic.KeyValuePair<bool, string> result = await _fCNumberGenerationService.GenerateFCNumber();

            if (result.Key)
            {

                response = FCNumGenResponseMapper.MapFCGENAPIResponseToEAPIResponse(Json.Deserialize<FCNumberGenResponse>(result.Value));
                _logger.LogInformation("SCVPlus - Loyalty Number Generation Success");
            }
            else
            {
                _logger.LogInformation("SCVPlus - Loyalty Number Generation Failed");
                throw new FCNumberGenAPIException(Constants.ErrorCodes.FCGenNumberTechnicalException);
            }
            return response;
        }

    }
}
