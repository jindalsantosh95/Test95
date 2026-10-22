using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ContactHistoryCommand;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.DuplicateCheck;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using CSharpFunctionalExtensions;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{
    public class UpdatePhoneNumberCommandHandler : IRequestHandler<UpdatePhoneNumberCommand, Response<string>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<UpdatePhoneNumberCommandHandler> _logger;
        private readonly IDeDupRepositoryEnrolment _deduprepository;
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public UpdatePhoneNumberCommandHandler(IMediator mediator, ILogger<UpdatePhoneNumberCommandHandler> logger, IDeDupRepositoryEnrolment deduprepository, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _deduprepository = deduprepository ?? throw new ArgumentNullException(nameof(deduprepository));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentException(nameof(commonRefDBRepository));
        }

        public async Task<Response<string>> Handle(UpdatePhoneNumberCommand request, CancellationToken cancellationToken)
        {
            if (request == null) throw new ArgumentNullException(nameof(request));

            try
            {

                //update Customer document in Cosmos 
                Customers CustomerDoc = (await _mediator.Send(new FetchCustomerDocumentQuery(request.CustomerID), cancellationToken)).Value.Value;

                ContactHistory contactHistory = (ContactHistory)CustomerDoc;

                var updateCustomerDoc = Task.Run(async () => await _mediator.Send(new CustomerRepoUpdateCommand(CustomerDocumentUpdateMapper.UpdatePhoneDataMapperCustomer(CustomerDoc, request)), cancellationToken));
                var createContactHistoryDoc = Task.Run(async () => await _mediator.Send(new ContactHistoryCreateCommand(contactHistory), cancellationToken));

                await Task.WhenAll(updateCustomerDoc, createContactHistoryDoc);

                var countryDetails = await _commonRefDBRepository.GetCountryCodeDetails();

                string countryCode = countryDetails.Where(x => x.Value == CustomerDoc.AddressDetails.First(x => x.Primary == "Y").Country).First(x => x.Name != CustomerDoc.AddressDetails.First(x => x.Primary == "Y").Country).Name;

                Customers updatedCustomerDoc = (await _mediator.Send(new FetchCustomerDocumentQuery(CustomerDoc.CustomerId), cancellationToken)).Value.Value;

                if (updatedCustomerDoc.ContactDetails.First(x => x.ContactType == Constants.Application.PhoneContactType && x.Primary == "Y").ContactValue == request.PhoneNumber)
                {
                    var emailAddress = updatedCustomerDoc.ContactDetails.First(x => x.ContactType == Constants.Application.EmailContactType && x.Primary == "Y").ContactValue;
                    var publishTasks = new List<Action>
                    {
                        async () => await _mediator.Publish(UpdateEventDataMapper.EventDataMapperEmailMS(updatedCustomerDoc,emailAddress), CancellationToken.None),
                        async () =>await _mediator.Publish(UpdateEventDataMapper.EventDataMapperOCDB(updatedCustomerDoc, countryCode), CancellationToken.None)
                    };

                    Parallel.Invoke(publishTasks.ToArray());
                }
                
                var result= new Response<string>()
                {
                    Status = Status.OK,
                    Value = "Updated Phone Number Successfully",
                    Errors = null
                };
                
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.CorrelationId, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = ManageAccountUpdateType.UpdatePhoneNumber.ToString() }, cancellationToken);

                return result;
            }

            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occured while updating member phone number");
                
                var result = Result.Failure<Response<UpdatePhoneNumberCommand>>(ex.Message.ToString());
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.CorrelationId, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Error, Type = ManageAccountUpdateType.UpdatePhoneNumber.ToString() }, cancellationToken);
                
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
