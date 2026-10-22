using System;
using VAA.EAPI.Microservices.Retail.Order.Data.Infrastructure.Helper;
using Xunit;

namespace VAA.EAPI.Microservices.Retail.Order.Data.Tests
{
    public class PaymentHelperTest
    {
        [Fact]
        public void PaymentHelper_ShouldSucceed_WithGoodRequest()
        {
            var result = PaymentHelper.GeneratePaymentId(DateTime.UtcNow);


            Assert.NotNull(result);

        }

    }
}
