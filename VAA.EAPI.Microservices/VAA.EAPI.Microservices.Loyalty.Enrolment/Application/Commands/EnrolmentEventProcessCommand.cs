using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CommonReferenceDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IValidation;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Helper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Infrastructure.Repositories.CheckEmailAndTier;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Mapper;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Validations;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface;
using static VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.CustomerDetails.Customer;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Enrolment.Domain;

namespace VAA.EAPI.Microservices.Loyalty.Enrolment.Application.Commands
{
    public class EnrolmentEventSubscribeCommandHandler : IRequestHandler<EnrolmentEventProcessCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnrolmentEventCommandHandler> _logger;
        private readonly IValidateReferenceDB _validateReferenceDB;
        private readonly IDeDupValidation _deDupValidation;
        private readonly IFCNumberGenerationRepository _fCNumberGenerationRepository;
        private readonly IErrorMapper _errorMapper; private readonly ICommonRefDBRepository _commonRefDBRepository;

        private readonly IBlobStorageServices _iBlobStorageServices;

        public EnrolmentEventSubscribeCommandHandler(ILogger<EnrolmentEventCommandHandler> logger, IValidateReferenceDB validateReferenceDB,
            IDeDupValidation deDupValidation, IErrorMapper errorMapper, IFCNumberGenerationRepository fCNumberGenerationRepository,
            IMediator mediator, ICommonRefDBRepository commonRefDBRepository, IBlobStorageServices iBlobStorageServices)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _validateReferenceDB = validateReferenceDB ?? throw new ArgumentNullException(nameof(validateReferenceDB));
            _deDupValidation = deDupValidation ?? throw new ArgumentNullException(nameof(deDupValidation));
            _errorMapper = errorMapper ?? throw new ArgumentNullException(nameof(errorMapper));
            _fCNumberGenerationRepository = fCNumberGenerationRepository ?? throw new ArgumentNullException(nameof(fCNumberGenerationRepository));
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentNullException(nameof(commonRefDBRepository));
            _iBlobStorageServices = iBlobStorageServices ?? throw new ArgumentNullException(nameof(iBlobStorageServices));
        }

        public async Task<Result<Unit>> Handle(EnrolmentEventProcessCommand request, CancellationToken cancellationToken)
        {
            EnrolmentEventRequest enrolmentRequest = null;

            try
            {
                _logger.LogInformation($"Enrolment Event Process Command Handler. CorrelationId: {request.Id}");
                enrolmentRequest = new()
                {
                    Data = Json.Deserialize<EnrolmentRequest>(Convert.ToString(request.Data)),
                    Id = request.Id,
                    Source = request.Source,
                    Subject = request.Subject,
                    Time = request.Time,
                    Type = request.Type
                };

                string actualTitle = enrolmentRequest.Data.CustomerDetails.Title;
                string abbreviatedTitle = enrolmentRequest.Data.CustomerDetails.Title;

                if (enrolmentRequest.Data.CustomerDetails.FirstName != null)
                    enrolmentRequest.Data.CustomerDetails.FirstName = RegExHelper.ConvertToTitle(enrolmentRequest.Data.CustomerDetails.FirstName);

                if (enrolmentRequest.Data.CustomerDetails.LastName != null)
                    enrolmentRequest.Data.CustomerDetails.LastName = RegExHelper.ConvertToTitle(enrolmentRequest.Data.CustomerDetails.LastName);

                if (enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.DeltaChannelID) || enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebMyaChannelId))
                {
                    actualTitle = await _commonRefDBRepository.GetTitleValue(enrolmentRequest?.Data?.CustomerDetails?.Title);
                    if (actualTitle == null)
                    {
                        _logger.LogWarning("Title in the request is not available in reference data");
                        actualTitle = enrolmentRequest.Data.CustomerDetails.Title;
                    }

                }

                if (enrolmentRequest.Data.CustomerDetails.CustomerType == CustomerTypeEnum.Basic)
                    await EnrolBasicCustomer(enrolmentRequest, actualTitle, abbreviatedTitle);
                else
                    await EnrolCustomer(enrolmentRequest, actualTitle, abbreviatedTitle);

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, ex.Message);
                await _mediator.Send(new ServiceResponseInsertCommand { Id = enrolmentRequest?.Id, JsonResponseData = ex.Message.ToString(), Status = ServiceResponseProcessStatus.Error, Type = Constants.Application.CreateCustomerEventSubject });
            }

            return Unit.Value;
        }

        private async Task<Unit> EnrolCustomer(EnrolmentEventRequest enrolmentRequest, string actualTitle, string abbreviatedTitle)
        {
            List<CountryDetails> countryDetails;
            List<PartnerStatusCodeDetails> partnerstatuscodeDetails;
            List<PartnerProfileDetails> partnercodeDetails;

            Result<Response<EnrolmentResponse>> enrolmentResponse = new Result<Response<EnrolmentResponse>>();

            string channelName = enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel;


            _logger.LogInformation($"Payload Validation-Start");
            if (!await ValidateError(enrolmentRequest))
                return Unit.Value;
            _logger.LogInformation($"Payload Validation-End");

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
                enrolmentRequest.Data.CustomerDetails.Preferences.First(x => x.PreferenceType == Preferences.PreferenceTypes.VAPartnersMarketingPermissions).PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAPartnersMarketingPreferenceValue);
            }
            if (!enrolmentRequest.Data.CustomerDetails.Preferences.Any(x => x.PreferenceType == Preferences.PreferenceTypes.VAAMarketingPermissions))
            {
                enrolmentRequest.Data.CustomerDetails.Preferences.Add(new Preferences() { PreferenceType = Preferences.PreferenceTypes.VAAMarketingPermissions, PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAAMarketingPreferenceValue) });
            }
            if (!enrolmentRequest.Data.CustomerDetails.Preferences.Any(x => x.PreferenceType == Preferences.PreferenceTypes.VAPartnersMarketingPermissions))
            {
                enrolmentRequest.Data.CustomerDetails.Preferences.Add(new Preferences() { PreferenceType = Preferences.PreferenceTypes.VAPartnersMarketingPermissions, PreferenceValue = ConfigReader.Get(Constants.Environment.DefaultVAPartnersMarketingPreferenceValue) });
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
            foreach (var code in enrolmentRequest.Data.CustomerDetails.AddressDetails)
            {
                metroArea.Add(await _commonRefDBRepository.GetMetroArea(code.Country));
            }


            if (!await ValidationErrorsFromRefDB(enrolmentRequest))
                return Unit.Value;
            _logger.LogInformation($"Reference DB Check-End");

            Result<Response<EnrolmentResponse>> response = new();
            CreateCustomerEventEntities requestEvent = new();

            if (await _mediator.Send(new CheckEmailAndTierQuery(enrolmentRequest.Data, requestEvent)))
            {
                Response<EnrolmentResponse> enrolRes = new();
                enrolRes.Value = new EnrolmentResponse
                {
                    CustomerId = requestEvent.CustomerId,
                    Memberdetails = new MemberStatus()
                    {
                        Status = (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), GetStatus.Status(enrolmentRequest.Data).Replace(" ", "")),
                        FlyingClubCardNumber = requestEvent.FcNumber,
                        MemberNumber = requestEvent.FcNumber,
                    }
                };
                response = enrolRes;
                response.Value.Status = Status.OK;
            }
            else
            {
                _logger.LogInformation($"DeDup Check-Start");
                if (!await ValidateDeDupCheck(enrolmentRequest))
                    return Unit.Value;
                _logger.LogInformation($"DeDup Check-End");


                response = await CreateFcAndCustomerId(enrolmentRequest, enrolmentResponse);
            }

            await SaveResponse(enrolmentRequest, response);

            requestEvent = CreateCustomerEventEntitiesMapper.CustomerEventEntitiesMapper(enrolmentRequest.Data, response, countryDetails, partnerstatuscodeDetails, partnercodeDetails, metroArea, actualTitle, abbreviatedTitle);

            _logger.LogInformation($"Publish Customer Event for customerId: {response.Value.Value.CustomerId} and member number: {response.Value.Value.Memberdetails.FlyingClubCardNumber}");
            await PublishEvent(CreateCustomerEventDataMapper.CustomerDataMapper(enrolmentRequest.Id, ConfigReader.Get(Constants.Environment.EventType), Json.Serialize(requestEvent)));

            return Unit.Value;
        }

        private async Task<Unit> EnrolBasicCustomer(EnrolmentEventRequest enrolmentRequest, string actualTitle, string abbreviatedTitle)
        {
            Response<EnrolmentResponse> enrolmentResponse = new();
            CreateCustomerEventEntities requestEvent = new();
            string channelName = enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel;
            List<CountryDetails> countryDetails = await _commonRefDBRepository.GetCountryCodeDetails();
            List<PartnerStatusCodeDetails> partnerstatuscodeDetails = await _commonRefDBRepository.GetPartnerStatusCodeDetails();
            List<PartnerProfileDetails> partnercodeDetails = await _commonRefDBRepository.GetPartnerCodeDetails();
            List<MetroAreaDetails> metroAreaDetails = new();


            foreach (var code in enrolmentRequest.Data.CustomerDetails.AddressDetails ?? Enumerable.Empty<Address>())
            {
                metroAreaDetails.Add(await _commonRefDBRepository.GetMetroArea(code.Country));
            }

            if (enrolmentRequest.Data.CustomerDetails.Preferences != null && enrolmentRequest.Data.CustomerDetails.Preferences.Find(x => x.PreferenceType == Preferences.PreferenceTypes.VAAMarketingPermissions) != null)
            {
                if (string.IsNullOrWhiteSpace(enrolmentRequest.Data.CustomerDetails.Preferences.Find(a => Convert.ToString(a.PreferenceType).Equals(Convert.ToString(Preferences.PreferenceTypes.VAAMarketingPermissions))).PreferenceValue))
                {
                    enrolmentRequest.Data.CustomerDetails.Preferences.Find(a => Convert.ToString(a.PreferenceType).Equals(Convert.ToString(Preferences.PreferenceTypes.VAAMarketingPermissions))).PreferenceValue = "N";
                }

            }

            _logger.LogInformation($"EnrolBasicCustomer method started");


            if (!await ValidateError(enrolmentRequest))
                return Unit.Value;

            if (ConfigReader.Get(Constants.Environment.PowerAppChannelID) != channelName)
            {
                enrolmentRequest.Data.ChannelUserId = channelName;
            }

            _logger.LogInformation($"Checking Email and Tire For WEB Channel");
            if (await _mediator.Send(new CheckEmailAndTierQuery(enrolmentRequest.Data, requestEvent)) && channelName == ConfigReader.Get(Constants.Environment.WebSiteChannelId))
            {
                enrolmentResponse.Value = new EnrolmentResponse
                {
                    CustomerId = requestEvent.CustomerId,
                    Memberdetails = new MemberStatus()
                    {
                        FlyingClubCardNumber = requestEvent.FcNumber,
                        MemberNumber = requestEvent.FcNumber,
                    }
                };
                requestEvent = CreateCustomerEventEntitiesMapper.CustomerEventEntitiesMapper(enrolmentRequest.Data, enrolmentResponse, countryDetails, partnerstatuscodeDetails, partnercodeDetails, metroAreaDetails, actualTitle, abbreviatedTitle);
                await SaveResponse(enrolmentRequest, enrolmentResponse);
                _logger.LogInformation($"Publishing Event To Customer MS for Update Scenario");
                await PublishEvent(CreateCustomerEventDataMapper.CustomerDataMapper(enrolmentRequest.Id, ConfigReader.Get(Constants.Environment.EventType), Json.Serialize(requestEvent)));
                return Unit.Value;
            }
            else
            {
                if (!await ValidateDeDupCheck(enrolmentRequest))
                    return Unit.Value;

                if (enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.ToUpper() == ConfigReader.Get(Constants.Environment.WebSiteChannelId))
                {
                    var basicEnrolmentResponse = CreateFcAndCustomerId(enrolmentRequest, enrolmentResponse);
                    await SaveResponse(enrolmentRequest, basicEnrolmentResponse.Result);
                    requestEvent = CreateCustomerEventEntitiesMapper.CustomerEventEntitiesMapper(enrolmentRequest.Data, basicEnrolmentResponse.Result, countryDetails, partnerstatuscodeDetails, partnercodeDetails, metroAreaDetails, actualTitle, abbreviatedTitle);
                    _logger.LogInformation($"Publish Web Basic Enrolment Event for customerId: {basicEnrolmentResponse.Result.Value.Value.CustomerId} and member number: {basicEnrolmentResponse.Result.Value.Value.Memberdetails.FlyingClubCardNumber}");
                    await PublishEvent(CreateCustomerEventDataMapper.CustomerDataMapper(enrolmentRequest.Id, ConfigReader.Get(Constants.Environment.EventType), Json.Serialize(requestEvent)));
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
                    await SaveResponse(enrolmentRequest, enrolmentResponse);
                    requestEvent = CreateCustomerEventEntitiesMapper.CustomerEventEntitiesMapper(enrolmentRequest.Data, enrolmentResponse, countryDetails, partnerstatuscodeDetails, partnercodeDetails, metroAreaDetails, actualTitle, abbreviatedTitle);
                    await PublishEvent(CreateCustomerEventDataMapper.CustomerDataMapper(enrolmentRequest.Id, ConfigReader.Get(Constants.Environment.EventType), Json.Serialize(requestEvent)));
                }
                return Unit.Value;
            }
        }

        private async Task<bool> ValidateError(EnrolmentEventRequest enrolmentRequest)
        {
            List<ErrorDetails> validationErrors;
            var blackListedEmails = await _iBlobStorageServices.Deserialize<List<string>>();
            Result<Response<EnrolmentResponse>> enrolmentResponse = new();

            if (enrolmentRequest.Data.CustomerDetails.CustomerType == CustomerTypeEnum.Basic && enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel.ToUpper() == ConfigReader.Get(Constants.Environment.WebSiteChannelId))
            {
                validationErrors = ValidatePayload.ValidateBasicEnrolment(enrolmentRequest.Data);
                if (validationErrors?.Count > 0)
                    _logger.LogInformation(Json.Serialize(validationErrors));
            }
            else
                validationErrors = ValidatePayload.Validate(enrolmentRequest.Data, enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel, blackListedEmails);

            if (validationErrors?.Count > 0)
            {
                enrolmentResponse = ErrorMapperHelper.ErrorResponseMap(validationErrors);
                enrolmentResponse.Value.Status = Status.BusinessException;
                await SaveResponse(enrolmentRequest, enrolmentResponse);
                return false;
            }
            return true;
        }

        private async Task<bool> ValidationErrorsFromRefDB(EnrolmentEventRequest enrolmentRequest)
        {

            Result<Response<EnrolmentResponse>> enrolmentResponse = new Result<Response<EnrolmentResponse>>();

            List<ErrorDetails> validationErrorsFromRefDB = await _validateReferenceDB.ValidateFromReferenceDB(enrolmentRequest.Data);

            if (validationErrorsFromRefDB?.Count > 0)
            {
                enrolmentResponse = ErrorMapperHelper.ErrorResponseMap(validationErrorsFromRefDB);
                enrolmentResponse.Value.Status = Status.BusinessException;

                await _mediator.Send(new ServiceResponseInsertCommand { Id = enrolmentRequest.Id, JsonResponseData = Json.Serialize(enrolmentResponse), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.CreateCustomerEventSubject });

                return false;
            }

            return true;
        }

        private async Task<bool> ValidateDeDupCheck(EnrolmentEventRequest enrolmentRequest)
        {
            Result<Response<EnrolmentResponse>> enrolmentResponse = new Result<Response<EnrolmentResponse>>();

            List<ErrorDetails> deDupValidationError = await _deDupValidation.DeDupValidate(enrolmentRequest.Data);

            if (deDupValidationError.Count > 0)
            {
                enrolmentResponse = ErrorMapperHelper.ErrorResponseMap(deDupValidationError);
                enrolmentResponse.Value.Status = Status.BusinessException;
                if (enrolmentRequest.Data.CustomerDetails?.CustomerType == CustomerTypeEnum.Basic && enrolmentRequest.Data?.CustomerDetails?.LoyaltyMember?.EnrollmentChannel.ToUpper() == ConfigReader.Get(Constants.Environment.WebSiteChannelId) && deDupValidationError.Any(q => q.Code == Constants.ErrorCodes.EmailidDuplicateError))
                {
                    var error = new Response<string>()
                    {
                        Errors = EAPI.Infrastructure.Helper.ErrorMapper.MapTechnicalExceptionToEAPIException(Constants.ErrorCodes.EmailCheckForBasicAccount, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    };
                    _logger.LogInformation($"{Json.Serialize(error.Errors)}");
                }
                await SaveResponse(enrolmentRequest, enrolmentResponse.Value);

                return false;
            }

            return true;
        }

        private async Task<Result<Response<EnrolmentResponse>>> CreateFcAndCustomerId(EnrolmentEventRequest enrolmentRequest, Result<Response<EnrolmentResponse>> enrolmentResponse)
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
                    Status = IsWebBasic(enrolmentRequest) ? (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), MemberStatus.StatusEnum.Active.ToString()) : (MemberStatus.StatusEnum?)Enum.Parse(typeof(MemberStatus.StatusEnum), GetStatus.Status(enrolmentRequest.Data).Replace(" ", "")),
                    FlyingClubCardNumber = FCNumber,
                    MemberNumber = FCNumber,
                }
            };
            enrolmentResponse.Value.Status = Status.OK;
            #endregion

            return enrolmentResponse;
        }

        private async Task PublishEvent(CloudEvent eventGridTopic)
        {
            await _mediator.Publish(eventGridTopic, CancellationToken.None);
        }

        private async Task SaveResponse(EnrolmentEventRequest enrolmentRequest, Result<Response<EnrolmentResponse>> enrolmentResponse)
        {
            await _mediator.Send(new ServiceResponseInsertCommand { Id = enrolmentRequest.Id, JsonResponseData = Json.Serialize(enrolmentResponse), Status = ServiceResponseProcessStatus.Completed, Type = Constants.Application.CreateCustomerEventSubject });

        }

        private static bool IsWebBasic(EnrolmentEventRequest enrolmentRequest)
        {
            if (enrolmentRequest.Data.CustomerDetails.LoyaltyMember.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebSiteChannelId) && enrolmentRequest.Data.CustomerDetails.CustomerType == CustomerTypeEnum.Basic)
                return true;
            else
                return false;
        }

    }

}
