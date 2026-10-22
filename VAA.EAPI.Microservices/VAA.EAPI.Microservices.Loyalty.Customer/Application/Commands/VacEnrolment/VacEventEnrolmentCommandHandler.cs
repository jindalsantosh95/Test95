using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.FCNumberGeneration;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.VacEnrolment
{
    public class VacEventEnrolmentCommandHandler : IRequestHandler<VacEventEnrolmentCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<VacEventEnrolmentCommandHandler> _logger;
        private readonly IFCNumberGenerationRepository _fCNumberGenerationRepository;
        private readonly IDeDupRepositoryEnrolment _deduprepository;

        public VacEventEnrolmentCommandHandler(IMediator mediator, ILogger<VacEventEnrolmentCommandHandler> logger, IFCNumberGenerationRepository fCNumberGenerationRepository, IDeDupRepositoryEnrolment deduprepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _fCNumberGenerationRepository = fCNumberGenerationRepository ?? throw new ArgumentNullException(nameof(fCNumberGenerationRepository));
            _deduprepository = deduprepository ?? throw new ArgumentNullException(nameof(deduprepository));
        }

        public async Task<Result<Unit>> Handle(VacEventEnrolmentCommand request, CancellationToken cancellationToken)
        {
            if (request.Data == null)
                throw new ArgumentNullException(nameof(request));

            try
            {
                var enrolmentRequest = Json.Deserialize<CreateCustomerEntities>(request.Data.ToString());

                if (enrolmentRequest.EnrollmentChannel.Is(Constants.Application.WebChannelID) && enrolmentRequest.FcNumber.IsNullOrEmpty())
                    await EnrolNewCustomer(enrolmentRequest, cancellationToken);

                else
                    await UpgradeBasicCustomer(enrolmentRequest, cancellationToken);

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error while Enrolling or Upgrading VAC member.");
            }
            
            return Unit.Value;
        }

        private async Task EnrolNewCustomer(CreateCustomerEntities enrolmentRequest, CancellationToken cancellationToken )
        {
           enrolmentRequest.FcNumber = (await _fCNumberGenerationRepository?.GenerateFlyingClubNumber()).Value.Value.Memberdetails.MemberNumber;

           await _mediator.Send(new EnrolVacCustomerCommand(enrolmentRequest, false), cancellationToken);

           await PublishEvent(EventGridEventMapper.B2CEnrolmentDataMapper(enrolmentRequest, Guid.NewGuid().ToString()), cancellationToken);
        }

        private async Task UpgradeBasicCustomer(CreateCustomerEntities enrolmentRequest, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EnrolVacCustomerCommand(enrolmentRequest, true), cancellationToken);

            await PublishEvent(EventGridEventMapper.B2CEnrolmentDataMapper(enrolmentRequest, Guid.NewGuid().ToString()), cancellationToken);
        }

        private async Task PublishEvent(CloudEvent b2cAccountCreationEvent, CancellationToken cancellationToken)
        {
            await _mediator.Send(new EventRequestInsertCommand { Id = b2cAccountCreationEvent.Id, Type = Constants.Application.b2cEnrolmentEvent, Data = b2cAccountCreationEvent.Data, Subject = b2cAccountCreationEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(b2cAccountCreationEvent, cancellationToken);
            }

            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = b2cAccountCreationEvent.Id, Type = Constants.Application.b2cEnrolmentEvent, Data = b2cAccountCreationEvent.Data, Subject = b2cAccountCreationEvent.Subject }, cancellationToken);
                throw;
            }
        }
    }
}
