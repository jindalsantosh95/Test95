using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.UpdateBasicCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class BasicCustomerCommand : CloudEvent, IRequest<bool>
    {

    }
    public class BasicCustomerCommandHandler : IRequestHandler<BasicCustomerCommand, bool>
    {
        private readonly ILogger<BasicCustomerCommandHandler> _logger;
        private readonly IMediator _mediator;
        private readonly ICustomerRepository _customerRepository;
        private readonly ICustomerSearchRepository _customerSearchRepository;
        private readonly ICustomerProfileRepository _customerProfileRepository;
        private readonly IMemberProfileRepository _memberProfileRepository;
        private readonly IPartnerDocumentRepository _partnerDocumentRepository;
        private readonly IMembershipChangeRepository _membershipChangeRepository;
        public BasicCustomerCommandHandler(ILogger<BasicCustomerCommandHandler> logger, IMediator mediator, ICustomerRepository customerRepository, IMemberProfileRepository memberProfileRepository, ICustomerSearchRepository customerSearchRepository, ICustomerProfileRepository customerProfileRepository
            , IPartnerDocumentRepository partnerDocumentRepository, IMembershipChangeRepository membershipChangeRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _mediator = mediator;
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _memberProfileRepository = memberProfileRepository ?? throw new ArgumentNullException(nameof(memberProfileRepository));
            _customerProfileRepository = customerProfileRepository ?? throw new ArgumentNullException(nameof(customerProfileRepository));
            _partnerDocumentRepository = partnerDocumentRepository ?? throw new ArgumentNullException(nameof(partnerDocumentRepository));
            _customerSearchRepository = customerSearchRepository ?? throw new ArgumentNullException(nameof(customerSearchRepository));
            _membershipChangeRepository = membershipChangeRepository ?? throw new ArgumentNullException(nameof(membershipChangeRepository));
        }
        public async Task<bool> Handle(BasicCustomerCommand request, CancellationToken cancellationToken)
        {
            _logger.LogInformation($"Enrol Basic Customer Command handler-Start");
            try
            {
                var req = Json.Deserialize<CreateCustomerEntities>(Convert.ToString(request.Data));

                if (await _mediator.Send(new FetchDocumentDetailsQuery(req)))
                {
                    _logger.LogDebug($"Updating Preferences - Start");
                    await _mediator.Send(new UpdatePreferencesRepoCommand(req), cancellationToken);
                    _logger.LogDebug($"Updating Preferences - End");

                    return true;
                }
                else
                {
                    try
                    {
                        if (req.EnrollmentChannel == Constants.Application.WebChannelID)
                        {
                            _logger.LogDebug($"Enrol Web Basic Customer-Start");
                            await EnrolWebBasicCustomer(req);
                            _logger.LogDebug($"Enrol Web Basic Customer-End");
                        }
                        else
                        {
                            _logger.LogDebug($"Enrol Retail Basic Customer-Start");
                            await EnrolRetailBasicCustomer(req);
                            _logger.LogDebug($"Enrol Retail Basic Customer-End");
                        }

                        return true;
                    }

                    catch (Exception ex)
                    {
                        _logger.LogInformation($"Document Creation failed: {ex.Message}");
                        return false;
                    }
                }
            }

            finally
            {
                _logger.LogInformation($"Enrol Basic Customer Command handler-End");
            }
        }

        public async Task EnrolWebBasicCustomer(CreateCustomerEntities createCustomerRequest)
        {
            var addCustomerDetatils = _customerRepository.AddCustomerDetails(createCustomerRequest);
            var addCustomerSearchDetails = _customerSearchRepository.AddCustomerSearchDetails(createCustomerRequest);
            var addCustomerProfileDetails = _customerProfileRepository.AddCustomerProfileDetails(createCustomerRequest);
            var addPartnerDetails = _partnerDocumentRepository.AddPartnerDetails(createCustomerRequest);
            var addMemberProfileDetails = _memberProfileRepository.AddMemberProfileDetails(createCustomerRequest);
            var addMembershipChangeDetails = _membershipChangeRepository.AddMembershipChangeDetails(createCustomerRequest);
            await Task.WhenAll(addCustomerDetatils, addCustomerSearchDetails, addCustomerProfileDetails, addPartnerDetails, addMemberProfileDetails, addMembershipChangeDetails);
        }

        public async Task EnrolRetailBasicCustomer(CreateCustomerEntities createCustomerRequest)
        {
            var addCustomerDetatils = _customerRepository.AddCustomerDetails(createCustomerRequest);
            var addCustomerSearchDetails = _customerSearchRepository.AddCustomerSearchDetails(createCustomerRequest);
            var addCustomerProfileDetails = _customerProfileRepository.AddCustomerProfileDetails(createCustomerRequest);
            await Task.WhenAll(addCustomerDetatils, addCustomerSearchDetails, addCustomerProfileDetails);

            var preferredEmail = createCustomerRequest.ContactDetails?.EmailDetails?.Find(email => email.PreferredFlag.Is("Y"));
            if (preferredEmail is not null && !preferredEmail.Valid.Is("N"))
            {
                _logger.LogInformation("Send Email Triggering for Basic Event-Start");
                await SendEmail(createCustomerRequest);
                _logger.LogInformation("Send Email Triggering for Basic Event-End");
            }
        }

        private async Task SendEmail(CreateCustomerEntities createCustomerEventRequest)
        {
            EmailTriggeringEventRequest eventData = EmailTriggeringEventRequestMapper.CreateEventGridSchemaData(createCustomerEventRequest);
            var eventGridEvent = EmailTriggeringEventGridMapper.EventDataMapper(eventData);
            await _mediator.Publish(eventGridEvent, CancellationToken.None);
        }
    }
}
