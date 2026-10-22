using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Repository;
using VAA.EAPI.Microservices.Loyalty.Customer.Application.Query;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.CustomerDoc;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.CustomerDocCommand
{
    public class CustomerUpdateCommandHandler : IRequestHandler<CustomerUpdateCommand, Result<Unit>>
    {
        private readonly IMediator _mediator;
        private readonly ILogger<CustomerUpdateCommandHandler> _logger;

        public CustomerUpdateCommandHandler(IMediator mediator, ILogger<CustomerUpdateCommandHandler> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Result<Unit>> Handle(CustomerUpdateCommand request, CancellationToken cancellationToken)
        {
            Result<Response<CustomerUpdateCommand>> result = null;

            try
            {
                var docUpdateData = Json.DeserializeObject<Customers>((JToken)request.Data);
                Customers existingDoc = (await _mediator.Send(new FetchCustomerDocumentQuery(docUpdateData.CustomerId), cancellationToken)).Value.Value;

                if (existingDoc is null)
                    throw new ArgumentNullException(nameof(existingDoc));

                //Update fields in document
                existingDoc.LastUpdatedBy = docUpdateData.LastUpdatedBy;
                existingDoc.LastUpdatedOn = docUpdateData.LastUpdatedOn;
                existingDoc.Status = !string.IsNullOrWhiteSpace(docUpdateData.Status) ? docUpdateData.Status : existingDoc.Status;

                if (docUpdateData.AddressDetails is not null && docUpdateData.AddressDetails.Any())
                {
                    Address addressdetail = existingDoc.AddressDetails.FirstOrDefault(a => a.AddressId == docUpdateData.AddressDetails.FirstOrDefault().AddressId);
                    Address updatedAddress = docUpdateData.AddressDetails?.FirstOrDefault();
                    if (updatedAddress is not null)
                    {
                        addressdetail.AdditionalComments = updatedAddress.AdditionalComments;
                        addressdetail.AddressId = updatedAddress.AddressId;
                        addressdetail.AddressLine1 = updatedAddress.AddressLine1;
                        addressdetail.AddressLine2 = updatedAddress.AddressLine2;
                        addressdetail.AddressLine3 = updatedAddress.AddressLine3;
                        addressdetail.AddressType = updatedAddress.AddressType;
                        addressdetail.AddressVerified = updatedAddress.AddressVerified;
                        addressdetail.City = updatedAddress.City;
                        addressdetail.Country = updatedAddress.Country;
                        addressdetail.CreatedBy = updatedAddress.CreatedBy;
                        addressdetail.CreatedOn = updatedAddress.CreatedOn;
                        addressdetail.LandlordOrCompanyName = updatedAddress.LandlordOrCompanyName;
                        addressdetail.LastUpdatedBy = updatedAddress.LastUpdatedBy;
                        addressdetail.LastUpdatedOn = updatedAddress.LastUpdatedOn;
                        addressdetail.MetroArea = updatedAddress.MetroArea;
                        addressdetail.PostalStatus = updatedAddress.PostalStatus;
                        addressdetail.PostCode = updatedAddress.PostCode;
                        addressdetail.Primary = updatedAddress.Primary;
                        addressdetail.Province = updatedAddress.Province;
                        addressdetail.SourceIdentifier = updatedAddress.SourceIdentifier;
                        addressdetail.SourceSystem = updatedAddress.SourceSystem;
                        addressdetail.Valid = updatedAddress.Valid;
                        addressdetail.GeoCodeValidFlag = updatedAddress.GeoCodeValidFlag;
                        addressdetail.AddressVerificationStatus = updatedAddress.AddressVerificationStatus;
                        addressdetail.GroupAddress = updatedAddress.GroupAddress;
                        addressdetail.PartnerFileId = updatedAddress.PartnerFileId;
                        existingDoc.AddressDetails = new Address[] { addressdetail };
                    }
                }

                await _mediator.Send(new CustomerRepoUpdateCommand(existingDoc), cancellationToken);

            }
            catch (Exception ex)
            {
                result = Result.Failure<Response<CustomerUpdateCommand>>(ex.Message.ToString());
                await _mediator.Send(new ServiceResponseInsertCommand { Id = request.Id, JsonResponseData = Json.Serialize(result), Status = result.IsFailure ? ServiceResponseProcessStatus.Error : ServiceResponseProcessStatus.Completed, Type = Constants.Application.CommandResponse });
            }

            return Unit.Value;
        }
    }
}
