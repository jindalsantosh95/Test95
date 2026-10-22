using Moq;
using System;
using System.ServiceModel;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Infrastructure.PSS.Services;
using VAA.EAPI.Infrastructure.WcfServiceMessaging;
using Xunit;

namespace VAA.EAPI.Infrastructure.PSS.Tests
{
    public class PNRCommonV6Tests
    {
        private readonly Mock<PnrCommonClient> _client;

        public PNRCommonV6Tests()
        {
            _client = new Mock<PnrCommonClient>(WcfCustomBinding.Binding(true), new EndpointAddress(new Uri("https://webservices.dummy")));
        }

        [Fact]
        public void NullExceptionTest()
        {
            Assert.Throws<ArgumentNullException>(() =>
           new PNRCommonV6(null));
        }

    }
}
