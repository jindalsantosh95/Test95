using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.ValidationMS;
using VAA.EAPI.NDC.Offer.Validations;
using Xunit;

namespace VAA.EAPI.NDC.Offer.Tests.OrderReshop
{
    public class AgencyValidationsTests
    {
        [Fact]
        public void Validate_ErrorCodeFor_InvalidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = " "
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "911_22");
        }

        [Fact]
        public void Validate_ErrorCodeG009_ValidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "G009"
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "911_22");
        }

        [Fact]
        public void Validate_ErrorCode10144_ValidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "10144"
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "368_2");
        }

        [Fact]
        public void Validate_ErrorCode10146_ValidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "10146"
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "368_3");
        }

        [Fact]
        public void Validate_ErrorCode10147_ValidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "10147"
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "368_4");
        }

        [Fact]
        public void Validate_ErrorCode10145_ValidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "10145"
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "368_5");
        }

        [Fact]
        public void Validate_ErrorCode10140_ValidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "10140"
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "368_6");
        }

        [Fact]
        public void Validate_ErrorCode10152_ValidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "10152"
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "368_8");
        }

        [Fact]
        public void Validate_ErrorCode10153_ValidResponse()
        {
            var validationRS = new ValidationRS()
            {
                Error = new List<ErrorDetails>()
                    {
                        new ()
                        {
                            Code = "10153"
                        }
                    }
            };

            var validationErrors = RequestValidations.ValidateAgencyDetails(validationRS);
            Assert.True(validationErrors[0].ErrorCode == "911_44");
        }
    }
}
