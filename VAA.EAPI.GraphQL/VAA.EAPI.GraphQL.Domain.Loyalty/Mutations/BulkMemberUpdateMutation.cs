using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.BulkMemberUpdate;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class BulkMemberUpdateMutation
    {
        private readonly ILogger<BulkMemberUpdateMutation> _logger;
        private readonly IBulkMemberUpdateMutationProvider _bulkProvider;

        public BulkMemberUpdateMutation(IBulkMemberUpdateMutationProvider bulkProvider, ILogger<BulkMemberUpdateMutation> logger)
        {
            _bulkProvider = bulkProvider ?? throw new ArgumentNullException(nameof(bulkProvider));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        [Authorize(Roles = new[] { "Loyalty.Member.BulkUpdate" })]
        [GraphQLName("BulkMemberUpdate")]
        [GraphQLDescription("Initiate bulk member update from Blob file (status and/or notes). Returns immediate ack; check response file for status.")]
        public async Task<BulkMemberUpdateResponse> BulkMemberUpdate(BulkMemberUpdateRequest request)
        {
            try
            {
                if (request is null)
                    throw new RequestValidationException("Bulk member update request parameters must be specified");

                var validationResult = new BulkMemberUpdateValidator().Validate(request);

                if (!validationResult.IsValid)
                    throw new RequestValidationException("Bulk member update request validation failure.", validationResult.Errors);

                var response = await _bulkProvider.BulkUpdateWithFile(request);

                if (response.Errors is not null)
                {
                    var builder = ErrorBuilder.New().SetMessage("Bulk member update mutation request failed.");

                    foreach (var err in response.Errors)
                    {
                        builder.SetExtension("code", err.Code);
                        builder.SetExtension("message", err.Message);
                        builder.SetExtension("source", err.ErrorSource);
                    }

                    throw new GraphQLException(builder.Build());
                }

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in BulkMemberUpdate: {Message}", ex.Message);
                throw;
            }
        }
    }
}