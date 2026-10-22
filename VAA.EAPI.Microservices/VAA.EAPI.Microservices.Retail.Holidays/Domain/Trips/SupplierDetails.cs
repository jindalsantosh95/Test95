using Newtonsoft.Json.Linq;
namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class SupplierDetails
    {
        public SupplierPaymentInfo? SupplierPaymentInfo { get; set; }
        public SupplierCommunicationInfo? SupplierCommunicationInfo { get; set; }

        public static explicit operator SupplierDetails?(JToken? source)
        {
            if (source is null) return null;

            return new()
            {
                SupplierPaymentInfo = (SupplierPaymentInfo?)source["supplierPaymentInfo"],
                SupplierCommunicationInfo = (SupplierCommunicationInfo?)source["supplierCommunicationInfo"]
            };
        }
    }
}