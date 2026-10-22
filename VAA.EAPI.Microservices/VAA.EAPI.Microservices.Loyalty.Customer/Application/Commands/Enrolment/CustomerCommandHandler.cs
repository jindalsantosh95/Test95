using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class CustomerCommandHandler : IRequestHandler<CustomerCommand, bool>
    {
        private readonly ILogger<CustomerCommandHandler> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSearchRepository _customerSearchRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IMemberProfileRepository _memberProfileRepository;
        private readonly IPartnerDocumentRepository _partnerDocumentRepository;
        private readonly IMembershipChangeRepository _membershipChangeRepository;
        private readonly IMediator _mediator;
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public CustomerCommandHandler(ILogger<CustomerCommandHandler> logger, ICustomerRepository customerRepository, IMemberProfileRepository memberProfileRepository, ICustomerSearchRepository customerSearchRepository, ICustomerProfileRepository customerProfileRepository
            , IPartnerDocumentRepository partnerDocumentRepository, IMembershipChangeRepository membershipChangeRepository, IMediator mediator, ICommonRefDBRepository commonRefDBRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _memberProfileRepository = memberProfileRepository ?? throw new ArgumentNullException(nameof(memberProfileRepository));
            _customerProfileRepository = customerProfileRepository ?? throw new ArgumentNullException(nameof(customerProfileRepository));
            _partnerDocumentRepository = partnerDocumentRepository ?? throw new ArgumentNullException(nameof(partnerDocumentRepository));
            _customerSearchRepository = customerSearchRepository ?? throw new ArgumentNullException(nameof(customerSearchRepository));
            _membershipChangeRepository = membershipChangeRepository ?? throw new ArgumentNullException(nameof(membershipChangeRepository));
            _mediator = mediator;
            _commonRefDBRepository = commonRefDBRepository;
        }

        public async Task<bool> Handle(CustomerCommand request, CancellationToken cancellationToken)
        {
            bool microserviceEvent = false;

            var req = Json.Deserialize<CreateCustomerEntities>(Convert.ToString(request.Data));
            _logger.LogInformation($"CorrelationId: {request.Id} & CustomerId: {req.CustomerId}");
            try
            {
                var countryCode = req.AddressDetails?.FirstOrDefault(x => x.PreferredFlag.Is("Y"))?.CountryCode;
                var validFlag = req.AddressDetails.FirstOrDefault(x => x.PreferredFlag.Is("Y")).Valid;
                bool addressVerified = req.AddressDetails.FirstOrDefault(x => x.PreferredFlag.Is("Y")).AddressVerified;
                if ((req.EnrollmentChannel == Constants.Application.PowerAppChannelId || req.EnrollmentChannel == Constants.Application.DeltaChannelID) && (countryCode == Convert.ToString(Country.GB) || countryCode == Convert.ToString(Country.US)))
                {
                    if (AddressCheck(req, countryCode, validFlag, addressVerified))
                    {
                        req.TermsAndConditionsAccepted = true;
                        validFlag = "Y";
                        addressVerified = true;
                    }
                    else
                    {
                        validFlag = "N";
                        addressVerified = false;
                    }
                }
                else
                {
                    validFlag = "Y";
                    addressVerified = req.AddressDetails.First(x => x.PreferredFlag.Is("Y")).AddressVerified;
                }

                req.AddressDetails.First(x => x.PreferredFlag.Is("Y")).Valid = validFlag;
                req.AddressDetails.First(x => x.PreferredFlag.Is("Y")).AddressVerified = addressVerified;

                if (req.EnrollmentChannel == Constants.Application.WebMyaChannelID || req.EnrollmentChannel == ConfigReader.Get(Constants.Environment.AppMyaChannelId))
                {
                    var countryDetails = await _commonRefDBRepository.GetCountryCodeDetails();
                    foreach (var address in req.AddressDetails)
                    {
                        if (!string.IsNullOrEmpty(address.CountryCode))
                        {
                            var metroArea = await _commonRefDBRepository.GetMetroArea(address.CountryCode);
                            address.MetroArea = metroArea?.AdditionalValue1;
                            address.CountryName = countryDetails.FirstOrDefault(x => x.Name == address.CountryCode)?.Value;
                        }
                    }                   
                }

                bool upgradeBasicToLoyalty = await _mediator.Send(new FetchDocumentDetailsQuery(req), cancellationToken);
                CreateCustomerEventResponse customerDetails = new();
                if (upgradeBasicToLoyalty)
                {
                    _logger.LogInformation($"Upgrade Existing Basic to Loyalty Customer with ID:  {req.CustomerId}");
                    customerDetails = await _mediator.Send(new BasicAccUpgradeCommand(req), cancellationToken); //Update existing Cosmos Docs
                }
                else
                {
                    _logger.LogInformation($"Enrol Loyalty Customer with ID: {req.CustomerId}");
                    customerDetails = await EnrolCustomer(req, request.Id); //Create new Cosmos Docs
                }

                #region Publish Identity, VRED, OCDB and PSDS event
                if (!(req.EnrollmentChannel == Constants.Application.WebMyaChannelID || req.EnrollmentChannel == ConfigReader.Get(Constants.Environment.AppMyaChannelId)))
                {
                    if (ConfigReader.Get<bool>(Constants.Environment.VredEnableSignDate) && req.EnrollmentChannel == Constants.Application.PowerAppChannelId)
                    {
                        customerDetails.MarketingPreferencesSignedAt = customerDetails.TermsAndConditionsSignedAt = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat);
                        var preference = req.Preferences?.FirstOrDefault(item => item.PreferenceType == Preferences.PreferenceTypes.VAPartnersMarketingPermissions);
                        customerDetails.MarketingAccepted = !string.IsNullOrEmpty(preference?.PreferenceValue) && string.Equals(Constants.Application.TrueFlag, preference?.PreferenceValue, StringComparison.OrdinalIgnoreCase);
                    }
                    var eventData = CreateCustomerEventDataMapper.CustomerEventDataMapper(customerDetails, req, request.Id);

                    try
                    {
                        _logger.LogDebug($"Store event in event store - Start");
                        await _mediator.Send(new EventRequestInsertCommand { Id = eventData.Id, Type = eventData.Type, Data = eventData.Data, Subject = eventData.Subject });
                        microserviceEvent = true;
                        _logger.LogDebug($"Store event in event store - End");

                        _logger.LogInformation($"Publishing Event Start");
                        await _mediator.Publish(eventData, CancellationToken.None);
                        _logger.LogInformation($"Publishing Event End");
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Rollback from Event Store due to error: {ex.Message}");
                        if (microserviceEvent)
                            await _mediator.Send(new EventStoreRequestDeleteCommand { Id = request.Id, Type = ConfigReader.Get(Constants.Environment.EventTypeCustomerSync), Data = customerDetails, Subject = Constants.Application.CreateCustomerEventSubject }, cancellationToken);
                        throw;
                    }

                    #endregion

                    #region Publish Transactional Event to Siebel Exit
                    await SendTransactionalEvent(customerDetails);
                    #endregion
                }
                return true;
            }

            catch (Exception ex)
            {
                _logger.LogInformation($"Document Creation failed: {ex.Message}");
            }
            return false;
        }

        private async Task<CreateCustomerEventResponse> EnrolCustomer(CreateCustomerEntities createCustomerEventRequest, string Id)
        {
            var addCustomerDetatils = _customerRepository.AddCustomerDetails(createCustomerEventRequest);
            var addCustomerSearchDetails = _customerSearchRepository.AddCustomerSearchDetails(createCustomerEventRequest);
            var addCustomerProfileDetails = _customerProfileRepository.AddCustomerProfileDetails(createCustomerEventRequest);
            var addPartnerDetails = _partnerDocumentRepository.AddPartnerDetails(createCustomerEventRequest);
            var addMemberProfileDetails = _memberProfileRepository.AddMemberProfileDetails(createCustomerEventRequest);
            await Task.WhenAll(addCustomerDetatils, addCustomerSearchDetails, addCustomerProfileDetails, addPartnerDetails, addMemberProfileDetails);

            Result<Response<TierInfo>> tierDetails = await _membershipChangeRepository.AddMembershipChangeDetails(createCustomerEventRequest);

            var request = CreateCustomerEventEntitiesMapper.CustomerEventResponseMapper(createCustomerEventRequest, tierDetails);
            return request;
        }

        public static bool AddressCheck(CreateCustomerEntities request, string countryCode, string validFlag, bool addressVerified)
        {
            if ((bool)(request.AddressDetails?.Any(x => x.PreferredFlag == "Y")))
            {
                if (countryCode == Convert.ToString(Country.GB) || countryCode == Convert.ToString(Country.US))
                {
                    if (request.AddressDetails.FirstOrDefault(x => x.PreferredFlag.Is("Y"))?.AddressVerified == true)
                    {
                        validFlag = "Y";
                        addressVerified = true;
                    }
                    else
                    {
                        validFlag = "N";
                        addressVerified = false;
                    }
                }
                else
                {
                    validFlag = "Y";
                    addressVerified = false;
                }
            }
            else
            {
                validFlag = "Y";
                addressVerified = false;
            }

            request.AddressDetails.FirstOrDefault(x => x.PreferredFlag.Is("Y")).Valid = validFlag;
            return addressVerified;
        }

        public async Task SendTransactionalEvent(CreateCustomerEventResponse customerDetails)
        {
            bool transactionalEvent = false;
            string id = Guid.NewGuid().ToString();
            _logger.LogInformation("Publishing Transactional Event-Start");
            TransactionalEvent eventRequest = TransactionalEventRequestMapper.CreateEventGridSchemaData(customerDetails, id);
            EventGridTopicEvent eventGridEvent = EventGridEventMapper.EventDataMapper(eventRequest);

            try
            {
                _logger.LogDebug($"Store event in event store - Start");
                await _mediator.Send(new EventRequestInsertCommand { Id = eventGridEvent.Id, Type = eventGridEvent.Type, Data = eventGridEvent.Data, Subject = eventGridEvent.Subject });
                transactionalEvent = true;
                _logger.LogDebug($"Store event in event store - End");

                _logger.LogInformation("Publishing Transactional Event-End");
                await _mediator.Publish(eventGridEvent);
                _logger.LogInformation("Publishing Transactional Event-End");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Rollback from Event Store due to error: {ex.Message}");
                if (transactionalEvent)
                    await _mediator.Send(new EventStoreRequestDeleteCommand { Id = id, Type = Constants.Application.EventType, Data = customerDetails, Subject = Constants.Application.EventSubject });
                throw;
            }
        }
    }
}