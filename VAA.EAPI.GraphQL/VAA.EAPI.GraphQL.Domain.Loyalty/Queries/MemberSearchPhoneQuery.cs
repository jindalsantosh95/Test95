using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Mappers;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberSearchViaPhone;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Queries
{
    [ExtendObjectType(OperationTypeNames.Query)]
    public class MemberSearchPhoneQuery
    {
        private readonly ILogger<MemberSearchPhoneQuery> _logger;
        private readonly IVredCardDetailsQueryProvider _vredPointDetailsQueryProvider;
        private readonly ICustomerTransactionQueryProvider _customerTransactionQueryProvider;
        private readonly ICustomerQueryProvider _customerQueryProvider;

        public MemberSearchPhoneQuery(ILogger<MemberSearchPhoneQuery> logger, IVredCardDetailsQueryProvider vredPointDetailsQueryProvider, ICustomerTransactionQueryProvider customerTransactionQueryProvider, ICustomerQueryProvider customerQueryProvider)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _vredPointDetailsQueryProvider = vredPointDetailsQueryProvider ?? throw new ArgumentNullException(nameof(vredPointDetailsQueryProvider));
            _customerTransactionQueryProvider = customerTransactionQueryProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
            _customerQueryProvider = customerQueryProvider ?? throw new ArgumentNullException(nameof(customerTransactionQueryProvider));
        }

        [Authorize(Roles = ["Loyalty.Member.Search"])]
        [GraphQLName("memberSearchbyPhone")]
        [GraphQLDescription("Retreive a list of members via phone number search")]
        public async Task<IEnumerable<MemberInfo>> MemberSearchByPhone(string phoneNumber)
        {
            if (phoneNumber == null) throw new ArgumentNullException(nameof(phoneNumber));

            if (!RegexCompiled.IsMatch(phoneNumber, @"^[0-9]+$"))
            {
                throw new RequestValidationException("Please enter a valid phone number");
            }

            List<MemberInfo> memberSearchResponse = [];

            //Retrieve CustomerSearch 
            var customerSearch = await _customerQueryProvider.GetCustomerSearch(phoneNumber);

            if (!customerSearch.Any())
            {
                throw new RequestValidationException("No data found for the given phone number");
            }

            foreach (var customer in customerSearch)
            {
                try
                {
                    //Retrieve fcm doc
                    var flyingClubMemberDoc = await _customerTransactionQueryProvider.GetFlyingClubMember(customer.CustomerId);

                    if (!flyingClubMemberDoc.FlyingClubMemberDocuments.First().TierCode.Is(Common.Constants.Application.VirginAccountTierCode))
                    {
                        //Retrieve Customer
                        var memberDetails = await _customerQueryProvider.GetCustomerDoc(customer.CustomerId);

                        //Retrieve Member balance
                        var vredPointResponse = await _vredPointDetailsQueryProvider.VredPointBalance(customer.LoyaltyMemberNumber);

                        memberSearchResponse.Add(MemberSearchResponseMapper.MemberSearchResponse(customer, memberDetails, vredPointResponse, flyingClubMemberDoc.FlyingClubMemberDocuments.First()));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Exception occured in Member Search by Phone for CustomerId: {customer.CustomerId}");
                }
            }
            return memberSearchResponse;
        }
    }
}
