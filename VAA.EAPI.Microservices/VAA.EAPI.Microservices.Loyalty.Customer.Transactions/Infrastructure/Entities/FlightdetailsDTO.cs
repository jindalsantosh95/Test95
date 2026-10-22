using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Domain.Transaction;

namespace VAA.EAPI.Microservices.Loyalty.Customer.Transactions.Infrastructure.Entities
{
    public class FlightdetailsDTO
    {
        public string BookingDate { get; set; }
        public string BookingClass { get; set; }
        public string DestinationAirport { get; set; }
        public string ExternalReservationNumber { get; set; }
        public string FareBasis { get; set; }
        public string FareBasisCode { get; set; }
        public string GDSRecordLocator { get; set; }
        public string InternalFlightDate { get; set; }
        public string InternalOrigin { get; set; }
        public string MarketingFlightNumber { get; set; }
        public string MarketingPartnerCode { get; set; }
        public string OperatingFlightNumber { get; set; }
        public string OriginAirportcode { get; set; }
        public string PNRName { get; set; }
        public string DepartureDate { get; set; }
        public string Ticketnumber { get; set; } 
        public string OperatingAirlineAuthNumber { get; set; }
        public string TravelClass { get; set; }
        public string PassengerTypeCheckDigit { get; set; }
        public string RetroOverrideReason { get; set; }
        public string DocumentType { get; set; }
        public string RetroOverrideFlag { get; set; }
        public string BoardingNumber { get; set; }
        public string NumberOfSegments { get; set; }
        public string FirstNameAsPerDocument { get; set; }
        public string LastNameAsPerDocument { get; set; }
        public string SecondaryDocumentType { get; set; }
        public string Seatnumber { get; set; }
        public string CodeShareFlag { get; set; }
        public string MarketingBookingClass { get; set; }
        public string SecondaryDocumentStatus { get; set; }
        public string PrimaryDocumentStatus { get; set; }
        public string RetroFlag { get; set; }
        public string CabinClass { get; set; }
        public string OriginalOrigin { get; set; }
        public string OriginalDestination { get; set; }
        public string OriginalBookingClass { get; set; }
        public string OriginalActivityDate { get; set; }
        public string OriginalFlightNumber { get; set; }
        public string PassengerType { get; set; }
        public string ForceCredit { get; set; }
        public string VABaseFare { get; set; }
        public string VANetFare { get; set; }
        public string VARetroComments { get; set; }
        public string VAOperatingAirlineComments { get; set; }
        public string VAAgentSetAddress { get; set; }
        public string VAAgentDutyCode { get; set; }
        public string VAAgencyIATANumber { get; set; }
        public string? SalesLocation { get; set; }
        public string VAAgentInitial { get; set; }
        public string VAJFTransactionId { get; set; }
        public string VAJFUpdateTransactionId { get; set; }
        public string VAPArtialFlownIndicator { get; set; }
        public string VAPseudoCityCode { get; set; }
        public string VAPassengerTypeGroups { get; set; }
        public string VARewardType { get; set; }
        public string VASourceTransactionCode { get; set; }
        public string VAMarketedCabin { get; set; }
        public string VAOtherTax { get; set; }
        public string VABonusPromoId { get; set; }
        public string VAPostingTypeCode { get; set; }
        public string UserCreated { get; set; }
        public string CommunicationSent { get; set; }
        public string MIFlag { get; set; }
        public string RetroResponseErrorCode { get; set; }
        public string RetroResponseErrorMessage { get; set; }
        public string BookingType { get; set; }
        public string VAETicketIndicator { get; set; }
        public string TransactionId { get; set; }
        public string PartnerProcessedDate { get; set; }
        public string Distance { get; set; }
        public string Quantity { get; set; }
        public string TaxAmount { get; set; }
        public List<PassengerdetailDTO> PassengerDetails { get; set; }

        public FlightdetailsDTO()
        {

        }

