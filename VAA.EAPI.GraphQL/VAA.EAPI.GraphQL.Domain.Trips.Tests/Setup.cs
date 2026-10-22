using VAA.EAPI.GraphQL.Common.Helper;

namespace VAA.EAPI.GraphQL.Domain.Trips.Tests
{
    public static class Setup
    {
        public static void CreateTestEnvironmentSettings()
        {
            Dictionary<string, string> settings = new()
            {
                {"TRIPMS:BookingIdJwtKey", Guid.NewGuid().ToString()},
                {"Flightservicems:MSUrl","https://Testresources.virginatlantic.com/dev/flightservicesmicroservice"},
                {"FlightServiceMS:OcpSubID", "refkey"},
                {"Reference:MsUrl", "https://Testresources.virginatlantic.com/dev/ReferenceMicroservice/"},
                {"ReferenceMSOcpKey", "refkey"},
                {"TRIPMS:MSUrl","https://Testresources.virginatlantic.com/dev/tripmicroservice"},
                {"tripms:ocpsubid","refkey"},
                {"CheckIn:MsURL","https://Testresources.virginatlantic.com/dev/checkinmicroservice"},
                {"CheckIn:OcpSubID","refkey"},
                {"PaymentMS:Url", "https://Testresources.virginatlantic.com/dev/orderpaymentmicroservice/order/payment/" },
                {"Book:PaymentMS:OcpSubID", "refocpId" },
                {"BasketMS:Url", "https://testresources.virginatlantic.com/dev/basketmicroservice/basket/" },
                {"BasketMSOcpKey", "refocpId" },
                {"BookRetrieveSeatOfferUrl", "https://testresources.virginatlantic.com/dev/SeatMicroservice/seatoffer/{seatOfferId}" },
                {"bookseatmsocpkey", "refocpId" },
                {"bookupdateseatoffermsurl", "https://Testresources.virginatlantic.com/DEV/SeatMicroservice/seatOffer/seatUpdate" },
                {"lopamsurl", "https://Testresources.virginatlantic.com/Dev/lopaMicroservice/{fleetTypeCode}/{equipmentCode}/Lopa"},
                {"LOPAMSOcpKey", "refocpid"},
                {"seat:msurl", "https://testresources.virginatlantic.com/DEV/SeatMicroService"},
                {"ContentService:Url", "https://Testresources.virginatlantic.com/DEV/content/"},
                {"ContentService:Ocp", "refocpid"},
                {"OrderData:MSUrl","https://Testresources.virginatlantic.com/dev/orderdata" },
                {"OrderData:OcpSubID","refocpid"},
                {"TripSettings:FlightOpsUrl","http://tempURL.org" },
                {"TripSettings:FlightOpsOcpSubID","refocpid"},
                {"TripSettings:AircraftDetailsFeature","true"},
                {"TripSettings:PopsOpenDaysBeforeDeparture", "7"},
                {"TripSettings:PopsCloseDaysBeforeDeparture", "1"},
                {"MyAccount:IdentityMSURLForLoginTokenValidation","https://Testresources.virginatlantic.com/dev/identitymicroservice/ValidateAuthToken"},
                {"IdentityMS:AppId","testAppid"},
                {"OrderDataProvider:MsUrl","https://Testresources.virginatlantic.com/dev/orderdata"},
                {"identity:ocpsubid","ocpkeyvalue" },
                {"eapigraph:appid","testappids" },
                {"eapigraph:clientsecret","testsecret" },
                {"aad:authority","https://login.microsoftonline.com/" },
                {"aad:tenantid","00000000-4f89-11d3-9a0c-0305e82c3301" },
                {"orderdataprovider:ocpkey","testkey"},
                {"tripsettings:holidayprovider:holidaysms:ocpsubid", "testkey" },
                {"tripsettings:holidayprovider:holidaysms:url", "https://testresources.virginatlantic.com/dev/digital/holidays" },
                { "TripSettings:AllowedDocumentTypes","CBKINV,VATINVOICE,ITINERARYDOCUMENT,BKGCONFEMAIL,ATOLCERTIFICATE,ATOLRECEIPT,TRVDOC,TRVDOCEML,VIAVOUEMAIL"},
                { "TripSettings:HiddenElementGroups","FCHIDDEN,OWA,OWAWC,OWAEC,RCD,RETAILBOOK,ATOLLEVY,ADVANCE,LOWDEP,CCBF,STBF,BOOKFEE,HREPFEES"},
                {"TripSettings:HolidayMaxPaymentAmountInGBP", "50000" },
                {"TripSettings:PaymentProvider:HolidayPayment:Url", "https://Testresources.virginatlantic.com/dev/orderpaymentmicroservice" },
                {"TripSettings:PaymentProvider:HolidayPayment:OcpKey", "dummyocpkey" },
                { "TripSettings:KoreanAirlineOperatedHelpText", "If you'd like to choose or request a seat or a meal, our Virgin Atlantic Customer Centre will be happy to help." },
                { "TripSettings:OtherAirlineOperatedHelpText", "For full support to view and update your information, please contact the operating airline directly."}

            };

            foreach (var item in settings)
                ConfigReader.Set(item.Key, item.Value);
        }
    }
}
