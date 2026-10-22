using System;

// Code scaffolded by EF Core assumes nullable reference types (NRTs) are not used or disabled.
// If you have enabled NRTs for your project, then un-comment the following line:
// #nullable disable

namespace VAA.EAPI.Microservices.Loyalty.Customer.Infrastructure.Repositories.Enrolment.CommonRefDB.Models
{
    public partial class ProgramSubTypes
    {
        public string RowId { get; set; }
        public string DefaultFlag { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public string ProgramId { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
    }
}
