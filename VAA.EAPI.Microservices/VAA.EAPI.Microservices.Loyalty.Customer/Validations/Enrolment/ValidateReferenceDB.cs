using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Constants;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Channel;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment
{
    public class ValidateReferenceDB : IValidateReferenceDB
    {

        private readonly ILogger<ValidateReferenceDB> _logger;
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public ValidateReferenceDB(ICommonRefDBRepository commonRefDBRepository, ILogger<ValidateReferenceDB> logger)
        {

            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<List<ErrorDetails>> ValidateFromReferenceDB(EnrolmentRequestCommand enrolmentRequest)
        {
            List<Channel> channeldetails = ChannelInformation.GetchannelDetails();
            List<ErrorDetails> validationErrors = new List<ErrorDetails>();
            if (channeldetails.Where(c => c.Code == enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateTitle)
            {

                List<TitleDetails> titleDetails = await _commonRefDBRepository.GetTitleDetails();

                if (!titleDetails.Any(t => t.Title == enrolmentRequest?.Data?.CustomerDetails?.Title))
                {
                    validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidTitle), Guid.NewGuid().ToString());
                    _logger.LogInformation($"Title value is invalid");
                    return validationErrors;
                }
                _logger.LogInformation($"Title value is valid");
            }

            List<MediaCodeDetails> mediaCodeDetails = await _commonRefDBRepository.GetMediaCodeDetails();

            if (!mediaCodeDetails.Any(m => m.MediaCode == enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.MediaCode) && enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember.EnrollmentChannel != ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidMediaCode), Guid.NewGuid().ToString());
                _logger.LogInformation($"MediaCode value is invalid");
                return validationErrors;
            }
            _logger.LogInformation($"MediaCode value is valid");

            string countryCode = enrolmentRequest?.Data?.CustomerDetails?.AddressDetails?.Where(x => x.PreferredFlag == "Y").FirstOrDefault()?.Country;
            if (!string.IsNullOrEmpty(countryCode) && channeldetails.Where(c => c.Code == enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel).FirstOrDefault().ValidateCountry)
            {
                List<CountryDetails> CountryCodeDetails = await _commonRefDBRepository.GetCountryCodeDetails();

                foreach (Address address in enrolmentRequest?.Data?.CustomerDetails?.AddressDetails)
                {
                    if (!CountryCodeDetails.Any(x => x?.Name == address?.Country))
                    {
                        validationErrors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(ErrorCodes.InvalidCountryCode), Guid.NewGuid().ToString());
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
