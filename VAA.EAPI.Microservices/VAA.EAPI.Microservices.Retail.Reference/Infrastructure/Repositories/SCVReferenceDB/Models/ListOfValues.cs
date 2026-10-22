using System;

namespace VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models
{
    public partial class ListOfValues
    {
        public string RowId { get; set; }
        public string Parent { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string SubType { get; set; }
        public string Value { get; set; }
        public string AdditionalValue1 { get; set; }
        public string AdditionalValue2 { get; set; }
        public string Active { get; set; }
        public int? SortOrder { get; set; }
        public string Product { get; set; }
        public DateTime? CreatedOn { get; set; }
        public string CreatedBy { get; set; }
        public DateTime? LastUpdatedOn { get; set; }
        public string LastUpdatedBy { get; set; }
    }
}
