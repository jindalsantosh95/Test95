using System.Collections.Generic;
using System.Xml.Serialization;

namespace VAA.EAPI.Microservices.CRM.Email.Domain
{
    public class EmailTemplate
    {
        public class ctx
        {
            [XmlElement("SendMessage")]
            public SendMessage SendMessage { get; set; }
        }

        public class SendMessage
        {
            [XmlElement("ProcessingMetadata")]
            public ProcessingMetadata ProcessingMetadata { get; set; }

            [XmlElement("Addressee")]
            public Addressee Addressee { get; set; }

            [XmlElement("MessageData")]
            public MessageData MessageData { get; set; }
        }

        public class ProcessingMetadata
        {
            [XmlElement("EmailAddress")]
            public string EmailAddress { get; set; }

            [XmlElement("CcAddresses")]
            public CcAddresses CcAddresses { get; set; }

            [XmlElement("FromAddress")]
            public string FromAddress { get; set; }

            [XmlElement("BounceAddress")]
            public string BounceAddress { get; set; }

            [XmlElement("SmsNumber")]
            public string SmsNumber { get; set; }

            [XmlElement("Channel")]
            public string Channel { get; set; }

            [XmlElement("LanguageCode")]
            public string LanguageCode { get; set; }

            [XmlElement("Template")]
            public string Template { get; set; }
        }

        public class CcAddresses
        {
            [XmlElement("EmailAddress")]
            public List<string> EmailAddress { get; set; }
        }

        public class Addressee
        {
            [XmlElement("CustomerId")]
            public string CustomerId { get; set; }

            [XmlElement("FirstName")]
            public string FirstName { get; set; }

            [XmlElement("LastName")]
            public string LastName { get; set; }
        }

        public class MessageData
        {
            [XmlElement("Wrapper")]
            public Wrapper Wrapper { get; set; }

            [XmlElement("FlyingClubDetails")]
            public FlyingClubDetails FlyingClubDetails { get; set; }
        }

        public class Wrapper
        {
            [XmlElement("document")]
            public List<document> document { get; set; }
        }

        public class document
        {
            [XmlElement("request")]
            public request request { get; set; }
        }

        public class request
        {
            [XmlElement("template_id")]
            public string template_id { get; set; }

            [XmlElement("email_address")]
            public string email_address { get; set; }

            [XmlElement("test")]
            public string test { get; set; }

            [XmlElement("MAILING_ID")]
            public MAILING_ID MAILING_ID { get; set; }

            [XmlElement("TRANSACTION_TIME")]
            public string TRANSACTION_TIME { get; set; }

            [XmlElement("CONTENT_DATA")]
            public CONTENT_DATA CONTENT_DATA { get; set; }
        }

        public class MAILING_ID
        {
            [XmlElement("DATE_SENT")]
            public string DATE_SENT { get; set; }

            [XmlElement("CHANNEL")]
            public string CHANNEL { get; set; }

            [XmlElement("LANGUAGE")]
            public string LANGUAGE { get; set; }

            [XmlElement("DESCRIPTIVE")]
            public string DESCRIPTIVE { get; set; }
        }

        public class CONTENT_DATA
        {
            [XmlElement("SUBJECT_LINE")]
            public string SUBJECT_LINE { get; set; }

            [XmlElement("BARCODE")]
            public string BARCODE { get; set; }

            [XmlElement("CTRY_CD")]
            public string CTRY_CD { get; set; }

            [XmlElement("NBR_PSNGRS")]
            public string NBR_PSNGRS { get; set; }

            [XmlElement("PSNGR_NM")]
            public string PSNGR_NM { get; set; }

            [XmlElement("ITINERARY")]
            public ITINERARY ITINERARY { get; set; }

            [XmlElement("FARE")]
            public FARE FARE { get; set; }

            [XmlElement("AWARD_MILEAGE")]
            public string AWARD_MILEAGE { get; set; }

            [XmlElement("TAXES_CHARGES")]
            public TAXES_CHARGES TAXES_CHARGES { get; set; }

            [XmlElement("TICKET_DTL")]
            public TICKET_DTL TICKET_DTL { get; set; }

            [XmlElement("EFEE")]
            public List<EFEE> EFEE { get; set; }

