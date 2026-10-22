using Newtonsoft.Json;
using System;
using System.Net.Http;
using VAA.EAPI.GraphQL.Domain.Loyalty.Models.MemberMerge;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions;

namespace VAA.EAPI.GraphQL.Domain.Loyalty.Models
{
    public class ContactNoteMutationRequest
    {
        [JsonIgnore]
        public string CustomerId { get; set; }
        [JsonIgnore]
        public HttpMethod RequestType { get; set; }
        public ContactNote ContactNote { get; set; }

        public static explicit operator ContactNoteMutationRequest(MemberMergeMutationRequest memberMergeRequest)
        {
            if (memberMergeRequest is null) throw new ArgumentNullException(nameof(memberMergeRequest));

            return new ContactNoteMutationRequest()
            {
                CustomerId = memberMergeRequest.SurvivorCustomerId,
                ContactNote = new()
                {
                    Group = Common.Constants.Application.ContactNoteGroup,
                    Type = Common.Constants.Application.ContactNoteType,
                    Text = Common.Constants.Application.ContactNoteText.Replace("{0}", memberMergeRequest.MergedMemberNumber),
                    CreatedBy = memberMergeRequest.InitiatedBy,
                    LastUpdatedBy = memberMergeRequest.InitiatedBy
                },
                RequestType = HttpMethod.Post
            };
        }
    }
}
