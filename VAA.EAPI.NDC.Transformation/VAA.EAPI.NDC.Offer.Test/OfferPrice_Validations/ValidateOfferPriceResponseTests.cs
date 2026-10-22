using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using System.Threading.Tasks;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OfferMS;
using VAA.EAPI.NDC.Offer.Validations.OfferPrice;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;

namespace VAA.EAPI.NDC.Offer.Tests.OfferPrice_Validations
{
    public class ValidateOfferPriceResponseTests
    {
        // Test case 1: Offer is null
        [Fact]
        public void ValidateOfferPriceResponse_OfferIsNull_ReturnsEmptyList()
        {
            // Arrange
            OfferResponse offer = null;

            // Act
            var result = ValidateOfferPrice.ValidateOfferPriceResponse(offer);

            // Assert
            Assert.Empty(result);
        }

        // Test case 2: Error is not null, contains INTERNALSERVERERROR
        [Fact]
        public void ValidateOfferPriceResponse_TechError_ReturnsServiceTechError()
        {
            // Arrange
           
            var offer = new OfferResponse
            {
                Error = new List<ErrorDetails>
            {
                new ErrorDetails { Code = "INTERNALSERVERERROR" }
            }
            };

            // Act
            var result = ValidateOfferPrice.ValidateOfferPriceResponse(offer);

            // Assert
            Assert.Single(result);
            Assert.Equal("Service_TechError", result[0].ErrorCode);
        }

        // Test case 3: Error is not null, contains NDCOFFEREXPIREDERROR
        [Fact]
        public void ValidateOfferPriceResponse_OfferExpiredError_ReturnsExpiredError()
        {
            // Arrange
            var offer = new OfferResponse
            {
                Error = new List<ErrorDetails>
            {
                new ErrorDetails { Code = "10118" }
            }
            };

            // Act
            var result = ValidateOfferPrice.ValidateOfferPriceResponse(offer);

            // Assert
            Assert.Single(result);
            Assert.Equal(Constants.Application.NDCOFFEREXPIREDERROR, result[0].ErrorCode);
        }

        //Test Case 4 : Offers is not null, but no offer is selected
        [Fact]
        public void ValidateOfferPriceResponse_NoOfferSelected_ReturnsError9132()
        {
            // Arrange
            var offer = new OfferResponse
            {
                Offers = new List<OfferDetails>
            {
                new OfferDetails { Selected = false },
                new OfferDetails { Selected = false }
            }
            };

            // Act
            var result = ValidateOfferPrice.ValidateOfferPriceResponse(offer);

            // Assert
            Assert.Single(result);
            Assert.Equal("913_2", result[0].ErrorCode);
        }
    }

}
