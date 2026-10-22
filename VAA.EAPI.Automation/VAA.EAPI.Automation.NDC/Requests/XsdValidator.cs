namespace VAA.EAPI.Automation.NDC.Requests
{
    public static class XsdValidator
    {
        /// <summary>
        /// Method to support the schema validations. We should have a mehod per endpoint passing in the schemas required for each endpoint
        /// </summary>
        /// <param name="xml"></param>
        /// <param name="exceptions"></param>
        /// <returns></returns>
        public static bool ValidateAirShoppingRequest(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=.\Tests\Xsds\IATA\IATA_AirShoppingRQ.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }

        public static bool ValidateAirShoppingResponse(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=.\Tests\Xsds\IATA\IATA_AirShoppingRS.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }

        public static bool ValidateSeatAvailabilityRequest(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=.\Tests\Xsds\IATA\IATA_SeatAvailabilityRQ.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }

        public static bool ValidateSeatAvailabilityResponse(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=.\Tests\Xsds\IATA\IATA_SeatAvailabilityRS.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }

        public static bool ValidateOrderCreateRequest(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=~\Tests\Xsds\IATA\IATA_OrderCreateRQ.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }

        public static bool ValidateOfferPriceRequest(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=.\Tests\Xsds\IATA\IATA_OfferPriceRQ.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }

        public static bool ValidateOfferPriceResponse(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=.\Tests\Xsds\IATA\IATA_OfferPriceRS.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }

        public static bool ValidateOrderRetrieveRequest(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=.\Tests\Xsds\IATA\IATA_OrderRetrieveRQ.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }

        public static bool ValidateOrderRetrieveResponse(string xml, out string[] exceptions)
        {
            var schemas = new[]
            {
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersMessage=.\Tests\Xsds\IATA\IATA_OrderViewRS.xsd",
                @"http://www.iata.org/IATA/2015/EASD/00/IATA_OffersAndOrdersCommonTypes=.\Tests\Xsds\IATA\IATA_OffersAndOrdersCommonTypes.xsd",
                @"http://www.w3.org/2000/09/xmldsig#=.\Tests\Xsds\IATA\xmldsig-core-schema.xsd"
            };

            //Checking the request structure against the schema
            return VAA.EAPI.Automation.Core.Helpers.SchemaValidator.XsdSchemaValidator(xml, schemas, out exceptions);
        }
    }
}
