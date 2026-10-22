using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using Xunit;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class MapExternalErrorToNDCErrorTests
    {
        // Test case 1: External error exists in the error map
        [Fact]
        public void MapExternalErrorToNDCError_ExistingError_ReturnsMappedError()
        {
            // Arrange
            string externalError = "INTERNALSERVERERROR";  
            string expectedNDCError = "TECHNICAL";

            // Act
            string actualNDCError = ValidateOfferPrice.MapExternalErrorToNDCError(externalError);

            // Assert
            Assert.Equal(expectedNDCError, actualNDCError);
        }

        // Test case 2: External error does not exist in the error map
        [Fact]
        public void MapExternalErrorToNDCError_NonExistingError_ReturnsDefaultError()
        {
            // Arrange
            string externalError = "NonExistingError";
            string expectedNDCError = Constants.Application.NDCTECHNICALERROR;

            // Act
            string actualNDCError = ValidateOfferPrice.MapExternalErrorToNDCError(externalError);

            // Assert
            Assert.Equal(expectedNDCError, actualNDCError);
        }

      
        // Test case 3: External error is Empty String
        [Fact]
        public void MapExternalErrorToNDCError_EmptyString_ReturnsDefaultError()
        {
            // Arrange
            string externalError = ""; 
            string expectedNDCError = Constants.Application.NDCTECHNICALERROR;

            // Act
            string actualNDCError = ValidateOfferPrice.MapExternalErrorToNDCError(externalError);

            // Assert
            Assert.Equal(expectedNDCError, actualNDCError);
        }
    }
}
