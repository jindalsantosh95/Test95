using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Common.Serialisation;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MembersProfile;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.ReferenceData;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class UpdateMemberProfileMutation
    {
        private readonly ILogger<UpdateMemberProfileMutation> _logger;
        private readonly IMemberProfileMutationProvider<string> _memberProfileMutationProvider;
        private readonly ICustomerProvider _customerProvider;
        private readonly ILoyaltyReferenceDataProvider _referenceDataProvider;
        private readonly ICustomerTransactionMutationProvider<MemberPreferenceUpdateResponse> _preferenceUpdateProvider;

        public UpdateMemberProfileMutation(IMemberProfileMutationProvider<string> nonFCMemberMutationProvider, ILogger<UpdateMemberProfileMutation> logger, ICustomerProvider customerProvider, ILoyaltyReferenceDataProvider referenceDataProvider, ICustomerTransactionMutationProvider<MemberPreferenceUpdateResponse> preferenceUpdateProvider)
        {
            _memberProfileMutationProvider = nonFCMemberMutationProvider ?? throw new ArgumentNullException(nameof(nonFCMemberMutationProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _customerProvider = customerProvider ?? throw new ArgumentNullException(nameof(customerProvider));
            _referenceDataProvider = referenceDataProvider ?? throw new ArgumentNullException(nameof(referenceDataProvider));
            _preferenceUpdateProvider = preferenceUpdateProvider ?? throw new ArgumentNullException(nameof(preferenceUpdateProvider));
        }

        [Authorize(Roles = new[] { "Loyalty.MemberProfile.Update" })]
        [GraphQLName("UpdateVACMemberProfile")]
        [GraphQLDescription("Update Members Profile")]
        public async Task<string> UpdateMemberProfile(VACMemberProfileUpdateRequest updateMemberProfileRequest)
        {
            if (updateMemberProfileRequest is null)
                throw new RequestValidationException("Member Profile request parameters must be specified");

            return await MemberProfile(updateMemberProfileRequest);
        }

        private async Task<string> MemberProfile(VACMemberProfileUpdateRequest updateMemberProfileRequest)
        {
            try
            {
                if (string.IsNullOrEmpty(updateMemberProfileRequest.CustomerId) && string.IsNullOrEmpty(updateMemberProfileRequest.MemberNumber))
                    throw new RequestValidationException("Either 'Customer ID' or 'Member Number' must be specified");

                if (string.IsNullOrEmpty(updateMemberProfileRequest.CustomerId))
                    updateMemberProfileRequest.CustomerId = (await _customerProvider.GetCustomer(updateMemberProfileRequest.CustomerId, updateMemberProfileRequest.MemberNumber)).Customer.CustomerId;

                if (string.IsNullOrEmpty(updateMemberProfileRequest.MemberNumber))
                    updateMemberProfileRequest.MemberNumber = (await _customerProvider.GetCustomer(updateMemberProfileRequest.CustomerId, updateMemberProfileRequest.MemberNumber)).Customer.LoyaltyNumber;

                var validationResult = new MemberProfileValidation().Validate(updateMemberProfileRequest);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Member Profile mutation request validation failure.", validationResult.Errors);

                var blacklistedEmailList = (await _referenceDataProvider.GetBlacklistedEmails()).First();
                var blacklistedEmails = Json.Deserialize<List<string>>(blacklistedEmailList.Emails);

                if (blacklistedEmails.Any(x => x.Equals(updateMemberProfileRequest.EmailId, StringComparison.OrdinalIgnoreCase)))
                {
                    var builder = ErrorBuilder.New().SetMessage("Email BlackListed");
                    builder.SetExtension("code", Application.BlackListedEmaildErrorCode);
                    builder.SetExtension("message", Application.BlackListedEmaildErrorMessage);
                    builder.SetExtension("source", Application.BlackListedEmaildErrordErrorsource);
                    throw new GraphQLException(builder.Build());
                }

                // Profile Update
                var profileUpdateResponse = await _memberProfileMutationProvider.UpdateMemberProfile((MemberProfileUpdateRequest)updateMemberProfileRequest);

                //Preference Update
                if (profileUpdateResponse == Application.ProfileUpdateMSSuccess && updateMemberProfileRequest.Preferences != null)
                {
                    var prefUpdateResponse = await _preferenceUpdateProvider.UpdateVACMemberPreference((MemberPreferenceUpdateRequest)updateMemberProfileRequest);

                    if (prefUpdateResponse.ErrorDetails != null && prefUpdateResponse.ErrorDetails.Any())
                    {
                        var builder = ErrorBuilder.New().SetMessage("Update Marketing Preference failed");
                        prefUpdateResponse.ErrorDetails.ToList().ForEach(error =>
                        {
                            builder.SetExtension("code", error.Code);
                            builder.SetExtension("message", error.Message);
                            builder.SetExtension("source", error.ErrorSource);
                        });

                        throw new GraphQLException(builder.Build());
                    }
                }

                return Application.ProfileUpdateGraphQlSuccess;

            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Unable to invoke Member Profile");
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogCritical(ex, "Unahandled Exception in member profile mutation");
                throw;
            }
        }
    }
}
