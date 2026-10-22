using System;
using static VAA.EAPI.Microservices.Framework.Tests.TestHelpers;
using VAA.EAPI.Infrastructure.Helper;
using Xunit;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Transaction;
using VAA.EAPI.Microservices.Loyalty.Vred.Mapper;
using VAA.EAPI.Microservices.Loyalty.Vred.Domain.Enum;

namespace VAA.EAPI.Microservices.Loyalty.Vred.Tests
{
    public class CancelTransactionEventMapperTests 
    {
        public CancelTransactionEventMapperTests()
        {
            VRedCommon.CreateTestEnvironmentSettings();
        }

        [Fact]
        public void CancelTransactionEventMapperTest_Success()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionRequest>("VRedMS\\Transaction\\CancelTransactionRequest");
            
            var cloudEvent = CancelTransactionEventMapper.CancelTransactionCloudEventMapper(request, "ab4567788");
            
            Assert.Equal("ab4567788", cloudEvent.Id);
            Assert.Equal(EnumOperations.GetEnumDescription((VredEventType)Enum.Parse(typeof(VredEventType), VredEventType.CancelTransactionEvent.ToString())), cloudEvent.Type);
            Assert.Equal(Constants.Application.DataVersion, cloudEvent.Specversion);
        }

        [Fact]
        public void CancelTransactionEventMapperTest_Failure()
        {
            var request = TestDataProvider.GetTestData<CancelTransactionRequest>("VRedMS\\Transaction\\CancelTransactionRequest");
            
            var cloudEvent = CancelTransactionEventMapper.CancelTransactionCloudEventMapper(request, "ab4567788");
            
            Assert.NotEqual("a4567788", cloudEvent.Id);
        }
    }
}
