using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Domain;
using VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Infrasturcture.Repository;

namespace VAA.EAPI.Microservices.Loyalty.ChangeFeedProcessor.Application.Command.PhoneUpdate
{
    public class CustomerSearchUpdateCommandHandler : IRequestHandler<CustomerSearchUpdateCommand, bool>
    {
        private readonly IMediator _mediator;

        public CustomerSearchUpdateCommandHandler(IMediator mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        public async Task<bool> Handle(CustomerSearchUpdateCommand request, CancellationToken cancellationToken)
        {
            var existingDoc = await _mediator.Send(new CustomerSearchRepoQuery(request.Customer.LoyaltyMemberNumber, DocumentType.CustomerSearch.ToString()));

            if (existingDoc != null && existingDoc.Any())
            {
                var existingCustomerSearch = existingDoc?.Select(e => Json.DeserializeObject<CustomerSearchDTO>((JToken)e)).First();
                
                existingCustomerSearch.PhoneNumber = request.Customer.ContactDetails?.FirstOrDefault(x => x.ContactType == "Phone")?.ContactValue;
                existingCustomerSearch.PhoneCountryCode = request.Customer.ContactDetails?.FirstOrDefault(x => x.ContactType == "Phone")?.CountryCode;
                
                await _mediator.Send(new CustomerSearchUpdateRepoCommand(new Infrastructure.Repository.CosmosSqlDBv2.CosmosDocumentEvent(existingCustomerSearch, Infrastructure.Entities.Enums.CosmosModificationType.Replace)));
            }

            return true;
        }
    }
}
