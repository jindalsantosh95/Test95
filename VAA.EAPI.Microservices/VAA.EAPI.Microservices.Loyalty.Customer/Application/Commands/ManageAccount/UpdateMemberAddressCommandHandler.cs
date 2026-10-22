using MediatR;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Events;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Infrastructure.Repository.EventStore;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.AddressHistoryDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerSearchDocument;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enrolment.Interface.IRepository.CommonRefDB;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Enums;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.ManageAccount;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.ManageAccount
{

    public class UpdateMemberAddressCommandHandler : IRequestHandler<UpdateMemberAddressCommand, Response<string>>
    {

        private readonly IMediator _mediator;
        private readonly ILogger<UpdateMemberAddressCommand> _logger;
        private readonly ICommonRefDBRepository _commonRefDBRepository;

        public UpdateMemberAddressCommandHandler(IMediator mediator, ILogger<UpdateMemberAddressCommand> logger, ICommonRefDBRepository commonRefDBRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _commonRefDBRepository = commonRefDBRepository ?? throw new ArgumentException(nameof(commonRefDBRepository));
        }

        public async Task<Response<string>> Handle(UpdateMemberAddressCommand request, CancellationToken cancellationToken)
        {
            try
            {
                Customers customerDoc = (await _mediator.Send(new FetchCustomerDocumentQuery(request.MemberId), cancellationToken)).Value.Value;
                CustomerSearch customerSearchDoc = (await _mediator.Send(new CustomerSearchQuery(customerDoc.LoyaltyMemberNumber), cancellationToken)).Value.Value;

                await _mediator.Send(new AddressHistoryRepoCreateCommand((AddressHistory)customerDoc), cancellationToken);

                customerDoc = CustomerDocumentUpdateMapper.UpdateCustomerAddresMapper(customerDoc, request.AddressDetail);
                customerSearchDoc = CustomerDocumentUpdateMapper.UpdateCustomerSearchAddresMapper(customerSearchDoc, request.AddressDetail);

                var countryDetails = await _commonRefDBRepository.GetCountryCodeDetails();
                string countryCode = countryDetails.Where(x => x.Value.Is(request.AddressDetail.Country)).First(x => !x.Name.Is(request.AddressDetail.Country)).Name;
                
                customerDoc.AddressDetails.First(x => x.Primary == "Y").MetroArea = _commonRefDBRepository.GetMetroArea(countryCode)?.Result?.AdditionalValue1;

                string matchString = MatchString.GetMatchString(customerDoc, countryCode, Constants.Application.WebMyaChannelID);
                customerDoc.MatchString = matchString;
                customerSearchDoc.MatchString = matchString;

                var updateCustomerSearchDoc = Task.Run(async () => await _mediator.Send(new CustomerSearchRepoUpdateCommand(customerSearchDoc), cancellationToken));
                var updateCustomerDoc = Task.Run(async () => await _mediator.Send(new CustomerRepoUpdateCommand(customerDoc), cancellationToken));

                await Task.WhenAll(updateCustomerSearchDoc, updateCustomerDoc);

                if ((await _mediator.Send(new CustomerSearchQuery(customerDoc.LoyaltyMemberNumber), cancellationToken)).Value.Value.MatchString == matchString)
                {
                    var correlationIdForEvents = Guid.NewGuid().ToString();
                    var ocdbSyncCloudEvent = UpdateEventDataMapper.EventDataMapperOCDB(customerDoc, countryCode);
                    var emailCloudEvent = UpdateEventDataMapper.EmailEventDataMapper(customerSearchDoc.EmailAddress);

                    try
                    {
                        await Task.WhenAll(PublishEvent(ocdbSyncCloudEvent, correlationIdForEvents, Constants.Application.UpdateAddressOcdbEventType, cancellationToken),
                                           PublishEvent(emailCloudEvent, correlationIdForEvents, Constants.Application.UpdateAddressEmailEventType, cancellationToken));

                    }

                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error occured while updating email address");
                        return GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);
                    }

                    var result = new Response<string>
                    {
                        Status = Status.OK,
                        Value = "Updated Member Address Successfully"
                    };

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Completed, Type = ManageAccountUpdateType.UpdateAddress.ToString() }, cancellationToken);

                    return result;
                }
                else
                {
                    var result = GetErrorResponse(Constants.ErrorCodes.MiscellaneousTechnicalError);

                    await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = ServiceResponseProcessStatus.Error, Type = ManageAccountUpdateType.UpdateAddress.ToString() }, cancellationToken);

                    return result;
                }

            }

            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error occured while updating member address details");
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
                    ErrorMapper.MapBusinessExceptionToEAPIException(new Exception(errorCode), Guid.NewGuid().ToString(), VAA.EAPI.Infrastructure.Constants.Constants.ErrorCodesPath).FirstOrDefault()
                }
            };

        }
    }
}