            [XmlElement("TRIP_BAG_FEES")]
            public List<TRIP_BAG_FEES> TRIP_BAG_FEES { get; set; }

            [XmlElement("PROMOTIONS")]
            public string PROMOTIONS { get; set; }

            [XmlElement("PTC_ATP_PAX_CD")]
            public string PTC_ATP_PAX_CD { get; set; }

            [XmlElement("PTC_DL_PAX_TXT")]
            public string PTC_DL_PAX_TXT { get; set; }

            [XmlElement("IEMD_INDICATOR")]
            public string IEMD_INDICATOR { get; set; }
        }
        public class EFEE
        {
            [XmlElement("PSGR_NM")]
            public string PSGR_NM { get; set; }

            [XmlElement("DOC_NBR")]
            public string DOC_NBR { get; set; }

            [XmlElement("DOC_EXP_DTE")]
            public string DOC_EXP_DTE { get; set; }

            [XmlElement("ISSUE_DTE")]
            public string ISSUE_DTE { get; set; }

            [XmlElement("TRANSACTION")]
            public TRANSACTION TRANSACTION { get; set; }

            [XmlElement("TRANS_TAX_AMT")]
            public string TRANS_TAX_AMT { get; set; }

            [XmlElement("TRANS_TAX_CD")]
            public string TRANS_TAX_CD { get; set; }

            [XmlElement("TOTAL_AMT")]
            public string TOTAL_AMT { get; set; }

            [XmlElement("TOTAL_CUR_CD")]
            public string TOTAL_CUR_CD { get; set; }

            [XmlElement("EQUIV_AMT")]
            public string EQUIV_AMT { get; set; }

            [XmlElement("EQUIV_CUR_CD")]
            public string EQUIV_CUR_CD { get; set; }

            [XmlElement("FOP")]
            public string FOP { get; set; }

            [XmlElement("FOP_VENDOR")]
            public string FOP_VENDOR { get; set; }

            [XmlElement("FOP_CC_NBR")]
            public string FOP_CC_NBR { get; set; }

            [XmlElement("ROUTING_FEE")]
            public string ROUTING_FEE { get; set; }

            [XmlElement("ENDORSEMENT_LINE")]
            public ENDORSEMENT_LINE ENDORSEMENT_LINE { get; set; }

            [XmlElement("EMD_TYPE")]
            public string EMD_TYPE { get; set; }

            [XmlElement("RFIC_CODE")]
            public string RFIC_CODE { get; set; }
        }

        public class TRANSACTION
        {
            [XmlElement("TRANS_DESC")]
            public string TRANS_DESC { get; set; }

            [XmlElement("TRANS_AMT")]
            public string TRANS_AMT { get; set; }

            [XmlElement("TRANS_CUR_CD")]
            public string TRANS_CUR_CD { get; set; }

            [XmlElement("TRANS_DOC_NBR")]
            public string TRANS_DOC_NBR { get; set; }

            [XmlElement("SEGMENT")]
            public string SEGMENT { get; set; }
        }

        public class ENDORSEMENT_LINE
        {
            [XmlElement("ENDORSEMENT")]
            public string ENDORSEMENT { get; set; }
        }

        public class ITINERARY
        {
            [XmlElement("DL_REC_LOC")]
            public string DL_REC_LOC { get; set; }

            [XmlElement("TCKT_NBR")]
            public string TCKT_NBR { get; set; }

            [XmlElement("ARRIVE_IND")]
            public string ARRIVE_IND { get; set; }

            [XmlElement("NEST")]
            public List<NEST> NEST { get; set; }

            [XmlElement("PSNGR_INFO")]
            public PSNGR_INFO PSNGR_INFO { get; set; }

            [XmlElement("BRAZIL_DEP_IND")]
            public string BRAZIL_DEP_IND { get; set; }

            [XmlElement("OA_IND")]
            public string OA_IND { get; set; }

            [XmlElement("LGA_IND")]
            public string LGA_IND { get; set; }

            [XmlElement("ITINERARY_ORIGIN")]
            public string ITINERARY_ORIGIN { get; set; }

            [XmlElement("ITINERARY_DESTINATION")]
            public string ITINERARY_DESTINATION { get; set; }

