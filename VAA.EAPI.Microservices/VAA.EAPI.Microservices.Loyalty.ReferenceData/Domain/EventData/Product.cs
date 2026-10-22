using System;

namespace VAA.EAPI.Microservices.Loyalty.ReferenceData.Domain.EventData
{
    public class Product
    {
        public string ProductId { get; set; }
        public string ProductName { get; set; }
        public string ProductType { get; set; }
        public string ProductDescription { get; set; }
        public string ProductCategory { get; set; }
        public string CategoryCode { get; set; }

        public static explicit operator Product(ProductDetails product)
        {
            if (product == null) throw new ArgumentNullException(nameof(product));

            return new Product()
            {
                ProductId = product.RowId,
                ProductName = product.ProductName,
                ProductType = product.ProductType,
                ProductDescription = product.Description,
                ProductCategory = product.ProductCategory,
                CategoryCode = product.CategoryCode
            };
        }
    }
}
