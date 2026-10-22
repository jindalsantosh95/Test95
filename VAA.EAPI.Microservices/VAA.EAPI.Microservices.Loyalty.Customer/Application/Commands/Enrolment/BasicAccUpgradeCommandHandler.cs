using CSharpFunctionalExtensions;
using MediatR;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Infrastructure.Repository.CosmosSqlDBv2;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Domain.IRepository;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Entities;
using VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Interfaces.EnrolmentCustomer;
using VAA.EAPI.Microservices.Loyalty.Customer.Mapper.EnrolmentCustomer;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Application.Commands.Enrolment
{
    public class BasicAccUpgradeCommandHandler : IRequestHandler<BasicAccUpgradeCommand, CreateCustomerEventResponse>
    {

        private readonly IMediator _mediator;
        public readonly ICustomerService<CustomersDTO> _customerService;
        public readonly ICustomerRepository _customerRepository;
        public readonly ICustomerSearchRepository _customerSearchRepository;
        public readonly ICustomerProfileRepository _customerProfileRepository;
        public readonly IMemberProfileRepository _memberProfileRepository;
        public readonly IPartnerDocumentRepository _partnerDocumentRepository;
        public readonly IMembershipChangeRepository _membershipChangeRepository;

        public BasicAccUpgradeCommandHandler(IMediator mediator, ICustomerService<CustomersDTO> customerService, ICustomerRepository customerRepository, ICustomerSearchRepository customerSearchRepository, ICustomerProfileRepository customerProfileRepository, IMemberProfileRepository memberProfileRepository, IPartnerDocumentRepository partnerDocumentRepository, IMembershipChangeRepository membershipChangeRepository)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _customerSearchRepository = customerSearchRepository ?? throw new ArgumentNullException(nameof(customerSearchRepository));
            _customerProfileRepository = customerProfileRepository ?? throw new ArgumentNullException(nameof(customerProfileRepository));
            _memberProfileRepository = memberProfileRepository ?? throw new ArgumentNullException(nameof(memberProfileRepository));
            _partnerDocumentRepository = partnerDocumentRepository ?? throw new ArgumentNullException(nameof(partnerDocumentRepository));
            _membershipChangeRepository = membershipChangeRepository ?? throw new ArgumentNullException(nameof(membershipChangeRepository));
        }

        public async Task<CreateCustomerEventResponse> Handle(BasicAccUpgradeCommand createCustomerEntities, CancellationToken cancellationToken)
        {
            #region Documents updation in SCV Plus (Upgrade Existing Basic customer to Loyalty Customer

            //Get Value of Create don and CreatedBy from Customer Doc            

            Customers customerDetails = await GetCosmosReaderDoc<Customers>((int)DocumentType.Customer, createCustomerEntities.CustomerEntities.CustomerId.ToString(), cancellationToken);

            #region using existing repo class and added update method to replace docs in cosmos
            //Upgrade Customer Entities
            await _customerRepository.UpdateCustomerDetails(createCustomerEntities.CustomerEntities, customerDetails);
            await _customerSearchRepository.UpdateCustomerSearchDetails(createCustomerEntities.CustomerEntities, customerDetails);

            CustomerProfile customerPfl = await GetCosmosReaderDoc<CustomerProfile>((int)DocumentType.CustomerProfile, createCustomerEntities.CustomerEntities.CustomerId.ToString(), cancellationToken);

            await _customerProfileRepository.UpdateCustomerProfileDetails(createCustomerEntities.CustomerEntities, customerPfl);

            MemberProfile fcMemberPrl = await GetCosmosReaderDoc<MemberProfile>((int)DocumentType.FlyingClubMember, createCustomerEntities.CustomerEntities.CustomerId.ToString(), cancellationToken);

            await _memberProfileRepository.UpdateMemberProfileDetails(createCustomerEntities.CustomerEntities, fcMemberPrl);

            #region Get existing PartnerMembership Doc from Cosmos
            PartnerMembership partnerMembershipDoc = await GetCosmosReaderDoc<PartnerMembership>((int)DocumentType.PartnerMembership, createCustomerEntities.CustomerEntities.CustomerId.ToString(), cancellationToken);

            #endregion Get existing PartnerMembership Doc from Cosmos

            #region Update existing PartnerMembership Doc in Cosmos
            await _partnerDocumentRepository.UpdatePartnerDetails(createCustomerEntities.CustomerEntities, partnerMembershipDoc);
            #endregion Update existing PartnerMembership Doc in Cosmos

            #region Get existing MembershipChange Doc from Cosmos
            MembershipChange membershipChangeDoc = await GetCosmosReaderDoc<MembershipChange>((int)DocumentType.MembershipChange, createCustomerEntities.CustomerEntities.CustomerId.ToString(), cancellationToken);
            #endregion Get existing MembershipChange Doc from Cosmos

            #region Update existing MembershipChange Doc in Cosmos
            Result<Response<TierInfo>> tierDetails = await _membershipChangeRepository.UpdateMembershipChangeDetails(createCustomerEntities.CustomerEntities, membershipChangeDoc);
            #endregion Update existing MembershipChange Doc in Cosmos

            var response = CreateCustomerEventEntitiesMapper.CustomerEventResponseMapper(createCustomerEntities.CustomerEntities, tierDetails);
            return response;
            #endregion using existing repo class and added update method to replace docs in cosmos

            #endregion Documents updation in SCV Plus (Upgrade Existing Basic customer to Loyalty Customer
        }


        private async Task<T> GetCosmosReaderDoc<T>(int docType, string customerId, CancellationToken cancellationToken)
        {
            List<KeyValuePair<string, object>> queryParams = new();
            CosmosReaderRepoQuery cosmosReaderRepo = new();
            switch (docType)
            {
                case (int)DocumentType.CustomerProfile:
                    queryParams = new List<KeyValuePair<string, object>>
                    {
                        new KeyValuePair<string, object>("docType", DocumentType.CustomerProfile.ToString()),
                        new KeyValuePair<string, object>("partitionKey", customerId)
                    };
                    cosmosReaderRepo = new CosmosReaderRepoQuery()
                    {
                        CollectionName = ConfigReader.Get(Constants.Environment.SCVCustTxnDBCustPrflCntnr),
                        DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint),
                        DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustTxnDB),
                        QueryParams = queryParams,
                        SqlQuery = Constants.Application.SearchDbQuery
                    };
                    break;

                case (int)DocumentType.FlyingClubMember:
                    queryParams = new List<KeyValuePair<string, object>>
                    {
                        new KeyValuePair<string, object>("docType", DocumentType.FlyingClubMember.ToString()),
                        new KeyValuePair<string, object>("partitionKey", customerId)
                    };
                    cosmosReaderRepo = new CosmosReaderRepoQuery()
                    {
                        CollectionName = ConfigReader.Get(Constants.Environment.SCVCustTxnDBMmbrPrfleContnr),
                        DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint),
                        DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustTxnDB),
                        QueryParams = queryParams,
                        SqlQuery = Constants.Application.SearchDbQuery
                    };
                    break;

                case (int)DocumentType.MembershipChange:
                    queryParams = new List<KeyValuePair<string, object>>
                    {
                        new KeyValuePair<string, object>("docType", DocumentType.MembershipChange.ToString()),
                        new KeyValuePair<string, object>("partitionKey", customerId)
                    };
                    cosmosReaderRepo = new CosmosReaderRepoQuery()
                    {
                        CollectionName = ConfigReader.Get(Constants.Environment.SCVCustTxnDBMmbrshpChgContnr),
                        DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint),
                        DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustTxnDB),
                        QueryParams = queryParams,
                        SqlQuery = Constants.Application.SearchDbQuery
                    };
                    break;

                case (int)DocumentType.PartnerMembership:
                    queryParams = new List<KeyValuePair<string, object>>
                    {
                        new KeyValuePair<string, object>("docType", DocumentType.PartnerMembership.ToString()),
                        new KeyValuePair<string, object>("partitionKey", customerId)
                    };
                    cosmosReaderRepo = new CosmosReaderRepoQuery()
                    {
                        CollectionName = ConfigReader.Get(Constants.Environment.SCVCustTxnDBPartnerMemContnr),
                        DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustTxnDBEndPoint),
                        DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustTxnDB),
                        QueryParams = queryParams,
                        SqlQuery = Constants.Application.SearchDbQuery
                    };
                    break;

                default:
                    queryParams = new List<KeyValuePair<string, object>>
                    {
                        new KeyValuePair<string, object>("docType", DocumentType.Customer.ToString()),
                        new KeyValuePair<string, object>("partitionKey", customerId)
                    };
                    cosmosReaderRepo = new CosmosReaderRepoQuery()
                    {
                        CollectionName = ConfigReader.Get(Constants.Environment.SCVCustDBCustSearchColCntnr),
                        DatabaseEndpoint = ConfigReader.Get(Constants.Environment.SCVCustDBEndPoint),
                        DatabaseName = ConfigReader.Get(Constants.Environment.SCVCustDB),
                        QueryParams = queryParams,
                        SqlQuery = Constants.Application.SearchDbQuery
                    };
                    break;
            }

            return Json.DeserializeObject<T>((JToken)(await _mediator.Send(cosmosReaderRepo, cancellationToken))?.FirstOrDefault());

        }
    }
}
