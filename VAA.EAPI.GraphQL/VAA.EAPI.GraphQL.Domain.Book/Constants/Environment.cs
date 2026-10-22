namespace VAA.EAPI.GraphQL.Domain.Book.Constants
{
    public class Environment
    {
        /* Book [Order]*/
        public static readonly string BookOrderCreateMSUrl = "BookOrder:CreateUrl";
        public static readonly string BookOrderOcpKey = "BookOrder:OcpSubID";

        /* Book [Offer]*/
        public static readonly string BookOffersSearchUrl = "Book:Offer:Url";
        public static readonly string BookOffersSearchOcpKey = "Book:Offer:OcpSubID";
        public static readonly string BookOfferMsChannelId = "Book:OfferMS:ChannelId";

        /* Book [Payment]*/
        public static readonly string PaymentMSUrl = "PaymentMS:Url";
        public static readonly string PaymentMSOcpKey = "Book:PaymentMS:OcpSubID";
        public static readonly string PaymentCaptureMSVendorId = "Book:PaymentCaptureMS:VendorId";


        /* Book [Basket]*/
        public static readonly string BasketMSUrl = "BasketMS:Url";
        public static readonly string BasketMSRetrieveUrl = "BasketMSRetrieveUrl";
        public static readonly string BasketMSUpdatePassengerUrl = "BasketMSUpdatePassengerUrl";
        public static readonly string BasketMSUpdatePaymentUrl = "BasketMSUpdatePaymentUrl";
        public static readonly string BasketMSUpdateContactUrl = "BasketMSUpdateContactUrl";
        public static readonly string BasketMSUpdateTermsUrl = "BasketMSUpdateTermsUrl";
        public static readonly string BasketMSDeleteUrl = "BasketMSDeleteUrl";
        public static readonly string BasketMSOcpKey = "BasketMSOcpKey";
        public static readonly string BasketMSOrderStatusUpdateUrl = "BasketMSOrderStatusUpdateUrl";
        public static readonly string BasketMSUpdateSeatOfferUrl = "BasketMSUpdateSeatOfferUrl";
        public static readonly string BasketMSSetFlightSummaryUrl = "BasketMSSetFlightSummaryUrl";
        public static readonly string DelayTimeInMS = "DelayTimeInMS";
        public static readonly string PollingCount = "PollingCount";
        public static readonly string BasketMSUpdateMembershipUrl = "BasketMSUpdateMembershipUrl";
        public static readonly string EnableAutoMatch = "EnableAutoMatch";

        /* Book [Reference]*/
        public static readonly string ReferenceMSOcpKey = "ReferenceMSOcpKey";
        public static readonly string ReferenceMSAirportDetailsUrl = "ReferenceMSAirportDetailsUrl";
        public static readonly string ReferenceMSAircraftDetailsUrl = "ReferenceMSAircraftDetailsUrl";
        public static readonly string ReferenceMSAirlineDetailsUrl = "ReferenceMSAirlineDetailsUrl";
        public static readonly string ReferenceMSCabinDetailsUrl = "ReferenceMSCabinDetailsUrl";
        public static readonly string ReferenceMSAir4CurrencyNumberUrl = "ReferenceMSAir4CurrencyNumberUrl";
        public static readonly string ReferenceMSCountriesUrl = "ReferenceMSCountriesUrl";

        /* Book [Seat]*/
        public static readonly string BookSeatMSOcpKey = "BookSeatMSOcpKey";
        public static readonly string BookSeatMSUrl = "BookSeatMSUrl";
        public static readonly string BookRetrieveSeatOfferUrl = "BookRetrieveSeatOfferUrl";
        public static readonly string BookUpdateSeatOfferMSUrl = "BookUpdateSeatOfferMSUrl";
        public static readonly string EnableSeatMockResponse = "EnableSeatMockResponse";

        /* Book [LOPA]*/
        public static readonly string LOPAMSOcpKey = "LOPAMSOcpKey";
        public static readonly string LOPAMSUrl = "LOPAMSUrl";

        public static readonly string SeatIntegrationFlag = "PRISeatIntegrationFlag";

        public static readonly string SeatMockResponse = "SeatMockResponse";

        public static readonly string EnableFlightSummaryMockResponse = "EnableFlightSummaryMockResponse";

        /* Book [VRed]*/
        public static readonly string BookVRedMSGetPointsUrl = "Book:VRedMSGetPoints:Url";
        public static readonly string BookVRedMSOcpKey = "Book:VRedMS:OcpKey";
        public static readonly string BookVRedMSAppId = "Book:VRedMS:AppId";

        /* Pay With Points Login */
        public static readonly string DisableRevenueLogin = "DisableRevenueLogin";
        public static readonly string EnableMockPointsCurrencyConverter = "EnableMockPointsCurrencyConverter";
        public static readonly string EnableEarnPointCalculator = "EnableEarnPointCalculator";
        public static readonly string PointsToCurrencyConverterComponent = "PointsToCurrencyConverterComponent";

        /* Book [Insurance]*/
        public static readonly string InsuranceMSOcpKey = "InsuranceMSOcpKey";
        public static readonly string InsuranceMSUrl = "InsuranceMSUrl";
        public static readonly string EnableMockInsuranceQuote = "EnableMockInsuranceQuote";

        /* Book [VA Account Enrolment]*/
        public static readonly string EnableVACEnrolment = "EnableVACEnrolment";
    }
}
