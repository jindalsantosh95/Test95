using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IValidation;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Helper.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CheckEmailAndTier;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment;
using CustomerTypeEnum = VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.EnrolCustomerDetails.CustomerTypeEnum;
using Preferences = VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails.Preferences;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class EnrolmentRequestCommandHandler : IRequestHandler<EnrolmentRequestCommand, Result<Response<EnrolmentResponse>>>
    {
        private readonly ILogger<EnrolmentRequestCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly IValidateReferenceDB _validateReferenceDB;
        private readonly IDeDupValidationEnrolment _deDupValidation;
        private readonly IFCNumberGenerationRepository _fCNumberGenerationRepository;
        private readonly IErrorMapper _errorMapper;
        private readonly ICommonRefDBRepository _commonRefDBRepository;
        private readonly IBlobStorageServices _iBlobStorageServices;

        public EnrolmentRequestCommandHandler(IMediator mediator,
                                            ILogger<EnrolmentRequestCommandHandler> logger,
                                            IValidateReferenceDB validateReferenceDB,
                                            IDeDupValidationEnrolment deDupValidation,
                                            IErrorMapper errorMapper,
                                            IFCNumberGenerationRepository fCNumberGenerationRepository,
                                            ICommonRefDBRepository commonRefDBRepository,
                                            IBlobStorageServices iBlobStorageServices)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validateReferenceDB = validateReferenceDB ?? throw new ArgumentNullException(nameof(validateReferenceDB));
            _deDupValidation = deDupValidation ?? throw new ArgumentNullException(nameof(deDupValidation));
            _errorMapper = errorMapper ?? throw new ArgumentNullException(nameof(errorMapper));
            _fCNumberGenerationRepository = fCNumberGenerationRepository ?? throw new ArgumentNullException(nameof(fCNumberGenerationRepository));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _iBlobStorageServices = iBlobStorageServices ?? throw new ArgumentNullException(nameof(iBlobStorageServices));
        }

        public async Task<Result<Response<EnrolmentResponse>>> Handle(EnrolmentRequestCommand enrolmentRequest, CancellationToken cancellationToken)
        {

            try
            {
                Result<Response<EnrolmentResponse>> result;
                if (enrolmentRequest?.Data?.CustomerDetails?.CustomerType == CustomerTypeEnum.Basic)
                    result = ValidateBasicRequest(enrolmentRequest);
                else
                    result = ValidateRequest(enrolmentRequest);

                if (result.Value.Status == Status.OK)
                {
                    //copy logic from enrolment event subscriber or build a command hanlder to do so
                    Result<Response<EnrolmentResponse>> enrolmentResponse = null;

                    string actualTitle = enrolmentRequest?.Data?.CustomerDetails?.Title;
                    string abbreviatedTitle = enrolmentRequest?.Data?.CustomerDetails?.Title;

                    if (enrolmentRequest?.Data?.CustomerDetails?.FirstName != null)
                        enrolmentRequest.Data.CustomerDetails.FirstName = RegExHelper.ConvertToTitle(enrolmentRequest?.Data?.CustomerDetails?.FirstName);

                    if (enrolmentRequest?.Data?.CustomerDetails?.LastName != null)
                        enrolmentRequest.Data.CustomerDetails.LastName = RegExHelper.ConvertToTitle(enrolmentRequest?.Data?.CustomerDetails?.LastName);

                    if (enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.DeltaChannelID) || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebMyaChannelId) || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.AppMyaChannelId))
                    {
                        actualTitle = await _commonRefDBRepository.GetTitleValue(enrolmentRequest?.Data?.CustomerDetails?.Title);
                        if (actualTitle == null)
                        {
                            _logger.LogWarning("Title in the request is not available in reference data");
                            actualTitle = enrolmentRequest.Data.CustomerDetails.Title;
                        }

                    }

                    if (enrolmentRequest?.Data?.CustomerDetails?.CustomerType == CustomerTypeEnum.Basic)
                        enrolmentResponse = await EnrolBasicCustomer(enrolmentRequest, actualTitle, abbreviatedTitle);
                    else
                        enrolmentResponse = await EnrolCustomer(enrolmentRequest, actualTitle, abbreviatedTitle);

                    return enrolmentResponse;
                }
                else
                {
                    return result;
                }
            }
            catch (Exception ex)
            {
                _logger.LogInformation($"Exception occoured while enrolling- {ex}");
                return Result.Failure<Response<EnrolmentResponse>>(ex.InnerException.ToString());
            }
        }

        private Result<Response<EnrolmentResponse>> ValidateBasicRequest(EnrolmentRequestCommand enrolmentRequest)
        {
            var response = new Response<EnrolmentResponse>()
            {
                Status = Status.OK,
                Value = null,
                Errors = null
            };

            if (string.IsNullOrEmpty(enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel) || string.IsNullOrEmpty(Convert.ToString(enrolmentRequest?.Data?.CustomerDetails?.CustomerType)))
            {
                response = new Response<EnrolmentResponse>()
                {
                    Status = Status.BusinessException,
                    Value = null,
                    Errors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString())
                };
            }

            return response;
        }
        private Result<Response<EnrolmentResponse>> ValidateRequest(EnrolmentRequestCommand enrolmentRequest)
        {
            var response = new Response<EnrolmentResponse>()
            {
                Status = Status.OK,
                Value = null,
                Errors = null
            };

            if (string.IsNullOrEmpty(enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel))
            {
                response = new Response<EnrolmentResponse>()
                {
                    Status = Status.BusinessException,
                    Value = null,
                    Errors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidChannelError), Guid.NewGuid().ToString())
                };
            }
            else if (enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel == ConfigReader.Get(Constants.Environment.VredChannelId))
            {
                var errors = VredValidations.Validate(enrolmentRequest);
                if (errors.Any())
                {
                    response = new Response<EnrolmentResponse>()
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = errors
                    };
                }
            }
            else if (!new EnrolmentCommandValidator().Validate(enrolmentRequest).IsValid)
            {
                response = new Response<EnrolmentResponse>()
                {
                    Status = Status.BusinessException,
                    Value = null,
                    Errors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.MandatoryInputFieldsMissing), Guid.NewGuid().ToString())
                };
            }
            if(response.Status == Status.OK)
            {
                var preferences = enrolmentRequest?.Data?.CustomerDetails?.Preferences ?? Enumerable.Empty<Preferences>();
                var preferredPartner = preferences.FirstOrDefault(x => x.PreferenceType == Preferences.PreferenceTypes.PreferredPartnerFFP);
                if (preferredPartner != null && !string.IsNullOrEmpty(preferredPartner.PreferenceValue) && preferredPartner.PreferenceValue.Length > 3)
                {
                    response = new Response<EnrolmentResponse>()
                    {
                        Status = Status.BusinessException,
                        Value = null,
                        Errors = ErrorMapperHelper.MapBusinessExceptionToEAPIException(new Exception(Constants.ErrorCodes.InvalidPreferredPartnerFFP), Guid.NewGuid().ToString())
                    };
                }
            }
            

            return response;
        }
        private async Task<Result<Response<EnrolmentResponse>>> EnrolCustomer(EnrolmentRequestCommand enrolmentRequest, string actualTitle, string abbreviatedTitle)
        {
            List<CountryDetails> countryDetails;
            List<PartnerStatusCodeDetails> partnerstatuscodeDetails;
            List<PartnerProfileDetails> partnercodeDetails;
            List<MetroAreaDetails> metroAreaDetails = new();

            Result<Response<EnrolmentResponse>> enrolmentResponse = new Result<Response<EnrolmentResponse>>();

            foreach (var code in enrolmentRequest.Data.CustomerDetails.AddressDetails ?? Enumerable.Empty<Address>())
            {
                metroAreaDetails.Add(await _commonRefDBRepository.GetMetroArea(code.Country));
            }

            string channelName = enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel;

            var validationResponse = await ValidateError(enrolmentRequest);
            if (validationResponse.Value.Status == Status.BusinessException)
                return validationResponse;

            if (enrolmentRequest.Data.CustomerDetails.Preferences == null || !enrolmentRequest.Data.CustomerDetails.Preferences.Any())
            {
                enrolmentRequest.Data.CustomerDetails.Preferences = new List<Preferences>()
                {
                    new Preferences()
                    {
                        PreferenceType = Preferences.PreferenceTypes.VAAMarketingPermissions,
                        PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAAMarketingPreferenceValue)
                    },
                    new Preferences()
                    {
                        PreferenceType = Preferences.PreferenceTypes.VAPartnersMarketingPermissions,
                        PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAPartnersMarketingPreferenceValue)
                    }
                };
            }

            if (enrolmentRequest.Data.CustomerDetails.Preferences.Any(x => x.PreferenceType == Preferences.PreferenceTypes.VAAMarketingPermissions && string.IsNullOrEmpty(x.PreferenceValue)))
            {
                enrolmentRequest.Data.CustomerDetails.Preferences.First(x => x.PreferenceType == Preferences.PreferenceTypes.VAAMarketingPermissions).PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAAMarketingPreferenceValue);
            }

            if (enrolmentRequest.Data.CustomerDetails.Preferences.Any(x => x.PreferenceType == Preferences.PreferenceTypes.VAPartnersMarketingPermissions && string.IsNullOrEmpty(x.PreferenceValue)))
            {
                enrolmentRequest.Data.CustomerDetails.Preferences.FirstOrDefault(x => x.PreferenceType == Preferences.PreferenceTypes.VAPartnersMarketingPermissions).PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAPartnersMarketingPreferenceValue);
            }
            if (!enrolmentRequest.Data.CustomerDetails.Preferences.Any(x => x.PreferenceType == Preferences.PreferenceTypes.VAAMarketingPermissions))
            {
                enrolmentRequest.Data.CustomerDetails.Preferences.ToList().Add(new Preferences() { PreferenceType = Preferences.PreferenceTypes.VAAMarketingPermissions, PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAAMarketingPreferenceValue) });
            }
            if (!enrolmentRequest.Data.CustomerDetails.Preferences.Any(x => x.PreferenceType == Preferences.PreferenceTypes.VAPartnersMarketingPermissions))
            {
                enrolmentRequest.Data.CustomerDetails.Preferences.ToList().Add(new Preferences() { PreferenceType = Preferences.PreferenceTypes.VAPartnersMarketingPermissions, PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAPartnersMarketingPreferenceValue) });
            }


            if (ConfigReader.Get(Constants.Environment.PowerAppChannelID) != channelName)
            {
                enrolmentRequest.Data.ChannelUserId = channelName;
            }

            _logger.LogInformation($"Reference DB Check-Start");
            countryDetails = await _commonRefDBRepository.GetCountryCodeDetails();

            partnerstatuscodeDetails = await _commonRefDBRepository.GetPartnerStatusCodeDetails();

            partnercodeDetails = await _commonRefDBRepository.GetPartnerCodeDetails();

            List<MetroAreaDetails> metroArea = new();
            foreach (var countryCode in enrolmentRequest.Data.CustomerDetails.AddressDetails.Select(code => code.Country))
            {
                metroArea.Add(await _commonRefDBRepository.GetMetroArea(countryCode));
            }


            var referenceDBValidationResponse = await ValidationErrorsFromRefDB(enrolmentRequest);
            if (referenceDBValidationResponse.Value.Status == Status.BusinessException)
                return referenceDBValidationResponse;
            _logger.LogInformation($"Reference DB Check-End");


            CreateCustomerEntities requestEvent = new();

            if (await _mediator.Send(new CheckEmailAndTierQuery(enrolmentRequest, requestEvent)))
            {
                Response<EnrolmentResponse> enrolRes = new();
                enrolRes.Value = new EnrolmentResponse
                {
                    CustomerId = requestEvent.CustomerId,
                    Memberdetails = new MemberStatus()
                    {
                        Status = (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), GetStatus.Status(enrolmentRequest).Replace(" ", "")),
                        FlyingClubCardNumber = requestEvent.FcNumber,
                        MemberNumber = requestEvent.FcNumber,
                    }
                };
                enrolmentResponse = enrolRes;
                enrolmentResponse.Value.Status = Status.OK;
            }
            else
            {
                _logger.LogInformation($"DeDup Check-Start");
                var validateDeDupCheckResponse = await ValidateDeDupCheck(enrolmentRequest);
                if (validateDeDupCheckResponse.Value.Status == Status.BusinessException)
                    return validateDeDupCheckResponse;
                _logger.LogInformation($"DeDup Check-End");
                enrolmentResponse = await CreateFcAndCustomerId(enrolmentRequest, enrolmentResponse);
            }

            //invoke custoemr command for handling doc creation
            var enrolCustomerCommandRequest = CreateEnrolCustomerCommandMapper.CreateEnrolCustomerCommand(enrolmentRequest, enrolmentResponse, countryDetails, partnerstatuscodeDetails, partnercodeDetails, metroAreaDetails, actualTitle, abbreviatedTitle);
            await _mediator.Send(enrolCustomerCommandRequest);

            return enrolmentResponse;

        }
        private async Task<Result<Response<EnrolmentResponse>>> EnrolBasicCustomer(EnrolmentRequestCommand enrolmentRequest, string actualTitle, string abbreviatedTitle)
        {
            Response<EnrolmentResponse> enrolmentResponse = new();
            string channelName = enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel;
            List<CountryDetails> countryDetails = await _commonRefDBRepository.GetCountryCodeDetails();
            List<PartnerStatusCodeDetails> partnerstatuscodeDetails = await _commonRefDBRepository.GetPartnerStatusCodeDetails();
            List<PartnerProfileDetails> partnercodeDetails = await _commonRefDBRepository.GetPartnerCodeDetails();
            List<MetroAreaDetails> metroAreaDetails = new();
            CreateCustomerEntities requestEvent = new CreateCustomerEntities();

            foreach (var code in enrolmentRequest.Data.CustomerDetails.AddressDetails ?? Enumerable.Empty<Address>())
            {
                metroAreaDetails.Add(await _commonRefDBRepository.GetMetroArea(code.Country));
            }

            if (enrolmentRequest.Data.CustomerDetails.Preferences?.Any(x => x.PreferenceType == Preferences.PreferenceTypes.VAAMarketingPermissions) == true)
            {
                var marketingPreference = enrolmentRequest.Data.CustomerDetails.Preferences.First(a => a.PreferenceType == Preferences.PreferenceTypes.VAAMarketingPermissions);

                if (marketingPreference.PreferenceValue.IsNullOrWhiteSpace())
                    marketingPreference.PreferenceValue = "N";
            }

            _logger.LogInformation($"EnrolBasicCustomer method started");


            var validationResponse = await ValidateError(enrolmentRequest);
            if (validationResponse.Value.Status == Status.BusinessException)
                return validationResponse;

            if (ConfigReader.Get(Constants.Environment.PowerAppChannelID) != channelName)
            {
                enrolmentRequest.Data.ChannelUserId = channelName;
            }

            _logger.LogInformation($"Checking Email and Tire For WEB Channel");
            if (await _mediator.Send(new CheckEmailAndTierQuery(enrolmentRequest, requestEvent)) && channelName == ConfigReader.Get(Constants.Environment.WebSiteChannelId))
            {
                enrolmentResponse.Value = new EnrolmentResponse
                {
                    CustomerId = requestEvent.CustomerId,
                    Memberdetails = new MemberStatus()
                    {
                        FlyingClubCardNumber = requestEvent.FcNumber,
                        MemberNumber = requestEvent.FcNumber,
                        Status = (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), GetStatus.Status(enrolmentRequest).Replace(" ", "")),
                    }
                };
                enrolmentResponse.Status = Status.OK;
                //invoke custoemr command for handling doc creation
                var enrolCustomerCommandRequest = CreateEnrolCustomerCommandMapper.CreateEnrolCustomerCommand(enrolmentRequest, enrolmentResponse, countryDetails, partnerstatuscodeDetails, partnercodeDetails, metroAreaDetails, actualTitle, abbreviatedTitle);
                await _mediator.Send(enrolCustomerCommandRequest);
                return enrolmentResponse;
            }
            else
            {
                var dedupCheckResponse = await ValidateDeDupCheck(enrolmentRequest);
                if (dedupCheckResponse.Value.Status == Status.BusinessException)
                    return dedupCheckResponse;

                if (enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.ToUpper() == ConfigReader.Get(Constants.Environment.WebSiteChannelId))
                {
                    var basicEnrolmentResponse = await CreateFcAndCustomerId(enrolmentRequest, enrolmentResponse);
                    var enrolCustomerCommandRequest = CreateEnrolCustomerCommandMapper.CreateEnrolCustomerCommand(enrolmentRequest, basicEnrolmentResponse, countryDetails, partnerstatuscodeDetails, partnercodeDetails, metroAreaDetails, actualTitle, abbreviatedTitle);
                    await _mediator.Send(enrolCustomerCommandRequest);
                    return basicEnrolmentResponse;
                }
                else
                {
                    enrolmentResponse.Value = new EnrolmentResponse
                    {
                        CustomerId = Convert.ToString(Guid.NewGuid()),
                        Memberdetails = new MemberStatus()
                        {
                            Status = (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), MemberStatus.StatusEnum.Active.ToString()),
                        }
                    };
                    enrolmentResponse.Status = Status.OK;
                    //invoke custoemr command for handling doc creation
                    var enrolCustomerCommandRequest = CreateEnrolCustomerCommandMapper.CreateEnrolCustomerCommand(enrolmentRequest, enrolmentResponse, countryDetails, partnerstatuscodeDetails, partnercodeDetails, metroAreaDetails, actualTitle, abbreviatedTitle);
                    await _mediator.Send(enrolCustomerCommandRequest);
                    return enrolmentResponse;
                }
            }
        }
        private async Task<Result<Response<EnrolmentResponse>>> ValidateError(EnrolmentRequestCommand enrolmentRequest)
        {
            IEnumerable<ErrorDetails> validationErrors;
            var blackListedEmails = await _iBlobStorageServices.Deserialize<List<string>>();
            Result<Response<EnrolmentResponse>> enrolmentResponse = new();

            if (enrolmentRequest?.Data?.CustomerDetails.CustomerType == CustomerTypeEnum.Basic
                && enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel?.ToUpper() == ConfigReader.Get(Constants.Environment.WebSiteChannelId))
            {
                validationErrors = ValidatePayload.ValidateBasicEnrolment(enrolmentRequest);
                if (validationErrors.Any())
                    _logger.LogInformation(Json.Serialize(validationErrors));
            }
            else
                validationErrors = ValidatePayload.Validate(enrolmentRequest, enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel, blackListedEmails);

            if (validationErrors?.Any() ?? false)
            {
                enrolmentResponse = ErrorMapperHelper.ErrorResponseMapToEnrolmentResponse(validationErrors);
                enrolmentResponse.Value.Status = Status.BusinessException;

            }
            else
            {
                enrolmentResponse = new Response<EnrolmentResponse>()
                {
                    Status = Status.OK,

                    Errors = null
                };

            }
            return enrolmentResponse;
        }
        private async Task<Result<Response<EnrolmentResponse>>> ValidationErrorsFromRefDB(EnrolmentRequestCommand enrolmentRequest)
        {

            Result<Response<EnrolmentResponse>> enrolmentResponse = new Result<Response<EnrolmentResponse>>();

            List<ErrorDetails> validationErrorsFromRefDB = await _validateReferenceDB.ValidateFromReferenceDB(enrolmentRequest);

            if (validationErrorsFromRefDB?.Any() ?? false)
            {
                enrolmentResponse = ErrorMapperHelper.ErrorResponseMapToEnrolmentResponse(validationErrorsFromRefDB);
                enrolmentResponse.Value.Status = Status.BusinessException;
            }
            else
            {
                enrolmentResponse = new Response<EnrolmentResponse>()
                {
                    Status = Status.OK,
                    Value = null,
                    Errors = null
                };


            }
            return enrolmentResponse;
        }
        private async Task<Result<Response<EnrolmentResponse>>> ValidateDeDupCheck(EnrolmentRequestCommand enrolmentRequest)
        {
            Result<Response<EnrolmentResponse>> enrolmentResponse = new Result<Response<EnrolmentResponse>>();

            if (enrolmentRequest?.Data?.CustomerDetails?.CustomerType == CustomerTypeEnum.Loyalty
             && (enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel == Constants.Application.DeltaChannelID
             || enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel == Constants.Application.PowerAppChannelId))
            {
                var checkEmailResponse = await _mediator.Send(new CheckEmailQuery() { Email = enrolmentRequest.Data.CustomerDetails.ContactDetails.EmailDetails.First().EmailId });
                if (checkEmailResponse.Tier == Constants.Application.VacCode)
                {
                    enrolmentResponse = ErrorMapperHelper.ErrorResponseMapToEnrolmentResponse(ErrorMapperHelper.MapBusinessExceptionToEAPIExceptionwithFCNumber(new Exception(Constants.ErrorCodes.VACRestrictionError), Guid.NewGuid().ToString(), checkEmailResponse.CustomerId));
                    enrolmentResponse.Value.Status = Status.BusinessException;
                    return enrolmentResponse;
                }
            }

            List<ErrorDetails> deDupValidationError = await _deDupValidation.DeDupValidate(enrolmentRequest);

            if (deDupValidationError.Any())
            {
                enrolmentResponse = ErrorMapperHelper.ErrorResponseMapToEnrolmentResponse(deDupValidationError);
                enrolmentResponse.Value.Status = Status.BusinessException;
                if (enrolmentRequest?.Data?.CustomerDetails?.CustomerType == CustomerTypeEnum.Basic
                    && enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel.ToUpper() == ConfigReader.Get(Constants.Environment.WebSiteChannelId)
                    && deDupValidationError.Any(q => q.Code == Constants.ErrorCodes.EmailidDuplicateError))
                {
                    var error = new Response<string>()
                    {
                        Errors = EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.EmailCheckForBasicAccount, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    };
                    _logger.LogInformation($"{Json.Serialize(error.Errors)}");
                }
            }
            else
            {
                enrolmentResponse = new Response<EnrolmentResponse>()
                {
                    Status = Status.OK,
                    Value = null,
                    Errors = null
                };
            }
            return enrolmentResponse;
        }
        private async Task<Result<Response<EnrolmentResponse>>> CreateFcAndCustomerId(EnrolmentRequestCommand enrolmentRequest, Result<Response<EnrolmentResponse>> enrolmentResponse)
        {
            #region Loyalty Number Generation
            _logger.LogInformation($"SCVPlus - Loyalty Number Generation Start");
            enrolmentResponse = await _fCNumberGenerationRepository?.GenerateFlyingClubNumber();
            string FCNumber = enrolmentResponse.Value?.Value?.Memberdetails?.FlyingClubCardNumber;
            _logger.LogInformation($"SCVPlus - Loyalty Number Generation End");

            #endregion

            #region Mapping customerId and FCNumber to enrolmentresponse
            enrolmentResponse.Value.Value = new EnrolmentResponse
            {
                CustomerId = Convert.ToString(Guid.NewGuid()),
                Memberdetails = new MemberStatus()
                {
                    Status = IsWebBasic(enrolmentRequest) ? (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), MemberStatus.StatusEnum.Active.ToString()) : (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), GetStatus.Status(enrolmentRequest).Replace(" ", "")),
                    FlyingClubCardNumber = FCNumber,
                    MemberNumber = FCNumber,
                }
            };
            enrolmentResponse.Value.Status = Status.OK;
            #endregion

            return enrolmentResponse;
        }
        private static bool IsWebBasic(EnrolmentRequestCommand enrolmentRequest)
        {
            return enrolmentRequest?.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebSiteChannelId)
                   && enrolmentRequest?.Data?.CustomerDetails?.CustomerType == CustomerTypeEnum.Basic;
        }
    }
}