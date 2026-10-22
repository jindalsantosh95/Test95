using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.Infrastructure.NDC.Sabre.Mapper.CreateSession;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.CreateSession;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;

namespace VAA.EAPI.Infrastructure.NDC.Sabre.Tests
{
    public class CreateSessionRSMapperTests
    {
        [Fact]
         public void SessionCreateRSMapper_ValidSessionCreateRQResponse_ReturnsBinarySecurityToken()
        {

            //Arrange
            var sessionCreateRQResponse = TestDataProvider.GetTestData<SessionCreateRQResponse>("SabreResponses\\SessionCreateRQResponse");

            //Act
            var response = CreateSessionRSMapper.SessionCreateRSMapper(sessionCreateRQResponse);

            //Assert
            Assert.NotNull(response);
            Assert.Equal(sessionCreateRQResponse.Security.BinarySecurityToken.Value, response);
        }

        [Fact]
        public void SessionCreateRSMapper_ReturnsNull()
        {

            //Arrange
            var sessionCreateRQResponse = TestDataProvider.GetTestData<SessionCreateRQResponse>("SabreResponses\\SessionCreateRQResponse");
            sessionCreateRQResponse.SessionCreateRS = null;

            //Act
            var response = CreateSessionRSMapper.SessionCreateRSMapper(sessionCreateRQResponse);

            //Assert
            Assert.Null(response);
        }
    }
}
