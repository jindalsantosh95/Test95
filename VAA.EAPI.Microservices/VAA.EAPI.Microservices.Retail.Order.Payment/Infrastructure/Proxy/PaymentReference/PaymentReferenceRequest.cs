using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference
{
    public class PaymentReferenceRequest
    {
        public AirlineData AirlineData { get; set; }
        public string CurrencyCode { get; set; }
        public EcommAdditionalData EcommAdditionalData { get; set; }
        public FlightFare FlightFare { get; set; }
        public PaymentAmt PaymentAmt { get; set; }
        public AdditionalConfigParams AdditionalConfigParams { get; set; }
    }
}