            [XmlElement("NEXT_TRIP_ORIGIN")]
            public string NEXT_TRIP_ORIGIN { get; set; }

            [XmlElement("NEXT_TRIP_DESTINATION")]
            public string NEXT_TRIP_DESTINATION { get; set; }

            [XmlElement("ITINERARY_ROUNDTRIP")]
            public string ITINERARY_ROUNDTRIP { get; set; }

            [XmlElement("TTLDate")]
            public string TTLDate { get; set; }

            [XmlElement("TTLTime")]
            public string TTLTime { get; set; }

        }

        public class NEST
        {
            [XmlElement("DEPART_DAY")]
            public string DEPART_DAY { get; set; }

            [XmlElement("DEPART_DATE")]
            public string DEPART_DATE { get; set; }

            [XmlElement("SEGMENT")]
            public SEGMENT SEGMENT { get; set; }
        }

        public class SEGMENT
        {
            [XmlElement("SEGMENT_NO")]
            public string SEGMENT_NO { get; set; }

            [XmlElement("DEPART_DAY")]
            public string DEPART_DAY { get; set; }

            [XmlElement("DEPART_DATE")]
            public string DEPART_DATE { get; set; }

            [XmlElement("DEPART_TIME")]
            public string DEPART_TIME { get; set; }

            [XmlElement("DEPART_CITY")]
            public string DEPART_CITY { get; set; }

            [XmlElement("AIRLINE_CD")]
            public string AIRLINE_CD { get; set; }

            [XmlElement("AIRLINE_NAME")]
            public string AIRLINE_NAME { get; set; }

            [XmlElement("AIRLINE_TYPE")]
            public string AIRLINE_TYPE { get; set; }

            [XmlElement("FLIGHT_NBR")]
            public string FLIGHT_NBR { get; set; }

            [XmlElement("OPER_BY_CD")]
            public string OPER_BY_CD { get; set; }

            [XmlElement("OPER_BY_NAME")]
            public string OPER_BY_NAME { get; set; }

            [XmlElement("OPER_BY_TYPE")]
            public string OPER_BY_TYPE { get; set; }

            [XmlElement("COUPON_STATUS")]
            public string COUPON_STATUS { get; set; }

            [XmlElement("COUPON_STATUS_CD")]
            public string COUPON_STATUS_CD { get; set; }

            [XmlElement("MEALS_OTHER")]
            public List<MEALS_OTHER> MEALS_OTHER { get; set; }

            [XmlElement("BKNG_CLS")]
            public string BKNG_CLS { get; set; }

            [XmlElement("CABIN")]
            public string CABIN { get; set; }

            [XmlElement("ARRIVE_DAY")]
            public string ARRIVE_DAY { get; set; }

            [XmlElement("ARRIVE_DATE")]
            public string ARRIVE_DATE { get; set; }

            [XmlElement("ARRIVE_TIME")]
            public string ARRIVE_TIME { get; set; }

            [XmlElement("ARRIVE_CITY")]
            public string ARRIVE_CITY { get; set; }

            [XmlElement("ARRIVE_IND")]
            public string ARRIVE_IND { get; set; }

            [XmlElement("LAYOVER_IND")]
            public string LAYOVER_IND { get; set; }

            [XmlElement("DepartureAirport")]
            public string DepartureAirport { get; set; }

            [XmlElement("ArrivalAirport")]
            public string ArrivalAirport { get; set; }

            [XmlElement("UTC_DEPART_TIME")]
            public string UTC_DEPART_TIME { get; set; }

            [XmlElement("HOURS_TO_DEPART")]
            public HOURS_TO_DEPART HOURS_TO_DEPART { get; set; }

            [XmlElement("TERMINAL_INFO")]
            public TERMINAL_INFO TERMINAL_INFO { get; set; }

            [XmlElement("OA_FOOTNOTE")]
            public string OA_FOOTNOTE { get; set; }
        }

        public class MEALS_OTHER
        {
            [XmlElement("MEALS_OTHER_CD")]
            public string MEALS_OTHER_CD { get; set; }

            [XmlElement("MEALS_OTHER_DESC")]
            public string MEALS_OTHER_DESC { get; set; }
        }

