using System;
using System.Runtime.CompilerServices;
using VAA.EAPI.Common.Helpers;
using PaymentEnvironment = VAA.EAPI.Microservices.Retail.Order.Payment.Constants.Environment;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Tests
{
    internal static class TestEnvironment
    {
        [ModuleInitializer]
        internal static void Initialize()
        {
            ConfigReader.Set(PaymentEnvironment.CjsCheckoutPaymentAppUrl,
                global::System.Environment.GetEnvironmentVariable("CjsCheckoutPaymentAppUrl")
                ?? "https://stg.virginatlantic.com/cjs-checkout-api/v2/payment/app");

            ConfigReader.Set(PaymentEnvironment.CjsCheckoutPaymentAppVersion,
                global::System.Environment.GetEnvironmentVariable("CjsCheckoutPaymentAppVersion") ?? "1.0.0");

            ConfigReader.Set(PaymentEnvironment.MmbQueryLoginUrl,
                global::System.Environment.GetEnvironmentVariable("MmbQueryLoginUrl")
                ?? "https://vsolpublic-uat.webdev.vholsinternal.co.uk/mmb-query-api/login");

            // Ensure unit tests do not use the optional mock-service proxy unless explicitly enabled elsewhere.
            ConfigReader.Set("MockDelegatingHandler:Enabled",
                global::System.Environment.GetEnvironmentVariable("MockDelegatingHandler:Enabled") ?? bool.FalseString);
        }
    }
}