        public static explicit operator FlightdetailsDTO(Flightdetails flightdetails)
        {
            return new()
            {
                BookingClass = flightdetails?.BookingClass,
                BookingDate = string.IsNullOrEmpty(flightdetails?.BookingDate) ? null : DateTime.Parse(flightdetails?.BookingDate).ToString(Constants.Application.DBTimeFormat),
                DestinationAirport = flightdetails?.DestinationAirport,
                ExternalReservationNumber = flightdetails?.ExternalReservationNumber,
                FareBasis = flightdetails?.FareBasis,
                FareBasisCode = flightdetails?.FareBasisCode,
                GDSRecordLocator = flightdetails?.GDSRecordLocator,
                InternalFlightDate = string.IsNullOrEmpty(flightdetails?.InternalFlightDate) ? null : DateTime.Parse(flightdetails?.InternalFlightDate).ToString(Constants.Application.DBTimeFormat),
                InternalOrigin = flightdetails?.InternalOrigin,
                MarketingFlightNumber = flightdetails?.MarketingFlightNumber,
                MarketingPartnerCode = flightdetails?.MarketingPartnerCode,
                OperatingFlightNumber = flightdetails?.OperatingFlightNumber,
                OriginAirportcode = flightdetails?.OriginAirportcode,
                PNRName = flightdetails?.PNRName,
                DepartureDate = string.IsNullOrEmpty(flightdetails?.DepartureDate) ? null : DateTime.Parse(flightdetails?.DepartureDate).ToString(Constants.Application.DBTimeFormat),
                Ticketnumber = flightdetails?.Ticketnumber,
                OperatingAirlineAuthNumber = flightdetails?.OperatingAirlineAuthNumber,
                TravelClass = flightdetails?.TravelClass,
                PassengerTypeCheckDigit = flightdetails?.PassengerTypeCheckDigit,
                RetroOverrideReason = flightdetails?.RetroOverrideReason,
                DocumentType = flightdetails?.DocumentType,
                RetroOverrideFlag = flightdetails?.RetroOverrideFlag,
                BoardingNumber = flightdetails?.BoardingNumber,
                NumberOfSegments = flightdetails?.NumberOfSegments,
                FirstNameAsPerDocument = flightdetails?.FirstNameAsPerDocument,
                LastNameAsPerDocument = flightdetails?.LastNameAsPerDocument,
                SecondaryDocumentType = flightdetails?.SecondaryDocumentType,
                Seatnumber = flightdetails?.Seatnumber,
                CodeShareFlag = flightdetails?.CodeShareFlag,
                MarketingBookingClass = flightdetails?.MarketingBookingClass,
                SecondaryDocumentStatus = flightdetails?.SecondaryDocumentStatus,
                PrimaryDocumentStatus = flightdetails?.PrimaryDocumentStatus,
                RetroFlag = flightdetails?.RetroFlag,
                CabinClass = flightdetails?.CabinClass,
                OriginalOrigin = flightdetails?.OriginalOrigin,
                OriginalDestination = flightdetails?.OriginalDestination,
                OriginalBookingClass = flightdetails?.OriginalBookingClass,
                OriginalActivityDate = flightdetails?.OriginalActivityDate,
                OriginalFlightNumber = flightdetails?.OriginalFlightNumber,
                PassengerType = flightdetails?.PassengerType,
                ForceCredit = flightdetails?.ForceCredit,
                VABaseFare = flightdetails?.VABaseFare,
                VANetFare = flightdetails?.VANetFare,
                VARetroComments = flightdetails?.VARetroComments,
                VAOperatingAirlineComments = flightdetails?.VAOperatingAirlineComments,
                VAAgentSetAddress = flightdetails?.VAAgentSetAddress,
                VAAgentDutyCode = flightdetails?.VAAgentDutyCode,
                VAAgencyIATANumber = flightdetails?.VAAgencyIATANumber,
                SalesLocation = flightdetails?.SalesLocation,
                VAAgentInitial = flightdetails?.VAAgentInitial,
                VAJFTransactionId = flightdetails?.VAJFTransactionId,
                VAJFUpdateTransactionId = flightdetails?.VAJFUpdateTransactionId,
                VAPArtialFlownIndicator = flightdetails?.VAPArtialFlownIndicator,
                VAPseudoCityCode = flightdetails?.VAPseudoCityCode,
                VAPassengerTypeGroups = flightdetails?.VAPassengerTypeGroups,
                VARewardType = flightdetails?.VARewardType,
                VASourceTransactionCode = flightdetails?.VASourceTransactionCode,
                VAMarketedCabin = flightdetails?.VAMarketedCabin,
                VAOtherTax = flightdetails?.VAOtherTax,
                VABonusPromoId = flightdetails?.VABonusPromoId,
                VAPostingTypeCode = flightdetails?.VAPostingTypeCode,
                UserCreated = flightdetails?.UserCreated,
                CommunicationSent = flightdetails?.CommunicationSent,
                MIFlag = flightdetails?.MIFlag,
                RetroResponseErrorCode = flightdetails?.RetroResponseErrorCode,
                RetroResponseErrorMessage = flightdetails?.RetroResponseErrorMessage,
                BookingType = flightdetails?.BookingType,
                VAETicketIndicator = flightdetails?.VAETicketIndicator,
                TransactionId = flightdetails?.TransactionId,
                PartnerProcessedDate = flightdetails?.PartnerProcessedDate,
                Distance = flightdetails?.Distance,
                Quantity = !string.IsNullOrEmpty(flightdetails?.Quantity) ? ((int)Convert.ToDouble(flightdetails?.Quantity)).ToString() : string.Empty,
                TaxAmount = flightdetails?.TaxAmount,
                PassengerDetails = flightdetails?.PassengerDetails?.Select(x => (PassengerdetailDTO)x).ToList()
            };
        }

    }
}
