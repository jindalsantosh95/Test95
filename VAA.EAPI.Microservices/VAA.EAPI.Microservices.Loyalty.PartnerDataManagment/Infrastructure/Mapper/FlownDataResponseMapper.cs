using System.Data;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.VSRetro;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Mapper
{
    public static class FlownDataResponseMapper
    {
        public static VSFlownDetailsResponse GetFlownDataFromDataTableResponse(DataTable flownData)
        {
            IEnumerable<VSRetroFlownDetail> VSRetroFlownDetails = flownData.AsEnumerable()
                    .Where(dataRow => string.Equals(Convert.ToString(dataRow["ClaimStatus"]), "FREE", StringComparison.OrdinalIgnoreCase))
                    .Select(dataRow => new VSRetroFlownDetail
                    {
                        MemberNumber = dataRow.GetString("MemberNumber"),
                        ArrivalAirport = dataRow.GetString("ArrivalAirport"),
                        DepartureAirport = dataRow.GetString("DepartureAirport"),
                        DepartureDate = dataRow.GetDateTimeValue("DepartureDate"),
                        OriginalBookedClass = dataRow.GetString("OriginalBookedClass"),
                        Id = dataRow.GetInt("Id"),
                        FlownUpgradedClass = dataRow.GetString("FlownUpgradedClass"),
                        OperatingAirlineCode = dataRow.GetString("OperatingAirlineCode"),
                        FareBasis = dataRow.GetString("FareBasis"),
                        TicketDesignator = dataRow.GetString("TicketDesignator"),
                        PassengerType = dataRow.GetString("PassengerType"),
                        FlownCabinClass = dataRow.GetString("FlownCabinClass"),
                        VAMarketingBookingClass = dataRow.GetString("VAMarketingBookingClass"),
                        MarketingAirlineCode = dataRow.GetString("MarketingAirlineCode"),
                        MarketingFlightNumber = dataRow.GetString("MarketingFlightNumber"),
                        OperatingFlightNumber = dataRow.GetString("OperatingFlightNumber"),
                        TicketNumber = dataRow.GetString("TicketNumber"),
                        OperatingAirlineAuthNum = dataRow.GetString("OperatingAirlineAuthNum"),
                        RecordLocatorOperatingCarrier = dataRow.GetString("RecordLocatorOperatingCarrier"),
                        TicketIssueDate = dataRow.GetDateTimeValue("TicketIssueDate"),
                        BaseFare = dataRow.GetString("BaseFare"),
                        NetFare = dataRow.GetString("NetFare"),
                        YQTax = dataRow.GetString("YQTax"),
                        BaseMiles = dataRow.GetString("BaseMiles"),
                        FirstName = dataRow.GetString("FirstName"),
                        LastName = dataRow.GetString("LastName"),
                        ClaimStatus = dataRow.GetString("ClaimStatus"),
                        SalesLocation = dataRow.Table.Columns.Contains("BookingChannel") ? dataRow.GetString("BookingChannel") : string.Empty,
                        IataAgentNumber = dataRow.Table.Columns.Contains("IataAgentNum") ? dataRow.GetString("IataAgentNum") : string.Empty
                    });

            return new VSFlownDetailsResponse(VSRetroFlownDetails);

        }

        public static VSFlownDetailsResponse GetRetroFlownDataFromDataTableResponse(DataTable flownData)
        {
            IEnumerable<VSRetroFlownDetail> VSRetroFlownDetails = flownData.AsEnumerable()
                    .Select(dataRow => new VSRetroFlownDetail
                    {
                        MemberNumber = dataRow.GetString("MemberNumber"),
                        ArrivalAirport = dataRow.GetString("DestinationAirport"),
                        DepartureAirport = dataRow.GetString("OriginationAirport"),
                        DepartureDate = dataRow.GetDateTimeValue("ActivityDate"),
                        OriginalBookedClass = dataRow.GetString("OperatingBookingClass"),
                        Id = dataRow.GetInt("TransactionIntegrationId"),
                        FlownUpgradedClass = dataRow.GetString("RevenueClass"),
                        OperatingAirlineCode = dataRow.GetString("OperatingCarrier"),
                        FareBasis = dataRow.GetString("FareBasis"),
                        TicketDesignator = dataRow.GetString("GDSRecordLocator"),
                        PassengerType = dataRow.GetString("VAPaxType"),
                        FlownCabinClass = dataRow.GetString("CabinClass"),
                        VAMarketingBookingClass = dataRow.GetString("VAMarketingBookingClass"),
                        MarketingAirlineCode = dataRow.GetString("VAMarketingCarrierCode"),
                        MarketingFlightNumber = dataRow.GetString("MarketingFlightNumber"),
                        OperatingFlightNumber = dataRow.GetString("OperatingFlightNumber"),
                        TicketNumber = dataRow.GetString("TicketNumber"),
                        RecordLocatorOperatingCarrier = dataRow.GetString("PNR"),                        
                        TicketIssueDate = dataRow.GetDateTimeValue("TicketIssueDate"),
                        BaseFare = dataRow.GetString("VABaseFare"),
                        NetFare = dataRow.GetString("VANetFare"),
                        YQTax = dataRow.GetString("TaxAmount"),
                        BaseMiles = dataRow.GetString("VATotalMiles"),
                        FirstName = dataRow.GetString("FirstName"),
                        LastName = dataRow.GetString("LastName"),
                        ClaimStatus = dataRow.GetString("ClaimStatus"),
                        SalesLocation = dataRow.Table.Columns.Contains("BookingChannel") ? dataRow.GetString("BookingChannel") : string.Empty,
                        IataAgentNumber = dataRow.Table.Columns.Contains("IataAgentNum") ? dataRow.GetString("IataAgentNum") : string.Empty,
                        VAOtherTax = dataRow.Table.Columns.Contains("VAOtherTax") ? dataRow.GetString("VAOtherTax") : string.Empty,
                        LoyaltyProgramme = dataRow.Table.Columns.Contains("LoyaltyProgramme") ? dataRow.GetString("LoyaltyProgramme") : string.Empty,
                        TicketType = dataRow.Table.Columns.Contains("TicketType") ? dataRow.GetString("TicketType") : string.Empty,
                        SentToPartnerFlag = dataRow.Table.Columns.Contains("Partner_sent_flag") ? dataRow.GetString("Partner_sent_flag") : string.Empty,
                        VADealCode = dataRow.Table.Columns.Contains("VADealCode") ? dataRow.GetString("VADealCode") : string.Empty
                    });

            return new VSFlownDetailsResponse(VSRetroFlownDetails);

        }
    }
}