        public class HOURS_TO_DEPART
        {
            // preserve inner XML as string
            [XmlAnyElement]
            public System.Xml.XmlElement[] Any { get; set; }
        }

        public class TERMINAL_INFO
        {
            [XmlElement("FlightNumber")]
            public string FlightNumber { get; set; }

            [XmlElement("FlightDesig")]
            public string FlightDesig { get; set; }

            [XmlElement("FlightDate")]
            public string FlightDate { get; set; }

            [XmlElement("DepartureAirport")]
            public string DepartureAirport { get; set; }

            [XmlElement("DepartureCountryName")]
            public string DepartureCountryName { get; set; }

            [XmlElement("DepartureCountryCode")]
            public string DepartureCountryCode { get; set; }

            [XmlElement("ArrivalAirport")]
            public string ArrivalAirport { get; set; }

            [XmlElement("ArrivalCountryName")]
            public string ArrivalCountryName { get; set; }

            [XmlElement("ArrivalCountryCode")]
            public string ArrivalCountryCode { get; set; }

            [XmlElement("ClassCode")]
            public string ClassCode { get; set; }

            [XmlElement("OperatedBy")]
            public string OperatedBy { get; set; }

            [XmlElement("OperatedByDescription")]
            public string OperatedByDescription { get; set; }

            [XmlElement("ClassCodeDescription")]
            public string ClassCodeDescription { get; set; }

            [XmlElement("OtherAirLineBaggageURL")]
            public string OtherAirLineBaggageURL { get; set; }

            [XmlElement("ContactNumbers")]
            public string ContactNumbers { get; set; }

            [XmlElement("CheckInAirportDecription")]
            public string CheckInAirportDecription { get; set; }

            [XmlElement("CheckInTerminalDescription")]
            public string CheckInTerminalDescription { get; set; }

            [XmlElement("ArrivalAirportDecription")]
            public string ArrivalAirportDecription { get; set; }

            [XmlElement("ArrivalTerminalDescription")]
            public string ArrivalTerminalDescription { get; set; }
        }

        public class PSNGR_INFO
        {
            [XmlElement("FULL_NM")]
            public string FULL_NM { get; set; }

            [XmlElement("LNAME")]
            public string LNAME { get; set; }

            [XmlElement("FNAME")]
            public string FNAME { get; set; }

            [XmlElement("MI")]
            public string MI { get; set; }

            [XmlElement("TIER")]
            public string TIER { get; set; }

            [XmlElement("MEMBER_STATUS")]
            public string MEMBER_STATUS { get; set; }

            [XmlElement("MEDALLION_IND")]
            public string MEDALLION_IND { get; set; }

            [XmlElement("SKYMILES_NBR")]
            public string SKYMILES_NBR { get; set; }

            [XmlElement("LST_THREE_SKYMILES")]
            public string LST_THREE_SKYMILES { get; set; }

            [XmlElement("PPR_NBR")]
            public string PPR_NBR { get; set; }

            [XmlElement("EMP_DTE")]
            public string EMP_DTE { get; set; }

            [XmlElement("NON_DL_EMP")]
            public string NON_DL_EMP { get; set; }

            [XmlElement("SEAT_ASGMT")]
            public List<SEAT_ASGMT> SEAT_ASGMT { get; set; }
        }

        public class SEAT_ASGMT
        {
            [XmlElement("AIRLINE_CD")]
            public string AIRLINE_CD { get; set; }

            [XmlElement("AIRLINE_NAME")]
            public string AIRLINE_NAME { get; set; }

            [XmlElement("AIRLINE_TYPE")]
            public string AIRLINE_TYPE { get; set; }

            [XmlElement("FLIGHT_NBR")]
            public string FLIGHT_NBR { get; set; }

            [XmlElement("SEAT")]
            public string SEAT { get; set; }
        }

        public class FARE
        {
            [XmlElement("FARE_CALC1")]
            public string FARE_CALC1 { get; set; }

            [XmlElement("FARE_CALC2")]
            public string FARE_CALC2 { get; set; }

            [XmlElement("BASE_FARE_AMT")]
            public string BASE_FARE_AMT { get; set; }

            [XmlElement("BASE_FARE_CUR_CD")]
            public string BASE_FARE_CUR_CD { get; set; }

