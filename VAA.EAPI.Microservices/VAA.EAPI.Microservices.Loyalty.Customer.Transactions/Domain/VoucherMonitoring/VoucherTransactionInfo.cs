using System;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.VoucherMonitoring
{
    public class VoucherTransactionInfo
    {
        public string TransactionId { get; set; }
        public string TransactionType { get; set; }
        public string TransactionStatus { get; set; }
        public string ProductName { get; set; }
        public string PNR { get; set; }
        public int Points { get; set; }

        public static explicit operator VoucherTransactionInfo((TransactionDocument txnData, string productName) source)
        {
            return new VoucherTransactionInfo()
            {
                TransactionId = source.txnData.TransactionId,
                TransactionType = source.txnData.TransactionType,
                TransactionStatus = source.txnData.StatusCode,
                ProductName = source.productName,
                PNR = source.txnData.FlightDetails?.ExternalReservationNumber,
                Points = source.txnData.TransactionValue.IsNullOrWhiteSpace() ? 0 : (int)Convert.ToDouble(source.txnData.TransactionValue)
            };
        }
    }
}
