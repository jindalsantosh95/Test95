using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.IdentityMS
{
    public class AccountContactDetails
    {
        public IEnumerable<AccountEmailDetail> EmailDetails { get; set; }
        public IEnumerable<AccountPhoneDetail> PhoneDetails { get; set; }
    }
}
