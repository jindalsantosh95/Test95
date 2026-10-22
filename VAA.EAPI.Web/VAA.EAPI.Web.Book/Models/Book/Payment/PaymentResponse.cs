using System.Collections.Generic;
using VAA.EAPI.Web.Book.Models.YourDetails.Response;
using VAA.EAPI.Web.Common.Helper;

namespace VAA.EAPI.Web.Book.Models.Book.Payment
{
    public class PaymentResponse
    {
        public string Type { get; set; }
        public IEnumerable<PaymentSession> PaymentSessions { get; set; }
        public MilesPayment MilesPayment { get; set; }
        public PaymentDetails PaymentDetails { get; set; }
        public VoucherPayment VoucherPayment { get; set; }
        public decimal TotalAmountOnSessions { get; set; }
        public decimal MinDeposit { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AmountToPay { get; set; }
        public DirectDebit DirectDebit { get; set; }
        public string Link { get; set; }
        public object ConfirmationLink { get; set; }
        public Summary Summary { get; set; }
        public string ResourceType { get; set; }

        public void UpdateLinks(string RequestLink = "", string ExpiryUrl = "", string ReturnUrl = "", bool isConfirmationLinkRequired = false)
        {
            Type = Constants.Application.Type;
            PaymentDetails = new()
            {
                ExpiryUrl = ExpiryUrl,
                Link = RequestLink,
                ReturnUrl = ReturnUrl,
            };
            Link = ConfigReader.Get(Constants.Environment.BffPaymentLink);
            ConfirmationLink = isConfirmationLinkRequired ? ConfigReader.Get(Constants.Environment.BffPaymentConfirmationLink) : null;
            ResourceType = Constants.Application.ResourceType;
        }

        public void UpdatePaymentSession()
        {
            foreach (var ps in PaymentSessions)
            {
                ps.PaymentAmount = new PaymentAmount()
                {
                    AmountToPay = AmountToPay,
                    Link = ConfigReader.Get(Constants.Environment.BffPaymentAmountLink),
                };
                ps.RemoveLink = ConfigReader.Get(Constants.Environment.BffPaymentLink);
                ps.Status = Constants.Application.PaymentStatus;
            }
        }
    }
}