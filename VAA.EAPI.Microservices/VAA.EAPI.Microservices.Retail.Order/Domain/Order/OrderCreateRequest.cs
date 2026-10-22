using System;
using System.Collections.Generic;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Domain.Virgin.Models.Payment;

namespace VAA.EAPI.Microservices.Retail.Order.Domain.Order
{
    public class OrderCreateRequest
    { 
        public Pos Pos { get; set; }
        public IEnumerable<Offers> SelectedOffers { get; set; }
        public IEnumerable<PaymentDetails> PaymentDetails { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public bool CreditCardCheckConsent { get; set; }
        public InsuranceDetails InsuranceDetails { get; set; }
        public bool? IsInsuranceAdded { get; set; }
        public bool VoucherApplied { get; set; }
        public VoucherDetails? VoucherDetails { get; set; }
        public Dictionary<string, string> MetaPartner { get; set; } = [];
    }
}
