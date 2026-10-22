using VAA.EAPI.Microservices.Loyalty.Audit.Domain.Entities;
using VAA.EAPI.Microservices.Loyalty.Audit.Validators;

namespace VAA.EAPI.Microservices.Loyalty.Audit.Tests
{
    public class LoyaltyGenericAuditCreateEventValidatorTest
    {
        private readonly LoyaltyGenericAuditCreateEventValidator _eventValidator;

        public LoyaltyGenericAuditCreateEventValidatorTest()
        {
            _eventValidator = new LoyaltyGenericAuditCreateEventValidator();
        }

        [Fact]
        public void Validate_ReturnsTrue_GivenValidLoyaltyGenericAuditCreateEventData()
        {
            LoyaltyGenericAudit LoyaltyGenericAuditDocument = new LoyaltyGenericAudit()
            {
                Id = "1",
                CreatedBy = "Test",
                CreatedOn = DateTime.UtcNow,
                CustomerId = "CustomerId",
                SourceIdentifier = "LOOPS API",
                AuditCategory = "Supplementary Silver",
                AuditSubCategory = "Linking",
                RelationshipType = "Supp Silver",
                AuditData = "1004854921",
                DataUpdatedBy = "Test",
                DataUpdatedOn = DateTime.UtcNow
            };
            Assert.True(_eventValidator.Validate(LoyaltyGenericAuditDocument).IsValid);
        }

        [Fact]
        public void Validate_ReturnsFalse_GivenInvalidLoyaltyGenericAuditCreateEventData()
        {
            LoyaltyGenericAudit LoyaltyGenericAuditDocument = new LoyaltyGenericAudit();
            Assert.False(_eventValidator.Validate(LoyaltyGenericAuditDocument).IsValid);
        }
    }
}
