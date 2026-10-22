using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference
{
    public class Product
    {
        public string ProductCategory { get; set; }
        public string RecordLocator { get; set; }
        public string ProductType { get; set; }

        public static explicit operator Product(PaymentRefIdGenerateRequest source)
        {
            if (source == null)
                return null;
            Product product = new Product();
            if(!source.Selectedoffers.IsNullOrEmpty())
            {
                product.ProductCategory = source.Selectedoffers?.First()?.ProductType;
                product.RecordLocator = Constants.Application.RecordLocator;
                product.ProductType = null;
            }
            else if(!source.SeatDetails.IsNullOrEmpty())
            {
                product.ProductCategory = "Flight"; // E2E working with only Flight as ProductType
                product.ProductType = null;
                product.RecordLocator = Constants.Application.RecordLocator; // Assuming record locator is still needed but will look for if needed to pass PNR.
            }
            else if (!source.BagsDetails.IsNullOrEmpty())
            {
                product.ProductCategory = "ANCILLARY"; 
                product.ProductType = null;
                product.RecordLocator = source?.RecordLocator ?? Constants.Application.RecordLocator;
            }
            return product;
        }
    }
}