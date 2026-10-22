using System.Collections.Generic;

namespace VAA.EAPI.Domain.Virgin.Models.Products.Brand
{
    public class BrandProduct
    {
        public string ShortDesc { get; set; }
        public string LongDesc { get; set; }
        public List<Product> Product { get; set; }
        public string Id { get; set; }
        public string TypeCode { get; set; }
        public string SubTypeCode { get; set; }
        public string Category { get; set; }
        public int Priority { get; set; }
    }
}
