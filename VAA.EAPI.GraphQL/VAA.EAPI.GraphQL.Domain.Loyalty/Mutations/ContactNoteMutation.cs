using HotChocolate;
using HotChocolate.AspNetCore.Authorization;
using HotChocolate.Types;
using Microsoft.Extensions.Logging;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using VAA.EAPI.GraphQL.Common.Exceptions;
using VAA.EAPI.GraphQL.Domain.Loyalty.Interfaces;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models;
using VAA.EAPI.GraphQL.Domain.Loyalty.Validators;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;
using VAA.EAPI.GraphQL.Infrastructure.Interfaces;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Mutations
{
    [ExtendObjectType("Mutation")]
    public class ContactNoteMutation
    {
        private readonly IContactNoteMutationProvider<ContactNoteMutationResponse> _notesProvider;
        private readonly ICustomerProvider _customerService;
        private readonly ILogger<ContactNoteMutation> _logger;

        public ContactNoteMutation(IContactNoteMutationProvider<ContactNoteMutationResponse> notesProvider, ICustomerProvider customerService, ILogger<ContactNoteMutation> logger)
        {
            _notesProvider = notesProvider ?? throw new ArgumentNullException(nameof(notesProvider));
            _customerService = customerService ?? throw new ArgumentNullException(nameof(customerService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        [Authorize(Roles = new[] { "ContactNotes.Create" })]
        [GraphQLName("loyaltyCreateNote")]
        [GraphQLDescription("Create a new contact note for the customer")]
        public async Task<ContactNote> CreateNote(string? customerId, string? memberId, ContactNote contactNote)
        {
            return await MutateNote(customerId, memberId, contactNote, HttpMethod.Post);
        }

        [Authorize(Roles = new[] { "ContactNotes.Update" })]
        [GraphQLName("loyaltyUpdateNote")]
        [GraphQLDescription("Update an existing contact note for the customer")]

        public async Task<ContactNote> UpdateNote(string? customerId, string? memberId, ContactNote contactNote)
        {
            return await MutateNote(customerId, memberId, contactNote, HttpMethod.Patch);
        }

        [Authorize(Roles = new[] { "ContactNotes.Delete" })]
        [GraphQLName("loyaltyDeleteNote")]
        [GraphQLDescription("Delete an existing contact note for the customer")]

        public async Task<ContactNote> DeleteNote(string? customerId, string? memberId, ContactNote contactNote)
        {
            return await MutateNote(customerId, memberId, contactNote, HttpMethod.Delete);
        }

        private async Task<ContactNote> MutateNote(string? customerId, string? memberId, ContactNote contactNote, HttpMethod verb)
        {
            if (customerId is null && memberId is null)
                throw new RequestValidationException($"Either '{nameof(customerId)}' or {nameof(memberId)} must be specified.");

            if (contactNote is null)
                throw new ArgumentNullException(nameof(contactNote));


            var validationResult = new ContactNoteValidator(verb).Validate(contactNote);
            if (!validationResult.IsValid)
                throw new RequestValidationException("Contact note mutation request validation failure.", validationResult.Errors);

            // If I don't have a customer ID, go get the member based on member ID
            if (string.IsNullOrEmpty(customerId))
                customerId = (await _customerService.GetCustomer(customerId, memberId)).Customer.CustomerId;

            ContactNoteMutationRequest req = new()
            {
                CustomerId = customerId,
                ContactNote = contactNote,
                RequestType = verb
            };

            var outcome = await _notesProvider.Mutate(req);

            // If there were any errors, then report them to consumer, 
            if (outcome.Errors is not null)
            {
                var builder = ErrorBuilder.New()
                                          .SetMessage("Contact note mutation request failed.");
                outcome.Errors.ForEach(err =>
                {
                    builder.SetExtension("code", err.Code);
                    builder.SetExtension("message", err.Message);
                    builder.SetExtension("source", err.ErrorSource);
                });


                throw new GraphQLException(builder.Build());
            }

            return outcome.ContactNote;
        }

    }
}
