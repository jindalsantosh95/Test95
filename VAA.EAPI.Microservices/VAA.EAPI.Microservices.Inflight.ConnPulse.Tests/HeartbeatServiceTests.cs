using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using VAA.EAPI.Microservices.Inflight.ConnPulse.Services;
using Xunit;

namespace VAA.EAPI.Microservices.Inflight.ConnPulse.Tests
{
    public sealed class HeartbeatServiceTests
    {
        [Fact]
        public void IsTransient_ReturnsTrue_ForHttpRequestException()
        {
            var exception = new HttpRequestException("transport failure");

            var result = HeartbeatService.IsTransient(exception);

            Assert.True(result);
        }

        [Fact]
        public void IsTransient_ReturnsTrue_ForNestedConnectionResetSocketException()
        {
            var socketException = new SocketException((int)SocketError.ConnectionReset);
            var exception = new IOException("stream read failed", socketException);

            var result = HeartbeatService.IsTransient(exception);

            Assert.True(result);
        }

        [Fact]
        public void IsTransient_ReturnsFalse_ForNonTransientException()
        {
            var exception = new InvalidOperationException("bad request payload");

            var result = HeartbeatService.IsTransient(exception);

            Assert.False(result);
        }
    }
}
