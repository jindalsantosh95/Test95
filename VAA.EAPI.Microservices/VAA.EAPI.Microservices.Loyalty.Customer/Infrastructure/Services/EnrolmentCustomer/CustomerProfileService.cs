using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Infrastructure.Services.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.CustomerSearch;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Services.EnrolmentCustomer
{
    public class CustomerProfileService : ICustomerProfileService
    {
        private readonly IRequestHandler<CosmosReaderServiceQuery, List<object>> _cosmosReaderServiceQueryHandler;

        public CustomerProfileService(IRequestHandler<CosmosReaderServiceQuery, List<object>> cosmosReaderServiceQueryHandler)
        {
            _cosmosReaderServiceQueryHandler = cosmosReaderServiceQueryHandler;
        }

        public async Task<IEnumerable<Customers>> GetCustomerDetails(CustomerProfileRequest customerProfileRequest)
        {
            List<KeyValuePair<string, object>> parameters = new();
            string customerId = customerProfileRequest.CustomerId;

            if (customerProfileRequest.Email != null)
            {
                parameters.Add(new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType));
                parameters.Add(new KeyValuePair<string, object>("emailId", customerProfileRequest.Email.ToLower()));

                var customerSearchResponse = await _cosmosReaderServiceQueryHandler.Handle(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.EmailIdSqlQuery), CancellationToken.None);
                if (customerSearchResponse != null && customerSearchResponse.Count > 0)
                    customerId = Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.First()).CustomerId;
            }
            if (customerProfileRequest.MemberNumber != null)
            {
                parameters.Add(new KeyValuePair<string, object>("partitionKey", Constants.Application.CustomerSearchDocType));
                parameters.Add(new KeyValuePair<string, object>("memberNumber", customerProfileRequest.MemberNumber));

                var customerSearchResponse = await _cosmosReaderServiceQueryHandler.Handle(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnrTest), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.MemberNumberSqlQuery), CancellationToken.None);
                if (customerSearchResponse != null && customerSearchResponse.Count > 0)
                    customerId = Json.DeserializeObject<CustomerSearch>((JToken)customerSearchResponse.First()).CustomerId;
            }
            if (customerId != null)
            {
                parameters.Add(new KeyValuePair<string, object>("partitionKey", customerId));
                parameters.Add(new KeyValuePair<string, object>("docType", DocumentType.Customer.ToString()));

                var customerDetails = Json.Deserialize<IEnumerable<Customers>>(Json.Serialize(await _cosmosReaderServiceQueryHandler.Handle(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr), ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustDB), parameters, Constants.Application.SearchDbQuery), CancellationToken.None)));
                if (customerDetails != null)
                {

                    List<KeyValuePair<string, object>> keyValues = new()
                    {
                        new KeyValuePair<string, object>("partitionKey", customerDetails.First().CustomerId),
                        new KeyValuePair<string, object>("id", customerDetails.First().LoyaltyMemberNumber),
                        new KeyValuePair<string, object>("docType", DocumentType.FlyingClubMember.ToString())
                    };

                    var memberProfileResponse = await _cosmosReaderServiceQueryHandler.Handle(CosmosQueryHandlerMapper.Mapper(ConfigReader.Get(Constants.Environment.SCVCustTxnDBMmbrPrfleContnr), ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint), ConfigReader.Get(Constants.Environment.SCVCustTxnDB), keyValues, Constants.Application.MemberPrfDbQuery), CancellationToken.None);

                    if (memberProfileResponse.Count > 0)
                    {
                        var memberProfile = Json.DeserializeObject<MemberProfile>((JToken)memberProfileResponse.First());
                        if ((memberProfile?.TierCode).Is(TierCode.Glass.ToString()) || (memberProfile?.TierCode).Is(TierCode.VAC.ToString()))
                            return null;
                        return customerDetails;
                    }
                }
            }

            return null;
        }

    }
}