            [XmlElement("EQUIV_FARE_PAID_AMT")]
            public string EQUIV_FARE_PAID_AMT { get; set; }

            [XmlElement("EQUIV_FARE_CUR_CD")]
            public string EQUIV_FARE_CUR_CD { get; set; }

            [XmlElement("TAX_AMT")]
            public string TAX_AMT { get; set; }

            [XmlElement("TOTL_FARE_AMT")]
            public string TOTL_FARE_AMT { get; set; }

            [XmlElement("TOTL_FARE_CUR_CD")]
            public string TOTL_FARE_CUR_CD { get; set; }

            [XmlElement("FOP")]
            public string FOP { get; set; }

            [XmlElement("VENDOR")]
            public string VENDOR { get; set; }

            [XmlElement("CC_NBR")]
            public string CC_NBR { get; set; }

            [XmlElement("ORIG_TKT")]
            public string ORIG_TKT { get; set; }

            [XmlElement("ORIG_FOP")]
            public string ORIG_FOP { get; set; }

            [XmlElement("ENDORSEMENT")]
            public string ENDORSEMENT { get; set; }

            [XmlElement("TCKT_TYP_CD")]
            public string TCKT_TYP_CD { get; set; }

            [XmlElement("PAY_WITH_MILES_CD")]
            public string PAY_WITH_MILES_CD { get; set; }

            [XmlElement("MKT_PRD_TYP_CD")]
            public string MKT_PRD_TYP_CD { get; set; }

            [XmlElement("TAX_ID")]
            public string TAX_ID { get; set; }

            [XmlElement("UPSELL_IND")]
            public string UPSELL_IND { get; set; }

            [XmlElement("PAID_WITH_ONLINE")]
            public string PAID_WITH_ONLINE { get; set; }

            [XmlElement("EXGCPN_TKT_NO")]
            public string EXGCPN_TKT_NO { get; set; }

            [XmlElement("EXGCPN_AMT")]
            public string EXGCPN_AMT { get; set; }

            [XmlElement("EXGCPN_CURRCD")]
            public string EXGCPN_CURRCD { get; set; }

            [XmlElement("SKYMILES_ACCOUNTNO")]
            public string SKYMILES_ACCOUNTNO { get; set; }

            [XmlElement("SKYMILES_REDEEMED")]
            public string SKYMILES_REDEEMED { get; set; }
        }

        public class TAXES_CHARGES
        {
            [XmlElement("CHARGE")]
            public List<CHARGE> CHARGE { get; set; }

            [XmlElement("TAX")]
            public List<TAX> TAX { get; set; }

            [XmlElement("TOTL_TAXES")]
            public TOTL_TAXES TOTL_TAXES { get; set; }

            [XmlElement("TOTL_TAXES_CHARGES")]
            public TOTL_TAXES_CHARGES TOTL_TAXES_CHARGES { get; set; }
        }

        public class CHARGE
        {
            [XmlElement("CHARGE_CD")]
            public string CHARGE_CD { get; set; }

            [XmlElement("CHARGE_DESC")]
            public string CHARGE_DESC { get; set; }

            [XmlElement("CHARGE_AMT")]
            public string CHARGE_AMT { get; set; }

            [XmlElement("CHARGE_CURR_CD")]
            public string CHARGE_CURR_CD { get; set; }
        }

        public class TAX
        {
            [XmlElement("TAX_CD")]
            public string TAX_CD { get; set; }

            [XmlElement("TAX_DESC")]
            public string TAX_DESC { get; set; }

            [XmlElement("TAX_AMT")]
            public string TAX_AMT { get; set; }

            [XmlElement("TAX_CURR_CD")]
            public string TAX_CURR_CD { get; set; }
        }

        public class TOTL_TAXES
        {
            [XmlElement("TOTL_AMT")]
            public string TOTL_AMT { get; set; }

            [XmlElement("TOTL_CURR_CD")]
            public string TOTL_CURR_CD { get; set; }
        }

        public class TOTL_TAXES_CHARGES
        {
            [XmlElement("TOTL_CD")]
            public string TOTL_CD { get; set; }

            [XmlElement("TOTL_AMT")]
            public string TOTL_AMT { get; set; }

