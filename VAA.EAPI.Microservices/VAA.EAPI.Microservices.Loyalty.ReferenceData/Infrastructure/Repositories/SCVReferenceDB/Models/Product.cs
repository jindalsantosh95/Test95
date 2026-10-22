using System;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Infrastructure.Repositories.SCVReferenceDB.Models
{
    public class Product
    {
        public string RowId { get; set; }
        public string ProductCategory { get; set; }
        public string Description { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string DivisionCode { get; set; }
        public string AllowCancellation { get; set; }
        public string SourceIdentifier { get; set; }
        public string SourceSystem { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
        public string CategoryCode { get; set; }

    }
}
