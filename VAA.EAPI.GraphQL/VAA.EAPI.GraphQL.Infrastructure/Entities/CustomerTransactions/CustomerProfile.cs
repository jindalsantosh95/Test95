using System;
using System.Collections.Generic;

namespace VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions
{
    public class CustomerProfile
    {
        public string Id { get; set; }
        public string CustomerId { get; set; }
        public IEnumerable<Preference> Preferences { get; set; }
        public string ReasonForTravel { get; set; }
        public string Brand { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string SourceSystem { get; set; }
        public string SourceIdentifier { get; set; }
        public string SchemaVersion { get; set; }
    }
}