            [XmlElement("TOTL_CURR_CD")]
            public string TOTL_CURR_CD { get; set; }
        }

        public class TICKET_DTL
        {
            [XmlElement("TCKT_NBR")]
            public string TCKT_NBR { get; set; }

            [XmlElement("EXP_DTE")]
            public string EXP_DTE { get; set; }

            [XmlElement("ISSUE_DTE")]
            public string ISSUE_DTE { get; set; }

            [XmlElement("ISSUE_PLACE")]
            public string ISSUE_PLACE { get; set; }

            [XmlElement("ISSUE_PLACE_TEXT")]
            public string ISSUE_PLACE_TEXT { get; set; }

            [XmlElement("ISSUE_AGENT_ID")]
            public string ISSUE_AGENT_ID { get; set; }
        }

        public class TRIP_BAG_FEES
        {
            [XmlElement("DEPART_DAY_DATE")]
            public string DEPART_DAY_DATE { get; set; }

            [XmlElement("ARRIVE_DAY_DATE")]
            public string ARRIVE_DAY_DATE { get; set; }

            [XmlElement("SEGMENT_BAG_FEES")]
            public SEGMENT_BAG_FEES SEGMENT_BAG_FEES { get; set; }

            [XmlElement("TOTAL_CHRG")]
            public string TOTAL_CHRG { get; set; }

            [XmlElement("CURRENCY_CD")]
            public string CURRENCY_CD { get; set; }

            [XmlElement("DISCLAIMER")]
            public DISCLAIMER DISCLAIMER { get; set; }
        }

        public class SEGMENT_BAG_FEES
        {
            [XmlElement("AIRLINE_RULE_APPLIED")]
            public string AIRLINE_RULE_APPLIED { get; set; }

            [XmlElement("AIRLINE_CD_RULE_APPLIED")]
            public string AIRLINE_CD_RULE_APPLIED { get; set; }

            [XmlElement("DEPART_CITY")]
            public string DEPART_CITY { get; set; }

            [XmlElement("ARRIVE_CITY")]
            public string ARRIVE_CITY { get; set; }

            [XmlElement("DEPART_DAY_DATE")]
            public string DEPART_DAY_DATE { get; set; }

            [XmlElement("ARRIVE_DAY_DATE")]
            public string ARRIVE_DAY_DATE { get; set; }

            [XmlElement("BAG_COUNT")]
            public string BAG_COUNT { get; set; }

            [XmlElement("BAG")]
            public List<BAG> BAG { get; set; }

            [XmlElement("TAX")]
            public string TAX { get; set; }

            [XmlElement("TOTAL_CHRG")]
            public string TOTAL_CHRG { get; set; }

            [XmlElement("CURRENCY_CD")]
            public string CURRENCY_CD { get; set; }
        }

        public class BAG
        {
            [XmlElement("BAGGAGE_TYPE")]
            public string BAGGAGE_TYPE { get; set; }

            [XmlElement("BAGGAGE_DESCRIPTION")]
            public string BAGGAGE_DESCRIPTION { get; set; }

            [XmlElement("ALLOWANCE_TYPE")]
            public string ALLOWANCE_TYPE { get; set; }

            [XmlElement("ALLOWANCE_AMT")]
            public string ALLOWANCE_AMT { get; set; }

            [XmlElement("CHECKED_BAG_NBR")]
            public string CHECKED_BAG_NBR { get; set; }

            [XmlElement("CURRENCY_CD")]
            public string CURRENCY_CD { get; set; }
        }

        public class DISCLAIMER
        {
            [XmlElement("DESC")]
            public string DESC { get; set; }
        }

        public class FlyingClubDetails
        {
            [XmlAttribute("currencyCode")]
            public string currencyCode { get; set; }

            [XmlAttribute("currentMiles")]
            public string currentMiles { get; set; }

            [XmlAttribute("currentTier")]
            public string currentTier { get; set; }

            [XmlAttribute("earnMiles")]
            public string earnMiles { get; set; }

            [XmlAttribute("earnTier")]
            public string earnTier { get; set; }

            [XmlAttribute("equivalentMoney")]
            public string equivalentMoney { get; set; }

            [XmlAttribute("redeemedMiles")]
            public string redeemedMiles { get; set; }
        }
    }
}