using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Channel;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IValidation;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Validations
{
    public class ValidateReferenceDB : IValidateReferenceDB
    {

        private readonly ILogger<EnrolmentEventCommandHandler> _logger;
        private readonly ICommonRefDBRepository _commonRefDBRepository;


        public ValidateReferenceDB(ICommonRefDBRepository commonRefDBRepository, ILogger<EnrolmentEventCommandHandler> logger)
        {

            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<List<ErrorDetails>> ValidateFromReferenceDB(EnrolmentRequest enrolmentRequest)
        {
            List<Channel> channeldetails = ChannelInformation.GetchannelDetails();
            List<ErrorDetails> validationErrors = new List<ErrorDetails>();
            if (channeldetails.Where(c => c.Code == enrolmentRequest?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateTitle)
            {

                List<Domain.CommonReferenceDetails.TitleDetails> titleDetails = await _commonRefDBRepository.GetTitleDetails();

                if (!titleDetails.Any(t => t.Title == enrolmentRequest?.CustomerDetails?.Title))
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidTitle), Guid.NewGuid().ToString());
                    _logger.LogInformation($"Title value is invalid");
                    return validationErrors;
                }
                _logger.LogInformation($"Title value is valid");
            }

            List<Domain.CommonReferenceDetails.MediaCodeDetails> mediaCodeDetails = await _commonRefDBRepository.GetMediaCodeDetails();

            if (!mediaCodeDetails.Any(m => m.MediaCode == enrolmentRequest?.CustomerDetails?.LoyaltyMember?.MediaCode) && enrolmentRequest.CustomerDetails.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidMediaCode), Guid.NewGuid().ToString());
                _logger.LogInformation($"MediaCode value is invalid");
                return validationErrors;
            }
            _logger.LogInformation($"MediaCode value is valid");

            string countryCode = enrolmentRequest?.CustomerDetails?.AddressDetails?.Where(x => x.PreferredFlag == "Y").FirstOrDefault()?.Country;
            if (!string.IsNullOrEmpty(countryCode) && channeldetails.Where(c => c.Code == enrolmentRequest?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateCountry)
            {
                List<Domain.CommonReferenceDetails.CountryDetails> CountryCodeDetails = await _commonRefDBRepository.GetCountryCodeDetails();

                foreach (Address address in enrolmentRequest?.CustomerDetails?.AddressDetails)
                {
                    if (!CountryCodeDetails.Any(x => x?.Name == address?.Country))
                    {
                        validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidCountryCode), Guid.NewGuid().ToString());
                        _logger.LogInformation($"CountryCode value is invalid");
                        return validationErrors;
                    }
                    _logger.LogInformation($"CountryCode value is valid");
                }
            }
            return validationErrors;
        }


    }
}
