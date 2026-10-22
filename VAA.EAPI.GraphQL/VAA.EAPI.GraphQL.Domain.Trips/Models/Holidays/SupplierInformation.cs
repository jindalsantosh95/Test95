using VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.HolidaysMS;

namespace VAA.EAPI.GraphQL.Domain.Trips.Models.Holidays
{
    public class SupplierInformation
    {
        public string? Name { get; set; }
        public string? Code { get; set; }
        public string? ConfirmationNo { get; set; }

        public static explicit operator SupplierInformation(SupplierDetails source)
        {
            if (source == null) return null;

            return new SupplierInformation()
            {
                Name = source.SupplierPaymentInfo?.SupplierName,
                Code = source.SupplierPaymentInfo?.SupplierCode,
                ConfirmationNo = source.SupplierCommunicationInfo?.ConfirmationNo
            };
        }

        internal SupplierInformation Clone()
        {
            return new SupplierInformation
            {
                Name = this.Name,
                Code = this.Code,
                ConfirmationNo = this.ConfirmationNo
            };
        }
    }
}