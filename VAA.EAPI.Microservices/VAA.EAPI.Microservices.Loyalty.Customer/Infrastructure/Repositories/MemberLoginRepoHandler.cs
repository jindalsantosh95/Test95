using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories
{
    public class MemberLoginRepoHandler : IRequestHandler<MemberLoginRepoQuery, string>
    {
        private readonly IMediator _mediator;
        private readonly ICustomerService<CustomersDTO> _customerService;

        public MemberLoginRepoHandler(IMediator mediator, ICustomerService<CustomersDTO> customerService)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
        }

        public async Task<string> Handle(MemberLoginRepoQuery request, CancellationToken cancellationToken)
        {
            var customerStatus = string.Empty;
            string fcNumber = string.Empty;
            if (string.IsNullOrEmpty(request.MemberNumber) && !string.IsNullOrEmpty(request.EmailAddress))
            {
                string EmailId = request.EmailAddress.ToLower();
                List<KeyValuePair<string, object>> parameters = new()
                {
                    new KeyValuePair<string, object>("emailId", EmailId),

                };
                var customerSearchResponse = await _mediator.Send(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.EmailIdSqlQuery));

                if (customerSearchResponse.Any())
                {
                    fcNumber = Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.FirstOrDefault()).LoyaltyMemberNumber;
                }
            }

            if (!string.IsNullOrEmpty(request.MemberNumber) || !string.IsNullOrEmpty(fcNumber))
            {
                var customerResponse = await _customerService.GetItemAsync(DocumentType.Customer, Constants.Application.FcNumberSqlQuery, !string.IsNullOrEmpty(request.MemberNumber) ? request.MemberNumber : fcNumber);

                if (!customerResponse.Any())
                    return string.Empty;

                customerStatus = customerResponse.FirstOrDefault().Status;
            }
            return customerStatus;
        }
    }
}
