using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;
using System.Threading;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ContactHistoryCommand;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class UpdateEmailCommandHandler : IRequestHandler<UpdateEmailCommand, Response<string>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdateEmailCommandHandler> _logger;
        private readonly IDeDupRepositoryEnrolment _deduprepository;
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public UpdateEmailCommandHandler(IMediator mediator, ILogger<UpdateEmailCommandHandler> logger, IDeDupRepositoryEnrolment deduprepository, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deduprepository = deduprepository ?? throw new ArgumentNullException(nameof(deduprepository));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentException(nameof(commonRefDBRepository));
        }

        public async Task<Response<string>> Handle(UpdateEmailCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {
                string customerDtls = await _deduprepository.GetCustomersWithEmail(request.NewEmailAddress);
                if (!string.IsNullOrEmpty(customerDtls))
                    return GetErrorResponse(Constants.ErrorCodes.EmailExistsInCosmosDB);

                //update Customer and CustomerSearch document in Cosmos 
                CustomerSearch existingDocCustomerSearch = (await _mediator.Send(new CustomerSearchQuery(request.MemberNumber), cancellationToken)).Value.Value;

                Customers existingDocCustomer = (await _mediator.Send(new FetchCustomerDocumentQuery(existingDocCustomerSearch.CustomerId), cancellationToken)).Value.Value;

                ContactHistory contactHistory = ContactHistoryMapper.EmailMapRequest(existingDocCustomer);

                string currentEmailAddress = existingDocCustomerSearch.EmailAddress;

                existingDocCustomerSearch.EmailAddress = request.NewEmailAddress;

                var updateCustomerSearchDoc = Task.Run(async () => await _mediator.Send(new CustomerSearchRepoUpdateCommand(existingDocCustomerSearch), cancellationToken));
                var updateCustomerDoc = Task.Run(async () => await _mediator.Send(new CustomerRepoUpdateCommand(CustomerDocumentUpdateMapper.UpdateEmailDataMapperCustomer(existingDocCustomer, request.NewEmailAddress)), cancellationToken));
                var createContactHistoryDoc = Task.Run(async () => await _mediator.Send(new ContactHistoryCreateCommand(contactHistory), cancellationToken));

                await Task.WhenAll(updateCustomerSearchDoc, updateCustomerDoc, createContactHistoryDoc);

                var countryDetails = await _commonRefDBRepository.GetCountryCodeDetails();

                string countryCode = countryDetails.Where(x => x.Value == existingDocCustomer.AddressDetails.First(x => x.Primary == "Y").Country).First(x => x.Name != existingDocCustomer.AddressDetails.First(x => x.Primary == "Y").Country).Name;

                Customers updatedCustomerDoc = (await _mediator.Send(new FetchCustomerDocumentQuery(existingDocCustomerSearch.CustomerId), cancellationToken)).Value.Value;

                if (updatedCustomerDoc.ContactDetails.First(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y").ContactValue == request.NewEmailAddress)
                {
                    //publish event for B2C, OCDB and VRED email update

                    var correlationIdForEvents = Guid.NewGuid().ToString();
                    try
                    {
                        await Task.WhenAll(PublishEvent(UpdateEventDataMapper.EventDataMapperOCDB(updatedCustomerDoc, countryCode), correlationIdForEvents, Constants.Application.UpdateEmailOCDBEventType, cancellationToken),
                                           PublishEvent(UpdateEventDataMapper.EventDataMapperVRED(updatedCustomerDoc), correlationIdForEvents, Constants.Application.UpdateEmailVREDEventType, cancellationToken),
                                           PublishEvent(UpdateEventDataMapper.EventDataMapperB2C(existingDocCustomerSearch, currentEmailAddress), correlationIdForEvents, Constants.Application.UpdateEmailVREDEventType, cancellationToken));

                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Error occurred while publishing cloud events. {ex.Message}");
                        return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
                    }
                }

                return new Response<string>()
                {
                    Status = Status.OK,
                    Value = "Email Update Successful",
                    Errors = null
                };
            }

            catch (Exception ex)
            {
                _logger.LogError(ex.Message, $"Error occured while updating email address");
                return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
            }
        }

        private async Task PublishEvent(CloudEvent emailEvent, string correlationIdForEvents, string type, CancellationToken cancellationToken)
        {

            await _mediator.Send(new EventRequestInsertCommand { Id = correlationIdForEvents, Type = type, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);

            try
            {
                await _mediator.Publish(emailEvent, cancellationToken);
            }
            catch
            {
                await _mediator.Send(new EventStoreRequestDeleteCommand { Id = correlationIdForEvents, Type = type, Data = emailEvent.Data, Subject = emailEvent.Subject }, cancellationToken);
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
