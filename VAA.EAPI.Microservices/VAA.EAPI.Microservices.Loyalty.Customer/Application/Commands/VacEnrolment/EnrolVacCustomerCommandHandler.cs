using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class EnrolVacCustomerCommandHandler : IRequestHandler<EnrolVacCustomerCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<EnrolVacCustomerCommandHandler> _logger;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSearchRepository _customerSearchRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IMemberProfileRepository _memberProfileRepository;
        private readonly IPartnerDocumentRepository _partnerDocumentRepository;
        private readonly IMembershipChangeRepository _membershipChangeRepository;
        public readonly IGetCustTransDocumentService<MembershipChange> _custTransMbrChangeService;
        public readonly IGetCustTransDocumentService<MemberProfile> _custTransFcmService;
        public readonly IGetCustTransDocumentService<PartnerMembership> _custTransPartnerMbrShipService;
        public readonly IGetCustTransDocumentService<CustomerProfile> _custTransCustomerProfileService;

        public EnrolVacCustomerCommandHandler(IMediator mediator, ILogger<EnrolVacCustomerCommandHandler> logger, ICustomerRepository customerRepository, IMemberProfileRepository memberProfileRepository, ICustomerSearchRepository customerSearchRepository, ICustomerProfileRepository customerProfileRepository, IPartnerDocumentRepository partnerDocumentRepository, IMembershipChangeRepository membershipChangeRepository, IGetCustTransDocumentService<MembershipChange> custTransMbrChangeService, IGetCustTransDocumentService<MemberProfile> custTransFcmService, IGetCustTransDocumentService<PartnerMembership> custTransPartnerMbrShipService, IGetCustTransDocumentService<CustomerProfile> custTransCustomerProfileService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
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

        public async Task<Result<Unit>> Handle(EnrolVacCustomerCommand request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                if (!request.IsBasic)
                    await EnrolNewVacMember(request.VaCustomerDetails);

                else
                {
                    Customers customerDoc = (await _mediator.Send(new FetchCustomerDocumentQuery(request.VaCustomerDetails.CustomerId), cancellationToken)).Value.Value;

                    MembershipChange mbrChangeDoc = await _custTransMbrChangeService.GetCosmosDoc(DocumentType.MembershipChange.ToString(), request.VaCustomerDetails.CustomerId);

                    MemberProfile fcmDoc = await _custTransFcmService.GetCosmosDoc(DocumentType.FlyingClubMember.ToString(), request.VaCustomerDetails.CustomerId);

                    PartnerMembership partnerMbrShipDoc = await _custTransPartnerMbrShipService.GetCosmosDoc(DocumentType.PartnerMembership.ToString(), request.VaCustomerDetails.CustomerId);

                    CustomerProfile custProfileDoc = await _custTransCustomerProfileService.GetCosmosDoc(DocumentType.CustomerProfile.ToString(), request.VaCustomerDetails.CustomerId);

                    await UpgradeBasicToVacMember(request.VaCustomerDetails, customerDoc, mbrChangeDoc, fcmDoc, partnerMbrShipDoc, custProfileDoc);
                }         
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while creating document in cosmos db for VAC Member.");
            }

            return Unit.Value;
        }

        private async Task UpgradeBasicToVacMember(CreateCustomerEntities enrolmentRequest, Customers customerDoc, MembershipChange mbrChangeDoc, MemberProfile fcmDoc, PartnerMembership partnerMbrShipDoc, CustomerProfile custProfileDoc)
        {
            var updateCustomerSearchDetails = _customerSearchRepository.UpdateCustomerSearchDetails(enrolmentRequest, customerDoc);
            var updateCustomerDetatils = _customerRepository.UpdateCustomerDetails(enrolmentRequest, customerDoc);
            var updateCustomerProfileDetails = _customerProfileRepository.UpdateCustomerProfileDetails(enrolmentRequest, custProfileDoc);
            var updateMemberProfileDetails = _memberProfileRepository.UpdateMemberProfileDetails(enrolmentRequest, fcmDoc);
            var updatePartnerDetails = _partnerDocumentRepository.UpdatePartnerDetails(enrolmentRequest, partnerMbrShipDoc);
            var updateMembershipChange = _membershipChangeRepository.UpdateMembershipChangeDetails(enrolmentRequest, mbrChangeDoc);

            await Task.WhenAll(updateCustomerSearchDetails, updateCustomerDetatils, updateCustomerProfileDetails, updateMemberProfileDetails, updatePartnerDetails, updateMembershipChange);
        }

        private async Task EnrolNewVacMember(CreateCustomerEntities enrolmentRequest)
        {
            var addCustomerSearchDetails = _customerSearchRepository.AddCustomerSearchDetails(enrolmentRequest);
            var addCustomerDetatils = _customerRepository.AddCustomerDetails(enrolmentRequest);
            var addCustomerProfileDetails = _customerProfileRepository.AddCustomerProfileDetails(enrolmentRequest);
            var addMemberProfileDetails = _memberProfileRepository.AddMemberProfileDetails(enrolmentRequest);
            var addPartnerDetails = _partnerDocumentRepository.AddPartnerDetails(enrolmentRequest);
            var addMembershipChange = _membershipChangeRepository.AddMembershipChangeDetails(enrolmentRequest);

            await Task.WhenAll(addCustomerDetatils, addCustomerSearchDetails, addCustomerProfileDetails, addPartnerDetails, addMemberProfileDetails, addMembershipChange);
        }
    }
}
