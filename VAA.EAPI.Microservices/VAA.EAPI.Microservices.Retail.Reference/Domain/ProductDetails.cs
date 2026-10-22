using System;
using VAA.EAPI.Microservices.Retail.Reference.Infrastructure.Repositories.SCVReferenceDB.Models;

namespace VAA.EAPI.Microservices.Retail.Reference.Domain
{
    public class ProductDetails
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

        public static explicit operator ProductDetails(Product product) => new()
        {
            RowId = product.RowId,
            ProductName = product.ProductName,
            Description = product.Description,
            ProductCategory = product.ProductCategory,
            ProductType = product.ProductType,
            DivisionCode = product.DivisionCode,
            AllowCancellation = product.AllowCancellation
        };
    }
}
