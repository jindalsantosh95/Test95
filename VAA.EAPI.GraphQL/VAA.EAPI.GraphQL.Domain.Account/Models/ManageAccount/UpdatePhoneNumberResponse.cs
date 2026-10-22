using VAA.EAPI.GraphQL.Common.Entities;

namespace VAA.EAPI.GraphQL.Domain.Account.Models.ManageAccount
{
    public class UpdatePhoneNumberResponse
    {
        public bool PreferenceValue { get; set; }
        public IEnumerable<ErrorDetails> ErrorDetails { get; set; }
    }
}
