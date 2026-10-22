using Newtonsoft.Json.Linq;
namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class SupplierPaymentInfo
    {
        public int? SupplierId { get; set; }
        public string? SupplierName { get; set; }
        public string? SupplierCode { get; set; }

        public static explicit operator SupplierPaymentInfo?(JToken? source)
        {
            if (source is null) return null;

            return new()
            {
                SupplierId = source.Value<int>("supplierId"),
                SupplierCode = source.Value<string>("supplierCode"),
                SupplierName = source.Value<string>("supplierName")
            };
        }
    }
}