using FluentValidation;
using MediatR;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Entities.Email;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;

//using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.CustomerDetails;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Validations.Enrolment;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class UpgradeToFlyingClubCommandHandler : IRequestHandler<UpgradeToFlyingClubCommand, Response<string>>
    {
        private readonly IMediator _mediator;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSearchRepository _customerSearchRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IMemberProfileRepository _memberProfileRepository;
        private readonly IPartnerDocumentRepository _partnerDocumentRepository;
        private readonly IMembershipChangeRepository _membershipChangeRepository;
        private readonly IGetCustTransDocumentService<MembershipChange> _custTransMbrChangeService;
        private readonly IGetCustTransDocumentService<MemberProfile> _custTransFcmService;
        private readonly IGetCustTransDocumentService<PartnerMembership> _custTransPartnerMbrShipService;
        private readonly IGetCustTransDocumentService<CustomerProfile> _custTransCustomerProfileService;

        public UpgradeToFlyingClubCommandHandler(IMediator mediator, ICustomerRepository customerRepository, IMemberProfileRepository memberProfileRepository, ICustomerSearchRepository customerSearchRepository, ICustomerProfileRepository customerProfileRepository, IPartnerDocumentRepository partnerDocumentRepository, IMembershipChangeRepository membershipChangeRepository, IGetCustTransDocumentService<MembershipChange> custTransMbrChangeService, IGetCustTransDocumentService<MemberProfile> custTransFcmService, IGetCustTransDocumentService<PartnerMembership> custTransPartnerMbrShipService, IGetCustTransDocumentService<CustomerProfile> custTransCustomerProfileService)
        {
            _mediator = mediator;
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _memberProfileRepository = memberProfileRepository ?? throw new ArgumentNullException(nameof(memberProfileRepository));
            _customerProfileRepository = customerProfileRepository ?? throw new ArgumentNullException(nameof(customerProfileRepository));
            _partnerDocumentRepository = partnerDocumentRepository ?? throw new ArgumentNullException(nameof(partnerDocumentRepository));
            _customerSearchRepository = customerSearchRepository ?? throw new ArgumentNullException(nameof(customerSearchRepository));
            _membershipChangeRepository = membershipChangeRepository ?? throw new ArgumentNullException(nameof(membershipChangeRepository));
            _custTransMbrChangeService = custTransMbrChangeService ?? throw new ArgumentNullException(nameof(custTransMbrChangeService));
            _custTransFcmService = custTransFcmService ?? throw new ArgumentNullException(nameof(custTransFcmService));
            _custTransPartnerMbrShipService = custTransPartnerMbrShipService ?? throw new ArgumentNullException(nameof(custTransPartnerMbrShipService));
            _custTransCustomerProfileService = custTransCustomerProfileService ?? throw new ArgumentNullException(nameof(custTransCustomerProfileService));

        }
        public async Task<Response<string>> Handle(UpgradeToFlyingClubCommand req, CancellationToken cancellationToken)
        {
            try
            {
                var result = ValidateRequest(req);

                if (result.Status == Status.OK)
                {
                    Customers customerDoc = (await _mediator.Send(new FetchCustomerDocumentQuery(req.CustomerDetails.CustomerId), cancellationToken)).Value.Value;

                    MembershipChange mbrChangeDoc = await _custTransMbrChangeService.GetCosmosDoc(DocumentType.MembershipChange.ToString(), req.CustomerDetails.CustomerId);

                    MemberProfile fcmDoc = await _custTransFcmService.GetCosmosDoc(DocumentType.FlyingClubMember.ToString(), req.CustomerDetails.CustomerId);

                    PartnerMembership partnerMbrShipDoc = await _custTransPartnerMbrShipService.GetCosmosDoc(DocumentType.PartnerMembership.ToString(), req.CustomerDetails.CustomerId);

                    CustomerProfile custProfileDoc = await _custTransCustomerProfileService.GetCosmosDoc(DocumentType.CustomerProfile.ToString(), req.CustomerDetails.CustomerId);

                    await UpgradeMember(req.CustomerDetails, customerDoc, mbrChangeDoc, fcmDoc, partnerMbrShipDoc, custProfileDoc);

                    CreateCustomerEventResponse customerDetails = CreateCustomerEventEntitiesMapper.CustomerEventResponseMapper(req.CustomerDetails, null);

                    #region Publish VRED, OCDB and PSDS event

                    if (ConfigReader.Get<bool>(Constants.Environment.VredEnableSignDate))
                    {
                        customerDetails.MarketingPreferencesSignedAt = customerDetails.TermsAndConditionsSignedAt = DateTime.UtcNow.ToString(Constants.Application.ISODateTimeFormat);
                        var preference = req.CustomerDetails.Preferences?.FirstOrDefault(item => item.PreferenceType == Domain.Entities.Preferences.PreferenceTypes.VAPartnersMarketingPermissions);
                        customerDetails.MarketingAccepted = !string.IsNullOrEmpty(preference?.PreferenceValue) && string.Equals(Constants.Application.TrueFlag, preference?.PreferenceValue, StringComparison.OrdinalIgnoreCase);
                    }

                    var correlationIdForEvents = Guid.NewGuid().ToString();

                    await PublishEvent(CreateCustomerEventDataMapper.VaCustomerEventDataMapper(customerDetails, correlationIdForEvents), correlationIdForEvents, cancellationToken);

                    var sendEmailReq = new SendEmailRequest
                    {
                        Template = Constants.Application.WelcomeEmailTemplate,
                        DataFields = new DataFields
                        {
                            Membership_Number = customerDoc.LoyaltyMemberNumber,
                            Customer_FirstName = customerDoc.FirstName,
                            Member_Name = customerDoc.FirstName,
                            Customer_LastName = customerDoc.LastName,
                            Territory = req.CustomerDetails.AddressDetails.First().CountryCode,
                            Tier = TierCode.Red.ToString(),
                            Virgin_Points = "0"
                        },
                        To = req.CustomerDetails.ContactDetails?.EmailDetails?.FirstOrDefault()?.EmailId
                    };

                    await PublishEvent(EmailTriggeringEventGridMapper.EmailEventDataMapper(sendEmailReq), correlationIdForEvents, cancellationToken);

                    #endregion

                    return new Response<string>()
                    {
                        Status = Status.OK,
                        Value = "Account Upgraded Successful",
                        Errors = null
                    };
                }
                return result;
            }

            catch (Exception)
            {
                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private static Response<string> ValidateRequest(UpgradeToFlyingClubCommand req)
        {
            var response = new Response<string>()
            {
                Status = Status.OK,
                Value = null,
                Errors = null
            };

            EnrolmentRequestCommand enrolmentRequest = new EnrolmentRequestCommand()
            {
                Data = new EnrolmentRequest()
                {
                    CustomerDetails = (EnrolCustomerDetails)req
                }
            };

            if (!new EnrolmentCommandValidator().Validate(enrolmentRequest).IsValid)
                response = GetErrorResponse(Constants.ErrorCodes.MandatoryInputFieldsMissing);

            if (string.IsNullOrEmpty(req.CustomerDetails.Gender))
                response = GetErrorResponse(Constants.ErrorCodes.MandatoryInputFieldsMissing);


            if (req.CustomerDetails.EnrollmentChannel == ConfigReader.Get(Constants.Environment.WebMyaChannelId) || req.CustomerDetails.EnrollmentChannel == ConfigReader.Get(Constants.Environment.AppMyaChannelId))
            {
                var countryList = ConfigReader.Get(Constants.Environment.PostCodeMandatoryCountryList).Split("|").ToList();

                if (countryList.Contains(req.CustomerDetails?.AddressDetails?.FirstOrDefault()?.CountryCode) && string.IsNullOrEmpty(req.CustomerDetails?.AddressDetails?.FirstOrDefault()?.PostalCode))
                    response = GetErrorResponse(Constants.ErrorCodes.MandatoryInputFieldsMissing);
            }

            return response;
        }

        private async Task UpgradeMember(CreateCustomerEntities enrolmentRequest, Customers customerDoc, MembershipChange mbrChangeDoc, MemberProfile fcmDoc, PartnerMembership partnerMbrShipDoc, CustomerProfile custProfileDoc)
        {
            var updateCustomerSearchDetails = _customerSearchRepository.UpdateCustomerSearchDetails(enrolmentRequest, customerDoc);
            var updateCustomerDetatils = _customerRepository.UpdateCustomerDetails(enrolmentRequest, customerDoc);
            var updateCustomerProfileDetails = _customerProfileRepository.UpdateCustomerProfileDetails(enrolmentRequest, custProfileDoc);
            var updateMemberProfileDetails = _memberProfileRepository.UpdateMemberProfileDetails(enrolmentRequest, fcmDoc);
            var updatePartnerDetails = _partnerDocumentRepository.UpdatePartnerDetails(enrolmentRequest, partnerMbrShipDoc);
            var updateMembershipChange = _membershipChangeRepository.UpdateMembershipChangeDetails(enrolmentRequest, mbrChangeDoc);

            await Task.WhenAll(updateCustomerSearchDetails, updateCustomerDetatils, updateCustomerProfileDetails, updateMemberProfileDetails, updatePartnerDetails, updateMembershipChange);
        }

        private async Task PublishEvent(CloudEvent enrolmentEvent, string correlationIdForEvents, CancellationToken cancellationToken)
        {

            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = Constants.Application.DeDupValidateEventType, Data = enrolmentEvent.Data, Subject = enrolmentEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(enrolmentEvent, cancellationToken);
            }

            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = Constants.Application.DeDupValidateEventType, Data = enrolmentEvent.Data, Subject = enrolmentEvent.Subject }, cancellationToken);
                throw;
            }
        }

        private static Response<string> GetErrorResponse(string errorCode)
        {
            return new Response<string>()
            {
                Status = Status.BusinessException,
                Errors = new()
                {
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };
        }
    }
}
