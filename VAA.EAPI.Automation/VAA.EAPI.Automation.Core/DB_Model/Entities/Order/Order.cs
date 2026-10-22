using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VAA.EAPI.Automation.Core.DB_Model.Entities.Order
{
    public class Order
    {
        public string id { get; set; }
        public string OrderID { get; set; }
        public string OrderHexAddress { get; set; }
        public string OrderVersion { get; set; }
        public string DocType { get; set; }
        public string BookingCancelledIndicator { get; set; }
        public OrderDetails OrderDetails { get; set; }
        public OriginalPOS OriginalPOS { get; set; }
        public List<CustomerDetail> CustomerDetails { get; set; }
        public List<SegmentDetail> SegmentDetails { get; set; }
        public List<SpecialServiceRequest> SpecialServiceRequest { get; set; }
        public List<AncillaryOrder> AncillaryOrders { get; set; }
        public Fulfillment Fulfillment { get; set; }
        public OrderHistorydetails OrderHistorydetails { get; set; }
        public string SourceSystem { get; set; }
        public string _rid { get; set; }
        public string _self { get; set; }
        public string _etag { get; set; }
        public string _attachments { get; set; }
        public int _ts { get; set; }
    }
        public class ActualEndDeliveryDateTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class ActualStartDeliveryDateTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class Address
        {
            public string contactType { get; set; }
            public string AddressLine1 { get; set; }
            public string AddressLine2 { get; set; }
            public string AddressLine3 { get; set; }
            public string AddressLine4 { get; set; }
            public string Region { get; set; }
            public string PostalCode { get; set; }
            public string Country { get; set; }
            public string CityName { get; set; }
        }

        public class AmendedDateTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class Amount
        {
            public string MonetaryAmount { get; set; }
            public string OfferCurrency { get; set; }
            public string ForexRate { get; set; }
            public string BaseCurrency { get; set; }
        }

        public class AncillaryOrder
        {
            public OrderItemId OrderItemId { get; set; }
            public SupplierReference SupplierReference { get; set; }
            public OfferIdReference OfferIdReference { get; set; }
            public OfferItemIdReference OfferItemIdReference { get; set; }
            public List<AncillaryOrderItem> AncillaryOrderItems { get; set; }
            public List<FlightProduct> FlightProduct { get; set; }
            public ProductSpec ProductSpec { get; set; }
            public List<ServiceFulfillment> ServiceFulfillments { get; set; }
            public List<ServicePayment> ServicePayments { get; set; }
            public List<ServiceStatus> ServiceStatus { get; set; }
            public ActualStartDeliveryDateTime ActualStartDeliveryDateTime { get; set; }
            public PlannedStartDeliveryDateTime PlannedStartDeliveryDateTime { get; set; }
            public ActualEndDeliveryDateTime ActualEndDeliveryDateTime { get; set; }
            public PlannedEndDeliveryDateTime PlannedEndDeliveryDateTime { get; set; }
            public string QuantityDelivered { get; set; }
            public List<Tracking> Tracking { get; set; }
        }

        public class AncillaryOrderItem
        {
            public string SsrNumber { get; set; }
            public string CustomerOrderItemSequenceNumber { get; set; }
            public string MarketedProductCode { get; set; }
            public string OrderItemPurchaseAmount { get; set; }
            public string OrderItemPurchaseCurrencyCode { get; set; }
            public string OrderItemPurchaseUSDAmount { get; set; }
            public string FormOfPaymentCategoryCode { get; set; }
            public string FormOfPaymentTypeCode { get; set; }
            public string RecordLocatorIdentifier { get; set; }
            public string BookingCreationGMTTimestamp { get; set; }
            public string Customer_Id { get; set; }
            public string ReservationSystemProviderCode { get; set; }
            public string BookingACNNumber { get; set; }
            public string TicketingACNNumber { get; set; }
            public string AgentSecurityIdentifier { get; set; }
            public string AgentDutyCode { get; set; }
            public string AgentLocationCode { get; set; }
            public string WaiverCode { get; set; }
            public string EMDNumber { get; set; }
            public string EMDIssueDate { get; set; }
            public string SOAApplicationChannelIdentifier { get; set; }
            public string SOAChannelSessionName { get; set; }
            public string CreationGMTTimestamp { get; set; }
            public string LastUpdateUserIdentifier { get; set; }
            public string LastUpdateGMTTimestamp { get; set; }
            public string AssociatedTicketDocumentNumber { get; set; }
            public string AssociatedTicketDocumentIssueLocalDate { get; set; }
            public string OrderItemExpirationDate { get; set; }
            public string MarketedProductOfferingTypeCode { get; set; }
            public string OrderItemWaiverQuoteAmount { get; set; }
            public string EMDCouponNumber { get; set; }
            public string MarketedProductTypeCode { get; set; }
            public string PurchaseId { get; set; }
            public string CustomerIdentifier { get; set; }
            public string CustomerNumber { get; set; }
            public string CustomerFullName { get; set; }
            public string CustomerFirstName { get; set; }
            public string CustomerLastName { get; set; }
            public string LoyaltyProgramCode { get; set; }
            public string LoyaltyCarrierCode { get; set; }
            public string LoyaltyProgramTierCode { get; set; }
            public string LoyaltyProgramAccountIdentifier { get; set; }
        }

        public class BookingDetail
        {
            public string href { get; set; }
            public string rel { get; set; }
            public string title { get; set; }
        }

        public class BookingReference
        {
            public string AirlineDesignator { get; set; }
            public string RLOC { get; set; }
            public string InfantOnlyFlag { get; set; }
            public string RLOCCreateDate { get; set; }
            public string BookingChannel { get; set; }
            public string GroupName { get; set; }
            public string NumberInGroup { get; set; }
            public string ExternalRecordLocator { get; set; }
            public string ActiveLevel { get; set; }
            public string ExternalSystem { get; set; }
            public BookingDetail BookingDetail { get; set; }
        }

        public class CancelledDateTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class Contact
        {
            public string contactType { get; set; }
            public string name { get; set; }
            public string email { get; set; }
            public string phone { get; set; }
            public string countrycode { get; set; }
        }

        public class CreationDateTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class CustomerContacts
        {
            public string ContactInfoID { get; set; }
            public List<Contact> Contacts { get; set; }
            public List<Address> Addresses { get; set; }
        }

        public class CustomerDetail
        {
            public string VAASCVID { get; set; }
            public string ContactTypeText { get; set; }
            public string Title { get; set; }
            public string CustID { get; set; }
            public string PTC { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
            public string FirstNameNumber { get; set; }
            public string LastNameNumber { get; set; }
            public string Bookingtype { get; set; }
            public string Bookingtyperemarks { get; set; }
            public FrequentFlyer FrequentFlyer { get; set; }
            public string CustomerType { get; set; }
            public string CustomerSubType { get; set; }
            public string CustomerValue { get; set; }
            public string CustomerRiskValue { get; set; }
            public string CustomerLifeTimeValue { get; set; }
            public string VirginHolidayBookingFlag { get; set; }
            public string CorporateIndicator { get; set; }
            public string CorporateAccountName { get; set; }
            public string StaffTravellerIndicator { get; set; }
            public string StaffTravellerType { get; set; }
            public string DocumentType { get; set; }
            public string DocumentNumber { get; set; }
            public string CountryOfIssue { get; set; }
            public string GenderOnDocument { get; set; }
            public string DocumentFirstName { get; set; }
            public string DocumentMiddleName { get; set; }
            public string DocmumentLastName { get; set; }
            public string DocumentTitle { get; set; }
            public string DOBOnDocument { get; set; }
            public string Nationality { get; set; }
            public string ExpiryDate { get; set; }
            public List<CustomerService> CustomerService { get; set; }
            public CustomerContacts CustomerContacts { get; set; }
            public HealthPassport HealthPassport { get; set; }
            public string NonRevenueStandbyFlag { get; set; }
            public string RevenueStandbyFlag { get; set; }
            public string GroupId { get; set; }
        }

        public class CustomerService
        {
            public string SegmentNo { get; set; }
            public string BaggageFlag { get; set; }
            public string UpgradeFlag { get; set; }
            public string TypeOfUpgrade { get; set; }
            public string UpgradeFrom { get; set; }
            public string UpgradeTo { get; set; }
            public string PreSelectMeal { get; set; }
            public string PrefferedSeatBooked { get; set; }
            public string ClubHouseEligibilityDescription { get; set; }
            public string ClubHouseEntryDateTime { get; set; }
            public string ClubHouseUsageIndicator { get; set; }
            public string ClubHouseLoungeName { get; set; }
        }

        public class Device
        {
            public string DeviceCode { get; set; }
            public string TouchPoint { get; set; }
            public string Latitude { get; set; }
            public string Longitude { get; set; }
        }

        public class FarePrice
        {
            public List<object> ApprovalCode { get; set; }
            public List<FarePriceDetail> FarePriceDetails { get; set; }
            public List<object> DtiSegmentInfo { get; set; }
        }

        public class FarePriceDetail
        {
            public string AdvisoryMessage { get; set; }
            public string BaseAmount { get; set; }
            public string BaseCurrency { get; set; }
            public string BooksCount { get; set; }
            public string DTIType { get; set; }
            public string DtiCount { get; set; }
            public string ExpandedPassengerTypeCode { get; set; }
            public string FarecalcType { get; set; }
            public string InternationalFlag { get; set; }
            public string PassengerTypeCode { get; set; }
            public List<object> Tax { get; set; }
            public string TaxItemCount { get; set; }
            public string TotalFareAmount { get; set; }
            public string TotalFareCurrency { get; set; }
            public string TotalFareType { get; set; }
            public string TotalTax { get; set; }
        }

        public class FlightDistance
        {
            public string Distance { get; set; }
            public string Unit { get; set; }
        }

        public class FlightProduct
        {
            public string FlightServiceProduct { get; set; }
            public string CustomerOrderItemSequenceNumber { get; set; }
            public string BookingSegmentNumber { get; set; }
            public string BookingCode { get; set; }
            public string FlightDetailSequenceNumber { get; set; }
            public string MarketedCarrierCode { get; set; }
            public string MarketedFlightNumber { get; set; }
            public string ScheduledDepartureLocalDate { get; set; }
            public string OriginTransportationStationCode { get; set; }
            public string DestinationTransportationStationCode { get; set; }
            public string ClassOfServiceCode { get; set; }
            public string SeatIdentifier { get; set; }
            public string ApplicationChannel { get; set; }
            public string SOAChannelSessionName { get; set; }
            public string CreationUserIdentifier { get; set; }
            public string FlightDetailCreationGMTTimestamp { get; set; }
            public string FlightSegmentNumber { get; set; }
            public string CabinClassCode { get; set; }
        }

        public class FrequentFlyer
        {
            public string AccountNumber { get; set; }
            public string TierCode { get; set; }
            public string AirlineCode { get; set; }
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string OsiNumber { get; set; }
            public string Status { get; set; }
            public string Validated { get; set; }
        }

        public class Fulfillment
        {
            public List<object> Tickets { get; set; }
            public List<object> Seat { get; set; }
        }

        public class HealthPassport
        {
            public string PassengerNameNumber { get; set; }
            public string PassengerFirstName { get; set; }
            public string PassengerLastName { get; set; }
            public string HealthPassportSource { get; set; }
            public string HealthPassportStatus { get; set; }
        }

        public class HistoryActionItem
        {
            public string ActionCode { get; set; }
            public string FreeformText { get; set; }
            public string OrdinalNumber { get; set; }
        }

        public class HistoryCreditOrSignature
        {
            public string CreditText { get; set; }
            public string OrdinalNumber { get; set; }
        }

        public class HistoryPnrCreation
        {
            public string CreditText { get; set; }
            public string OrdinalNumber { get; set; }
            public string GMTTs { get; set; }
        }

        public class Leg
        {
            public string LegSequenceNumber { get; set; }
            public string OriginAirportCode { get; set; }
            public string Destination_Airport_Code { get; set; }
            public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
            public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
            public string EquipmentType { get; set; }
            public LegDistance LegDistance { get; set; }
            public string LegDuration { get; set; }
            public string DelegatedCarrier { get; set; }
            public string OperatingCarrier { get; set; }
        }

        public class LegDistance
        {
            public string Distance { get; set; }
            public string Unit { get; set; }
        }

        public class Link
        {
            public string href { get; set; }
            public string rel { get; set; }
            public string title { get; set; }
        }

        public class OfferIdReference
        {
            public string OfferId { get; set; }
            public string OwnerAirline { get; set; }
            public string OwnerNonAirline { get; set; }
            public string ShoppingResponseID { get; set; }
            public TotalPrice TotalPrice { get; set; }
            public Link Link { get; set; }
        }

        public class OfferItemIdRef
        {
            public string OrderItemId { get; set; }
            public string OwnerAirline { get; set; }
            public string OwnerNonAirline { get; set; }
        }

        public class OfferItemIdReference
        {
            public string OfferItemId { get; set; }
            public string OwnerAirline { get; set; }
            public string OwnerNonAirline { get; set; }
        }

        public class OfferRef
        {
            public string OrderId { get; set; }
            public string OwnerAirline { get; set; }
            public string OwnerNonAirline { get; set; }
            public Link Link { get; set; }
        }

        public class OrderChangeWebHook
        {
            public string href { get; set; }
            public string rel { get; set; }
            public string title { get; set; }
        }

        public class OrderDetails
        {
            public string OrderId { get; set; }
            public string OwnerAirline { get; set; }
            public string OwnerNonAirline { get; set; }
            public OrderChangeWebHook OrderChangeWebHook { get; set; }
            public string ValidatingCarrier { get; set; }
            public FarePrice FarePrice { get; set; }
            public List<object> StoredFare { get; set; }
            public List<object> FareQuote { get; set; }
            public CreationDateTime CreationDateTime { get; set; }
            public CancelledDateTime CancelledDateTime { get; set; }
            public AmendedDateTime AmendedDateTime { get; set; }
            public TotalFeesAndTaxes TotalFeesAndTaxes { get; set; }
            public TotalProductPrice TotalProductPrice { get; set; }
            public List<BookingReference> BookingReferences { get; set; }
            public string OutsideSystemRecordLocator { get; set; }
        }

        public class OrderHistorydetails
        {
            public List<HistoryActionItem> HistoryActionItem { get; set; }
            public List<HistoryCreditOrSignature> HistoryCreditOrSignature { get; set; }
            public HistoryPnrCreation HistoryPnrCreation { get; set; }
            public List<OtherServiceInformation> OtherServiceInformation { get; set; }
            public List<object> Remarks { get; set; }
        }

        public class OrderItemId
        {
            public string OrderItem { get; set; }
            public string CustomerOrderNumber { get; set; }
            public string OrderPurchaseCount { get; set; }
            public string BillToCustomerName { get; set; }
            public string CreationUserIdentifier { get; set; }
            public string CreationGMTTimestamp { get; set; }
            public string SourceSystem { get; set; }
            public string EmailId { get; set; }
            public string OwnerAirline { get; set; }
            public string OwnerNonAirline { get; set; }
        }

        public class OriginalPOS
        {
            public string partyName { get; set; }
            public string IATANumber { get; set; }
            public string AgencyId { get; set; }
            public string AgentId { get; set; }
            public string PCC { get; set; }
            public string CityCode { get; set; }
            public string GDSPSSIndentifier { get; set; }
            public string CountryCode { get; set; }
            public Device Device { get; set; }
        }

        public class OtherServiceInformation
        {
            public string AirlineCode { get; set; }
            public string FreeformText { get; set; }
            public string OsiNumber { get; set; }
        }

        public class PlannedEndDeliveryDateTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class PlannedStartDeliveryDateTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class ProductSpec
        {
            public string ProductOwner { get; set; }
            public string ProductCode { get; set; }
            public string FareBasisCode { get; set; }
            public string ProductName { get; set; }
            public string ShortDescription { get; set; }
            public string ProductType { get; set; }
            public string ProductClass { get; set; }
        }
        public class ScheduledArrivalTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class ScheduledDepartureTime
        {
            public string Local_Date_Time { get; set; }
            public string UTC_Date_Time { get; set; }
        }

        public class SegmentDetail
        {
            public string ClassOfService_DualRBD { get; set; }
            public string ArrivalPoint { get; set; }
            public string ArrivalTime { get; set; }
            public string CassEquipment { get; set; }
            public string DepartureDayOfWeekNumber { get; set; }
            public string DeparturePoint { get; set; }
            public string DepartureTime { get; set; }
            public string ElapsedTime { get; set; }
            public string FirstLegDepartureCity { get; set; }
            public string FirstLegDepartureDate { get; set; }
            public string FirstLegDepartureTime { get; set; }
            public string FirstLegDepartureYear { get; set; }
            public string FlightDepartureDate { get; set; }
            public string FlightLegDepartureYear { get; set; }
            public string FlightMileage { get; set; }
            public string InventoryEquipment { get; set; }
            public string LegArrivalDate { get; set; }
            public string LegNumber { get; set; }
            public string MealServiceCode { get; set; }
            public string OfficialAirlineGuideEquipmentCode { get; set; }
            public string SecureFlightFlag { get; set; }
            public string SegmentNumber { get; set; }
            public string ActiveFlag { get; set; }
            public string AirlineCode { get; set; }
            public string BinaryFareTypeCode { get; set; }
            public string ClassOfService { get; set; }
            public string ClassOfServiceOverBookedFlag { get; set; }
            public string CodeShareParticipantFlag { get; set; }
            public string CompartmentNumber { get; set; }
            public string CompartmentOverBookedFlag { get; set; }
            public string ContinuingFlag { get; set; }
            public string CurrentActionCode { get; set; }
            public string DepartureDate { get; set; }
            public string DepartureDateAdjustment { get; set; }
            public string ElectronicTicketEligibleFlag { get; set; }
            public string FirstIndexableFlag { get; set; }
            public string FlightNumber { get; set; }
            public string InFlightDateAdjustment { get; set; }
            public string MarriedControlAvilBucket { get; set; }
            public string NoSmokingFlag { get; set; }
            public string NumberInParty { get; set; }
            public string PoolAllocationFlag { get; set; }
            public string PreviousActionCode { get; set; }
            public string PseudoTripNumber { get; set; }
            public string RmsFareValue { get; set; }
            public string ScheduleChangeFlag { get; set; }
            public string SegmentDepartureYear { get; set; }
            public string SegmentType { get; set; }
            public string UniqueBasId { get; set; }
            public string VitualInvBuckets { get; set; }
            public string MarketingCarrier { get; set; }
            public string MarketingFlightNumber { get; set; }
            public string OperatingCarrier { get; set; }
            public string OperatingFlightNumber { get; set; }
            public string OriginAirportCode { get; set; }
            public string DestinationAirportCode { get; set; }
            public string FirstNameNumber { get; set; }
            public string LastNameNumber { get; set; }
            public string OutboundConnectionFlag { get; set; }
            public string InboundConnectionFlag { get; set; }
            public ScheduledDepartureTime ScheduledDepartureTime { get; set; }
            public ScheduledArrivalTime ScheduledArrivalTime { get; set; }
            public string ConnectionTime { get; set; }
            public string NumberOfStops { get; set; }
            public FlightDistance FlightDistance { get; set; }
            public string FlightDuration { get; set; }
            public string NextDayIndicator { get; set; }
            public List<Leg> Legs { get; set; }
            public string ProductId { get; set; }
            public string SpaceSoldFromInventoryThisTransaction { get; set; }
            public string OandDTripNumberSegment { get; set; }
            public string SpaceFromInventory { get; set; }
            public string ReducedNumberInPartyOA { get; set; }
            public string ReducedNumberInPartyDL { get; set; }
        }

        public class ServiceFulfillment
        {
            public string FulfilmentStatusTypeCode { get; set; }
            public string CustomerOrderItemSequenceNumber { get; set; }
            public string FulfilmentStatusCreationGMTTimestamp { get; set; }
            public string FulfilmentServiceEventCode { get; set; }
            public string FulfilmentStatusReasonCode { get; set; }
            public string FulfilmentStatusReasontext { get; set; }
            public string OperationalFlightNumber { get; set; }
            public string OperationalCarrierCode { get; set; }
            public string ScheduleDepartureDateTime { get; set; }
            public string OriginTripStationCode { get; set; }
            public string DestinationTripStationCode { get; set; }
            public string MarketingProductCode { get; set; }
            public string ProductFulfilmentId { get; set; }
            public string ProductFulfilmentGMTTimestamp { get; set; }
            public string SourceSystem { get; set; }
            public string CabinClassCode { get; set; }
        }

        public class ServicePayment
        {
            public string PurchaseId { get; set; }
            public string TotalOrderPurchaseAmount { get; set; }
            public string TotalOrderPurchaseTax { get; set; }
            public string CreationGMTTimestamp { get; set; }
        }

        public class ServiceStatus
        {
            public string CustomerOrderStatusTypeCode { get; set; }
            public string CustomerOrderStatusCreationGMTTimestamp { get; set; }
            public string CustomerStatusReason { get; set; }
            public string CustomerOrderItemSequenceNumber { get; set; }
        }

        public class SpecialServiceRequest
        {
            public string AirlineCode { get; set; }
            public string AllSegmentsFlag { get; set; }
            public string AllPassengersFlag { get; set; }
            public string FlightDate { get; set; }
            public string FlightNumber { get; set; }
            public string FirstNameNumber { get; set; }
            public string LastNameNumber { get; set; }
            public string NumberOfSsrsRequestedFor { get; set; }
            public string Remarks { get; set; }
            public string RequestCode { get; set; }
            public string SsrNumber { get; set; }
            public string StatusCode { get; set; }
            public string LapInfantIndexNumber { get; set; }
        }

        public class SupplierReference
        {
            public OfferRef OfferRef { get; set; }
            public OfferItemIdRef OfferItemIdRef { get; set; }
        }

        public class TotalFeesAndTaxes
        {
            public string MonetaryAmount { get; set; }
            public string OfferCurrency { get; set; }
            public string ForexRate { get; set; }
            public string BaseCurrency { get; set; }
        }

        public class TotalPrice
        {
            public string MonetaryAmount { get; set; }
            public string OfferCurrency { get; set; }
            public string ForexRate { get; set; }
            public string BaseCurrency { get; set; }
        }

        public class TotalProductPrice
        {
            public string PriceComponentType { get; set; }
            public string PriceComponentName { get; set; }
            public string PriceComponentDescription { get; set; }
            public Amount Amount { get; set; }
        }

        public class Tracking
        {
            public string TrackingId { get; set; }
            public string IdType { get; set; }
        } 
}