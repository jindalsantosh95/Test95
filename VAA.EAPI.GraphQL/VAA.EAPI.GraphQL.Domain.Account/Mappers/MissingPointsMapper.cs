using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Domain.Account.Helpers;
using VAA.EAPI.GraphQL.Domain.Account.Infrastructure.Models.ReferenceMS;
using VAA.EAPI.GraphQL.Domain.Account.Models.Retro;
using VAA.EAPI.GraphQL.Domain.Account.Models.Transaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerTransactions.CreateTransaction;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.RulesEngineMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.SkyMS;

namespace VAA.EAPI.GraphQL.Domain.Account.Mappers
{
    public class MissingPointsMapper
    {
        public static CreateTransactionRequest CreateTransactionVRedMapper(CustomerCustMS custMS, ProductDetails productDetails, SegmentResponse inRuleResponse, PartnerProfileDetails partnerProfileDetails, StatementTextResponse statementTextResponse, VSRetroFlownDetail pdmResponse, string bookClass)
        {
            return new CreateTransactionRequest()
            {
                CustomerId = custMS.CustomerId,
                MemberNumber = custMS.LoyaltyNumber,
                TransactionSource = Application.WebMyaChannelID,
                Comments = inRuleResponse.ProcessingComments,
                PartnerCode = partnerProfileDetails.PartnerCode,
                PartnerName = partnerProfileDetails.Name,
                PartnerCategory = partnerProfileDetails.PartnerSubType,
                ProductName = productDetails.ProductName,
                ProductDescription = !productDetails.Description.IsNullOrEmpty() ? productDetails.Description : statementTextResponse.StatementText,
                ProductCategory = productDetails.ProductCategory,
                StatementText = statementTextResponse.StatementText,
                TransactionSubType = Application.TxnSubType,
                TransactionId = Helper.GenerateRandomId(),
                TransactionChannel = Application.TransactionChannel,
                ActivityDate = Convert.ToDateTime(inRuleResponse.FlightDate).ToString(Application.ISODateFormat),
                ActivityType = Application.AccrualTransactionType,
                Status = Application.TxnProcessingStatus,
                Points = inRuleResponse.PointsAward,
                CreatedBy = Application.WebMyaChannelID,
                DestinationAirport = pdmResponse.ArrivalAirport,
                OriginationAirport = pdmResponse.DepartureAirport,
                BookingClass = bookClass,
                MarketingCarrierCode = pdmResponse.MarketingAirlineCode,
                RetroIndicatorFlag = Common.Constants.Application.RetroFlag
            };

        }

        public static CabinClassDetailsRequest MapToGetCabinClassDetailsRequest(SkyFlightsPointsClaimRequest request, SkyClaimResponse flyingDetailsResponse)
        {
            return new CabinClassDetailsRequest
            {
                OperatedBookingClass = flyingDetailsResponse?.OperatingCarierBookingClass,
                MarketingBookingClass = flyingDetailsResponse?.MarketingCarrierBookingClass,
                OperatedAirlineCode = flyingDetailsResponse.OperatingCarrierCode,
                MarketingAirlineCode = flyingDetailsResponse.MarketingCarrierCode,
                FlightActivityDate = request.DepartureDate,
                OriginAirportCode = request.DepartureFrom,
                DestinationAirportCode = request.ArrivingAt
            };
        }

        public static CreateTransactionRequest CreateTransactionSkyRetroVRedMapper(Transaction transactionRequest, ProductDetails productDetails, PartnerProfileDetails partnerProfileDetails, StatementTextResponse statementTextResponse)
        {
            return new CreateTransactionRequest()
            {
                CustomerId = transactionRequest.CustomerId,
                TransactionId = transactionRequest.TransactionId,
                PartnerCode = partnerProfileDetails.PartnerCode,
                PartnerName = partnerProfileDetails.Name,
                MemberNumber = transactionRequest.MemberNumber,
                ActivityType = Application.AccrualTransactionType,
                Points = Convert.ToInt32(transactionRequest.TransactionValue),
                ActivityDate = Convert.ToString(transactionRequest.ActivityDate),
                StatementText = statementTextResponse.StatementText,
                CreatedBy = transactionRequest.CreatedBy,
                Created = transactionRequest.CreatedOn,
                UpdatedBy = transactionRequest.LastUpdatedBy,
                Updated = transactionRequest.LastUpdatedOn,
                TransactionChannel = Application.TransactionChannel,
                Status = transactionRequest.StatusCode,
                BookingClass = transactionRequest.FlightDetails?.BookingClass,
                TransactionNumber = transactionRequest.TransactionId,
                ActivityNumber = transactionRequest.ActivityNumber,
                FlightNumber = transactionRequest.FlightDetails?.OperatingFlightNumber,
                NoOfPassengers = Convert.ToInt32(transactionRequest.FlightDetails?.Quantity),
                NoOfSegments = Convert.ToInt32(transactionRequest.FlightDetails?.NumberOfSegments),
                ProductName = productDetails.ProductName,
                TicketNumber = transactionRequest.FlightDetails?.Ticketnumber,
                OperatingAirlineAuthnumber = transactionRequest.FlightDetails?.OperatingAirlineAuthNumber,
                TransactionSubType = transactionRequest.TransactionSubType,
                CancellationReason = transactionRequest.CancellationReasonCode,
                Comments = transactionRequest.Comments,
                CancelledTxnId = transactionRequest.CancelledTransactionId,
                VoucherType = productDetails?.ProductName,
                VoucherDescription = productDetails?.Description,
                PartnerCategory = partnerProfileDetails.PartnerType,
                ProductCategory = productDetails.ProductCategory,
                CancelledBy = transactionRequest.CancelledBy,
                ProductDescription = productDetails.Description,
                TransferMemberNumber = transactionRequest.TransferMemberNumber,
                SegmentBookingClass = transactionRequest.FlightDetails?.PassengerDetails?.ElementAtOrDefault(0)?.SegmentDetails?.ElementAtOrDefault(0)?.BookingClass,
                MarketingBookingClass = transactionRequest.FlightDetails?.MarketingBookingClass,
                MarketingCarrierCode = transactionRequest.FlightDetails?.MarketingPartnerCode,
                MarketingFlightNumber = transactionRequest.FlightDetails?.MarketingFlightNumber,
                SegmentFlightNumber = transactionRequest.FlightDetails?.PassengerDetails?.ElementAtOrDefault(0)?.SegmentDetails?.ElementAtOrDefault(0)?.OperatingFlightNumber,
                PostingType = transactionRequest.FlightDetails?.VAPostingTypeCode,
                OriginationAirport = transactionRequest.FlightDetails?.OriginAirportcode,
                DestinationAirport = transactionRequest.FlightDetails?.DestinationAirport,
                TravelDistance = transactionRequest.FlightDetails?.Distance,
                BasePoints = transactionRequest.CreditDetails?.FirstOrDefault(x => x.PointSubType == Common.Constants.Application.BaseMilesType)?.CreditValue,
                ClassOfServicePoints = transactionRequest.CreditDetails?.FirstOrDefault(x => x.PointSubType == Common.Constants.Application.CabinBonusType)?.CreditValue,
                EliteTierPoints = transactionRequest.CreditDetails?.FirstOrDefault(x => x.PointSubType == Common.Constants.Application.TierBonusType)?.CreditValue,
                PointType = !string.IsNullOrEmpty(transactionRequest.TransactionValue) ? Common.Constants.Application.PointType : string.Empty,
                PromotionBonusPoints = transactionRequest.CreditDetails?.FirstOrDefault(x => x.PointSubType == Common.Constants.Application.PromotionBonusType)?.CreditValue,
                ClassCode = transactionRequest.ClassCode,
                BookingReference = transactionRequest.FlightDetails?.ExternalReservationNumber,
                CabinClass = transactionRequest.FlightDetails?.CabinClass,
                FareBasis = transactionRequest.FlightDetails?.FareBasis,
                PartnerSubType = partnerProfileDetails.PartnerSubType,
                TransactionSource = Application.WebMyaChannelID
            };

        }

        public static TransactionRequest MapTransactionReq(CustomerCustMS customerCustMS, SegmentResponse inRuleResponse, VSRetroFlownDetail pdmFlightDtls, CreateTransactionRequest vredRequest, PartnerProfileDetails partnerCode, ProductDetails productCode, string segmentDistance, string statementText, string bookClass)
        {
            List<PassengerDetails> passengers = new List<PassengerDetails>();
            List<SegmentDetails> segmentDetails = new List<SegmentDetails>();

            SegmentDetails segmentDetail = new SegmentDetails()
            {
                SegmentId = Helper.GenerateRandomId(),
                BookingClass = !string.IsNullOrEmpty(pdmFlightDtls.FlownUpgradedClass) ? pdmFlightDtls.FlownUpgradedClass : pdmFlightDtls.OriginalBookedClass,
                DepartureDate = pdmFlightDtls.DepartureDate,
                DestinationAirportcode = pdmFlightDtls.ArrivalAirport,
                MarketingCarrierCode = pdmFlightDtls.MarketingAirlineCode,
                MarketingFlightNumber = pdmFlightDtls.MarketingFlightNumber,
                PartnerCode = pdmFlightDtls.OperatingAirlineCode,
                OperatingFlightNumber = pdmFlightDtls.OperatingFlightNumber,
                OriginAirportcode = pdmFlightDtls.DepartureAirport,
                SegmentMiles = pdmFlightDtls.BaseMiles,
                TicketDesignator = pdmFlightDtls.TicketDesignator,
                FareBasisCode = pdmFlightDtls.FareBasis

            };
            segmentDetails.Add(segmentDetail);

            PassengerDetails passenger = new()
            {
                PassengerId = Helper.GenerateRandomId(),
                PassengerName = $"{pdmFlightDtls.FirstName} {pdmFlightDtls.LastName}",
                CreatedBy = Common.Constants.Application.WebMyaChannelID,
                CreatedOn = DateTime.UtcNow,
                PhysicalTicketNumber = pdmFlightDtls.TicketNumber,
                ValueToCredit = Convert.ToString(inRuleResponse.PointsAward),
                NumberOfSegments = Convert.ToString(segmentDetails.Count()),
                SegmentDetails = segmentDetails
            };
            passengers.Add(passenger);

            return new TransactionRequest()
            {
                ProductName = productCode.ProductName,
                PartnerName = partnerCode.Name,
                TransactionDocument = new Transaction()
                {
                    TransactionId = vredRequest.TransactionId,
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = Common.Constants.Application.WebMyaChannelID,
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedBy = Common.Constants.Application.WebMyaChannelID,
                    CustomerId = customerCustMS.CustomerId,
                    Comments = inRuleResponse.ProcessingComments,
                    MemberNumber = customerCustMS.LoyaltyNumber,
                    TransactionValue = Convert.ToString(inRuleResponse.PointsAward),
                    PartnerId = partnerCode.RowId,
                    TransactionDate = DateTime.UtcNow,
                    ProductId = productCode.RowId,
                    StatusCode = Common.Constants.Application.TxnProcessingStatus,
                    TransactionSubType = Common.Constants.Application.TxnSubType,
                    TransactionChannel = Common.Constants.Application.WebMyaChannelID,
                    ActivityDate = Convert.ToDateTime(inRuleResponse.FlightDate),
                    TransactionType = Common.Constants.Application.AccrualTransactionType,
                    StatementText = statementText,
                    FlightDetails = new FlightDetails()
                    {
                        OperatingFlightNumber = pdmFlightDtls.OperatingFlightNumber,
                        MarketingFlightNumber = pdmFlightDtls.MarketingFlightNumber,
                        OriginAirportcode = pdmFlightDtls.DepartureAirport,
                        OperatingAirlineAuthNumber = pdmFlightDtls.OperatingAirlineAuthNum,
                        MarketingBookingClass = pdmFlightDtls.VAMarketingBookingClass,
                        DestinationAirport = pdmFlightDtls.ArrivalAirport,
                        BookingReference = pdmFlightDtls.RecordLocatorOperatingCarrier,
                        DepartureDate = pdmFlightDtls.DepartureDate,
                        ExternalReservationNumber = pdmFlightDtls.RecordLocatorOperatingCarrier,
                        Ticketnumber = pdmFlightDtls.TicketNumber,
                        CabinClass = pdmFlightDtls.FlownCabinClass,
                        BookingDate = pdmFlightDtls.TicketIssueDate,
                        BookingClass = pdmFlightDtls.OriginalBookedClass,
                        TravelClass = bookClass,
                        PassengerDetails = passengers,
                        Distance = segmentDistance,
                        RetroFlag = Common.Constants.Application.RetroFlag,
                        MarketingPartnerCode = pdmFlightDtls.MarketingAirlineCode,
                    },
                    CreditDetails = GetCreditDetails(inRuleResponse, partnerCode.PartnerCode)
                }
            };
        }

        private static IEnumerable<CreditDetails> GetCreditDetails(SegmentResponse inRuleResponse, string partnerCode)
        {
            List<CreditDetails> credits = new();

            if (inRuleResponse.BasePoints > 0)
            {
                credits.Add(new CreditDetails
                {
                    CreditItemId = Helper.GenerateRandomId(),
                    CreatedBy = Common.Constants.Application.WebMyaChannelID,
                    CreatedOn = DateTime.UtcNow,
                    CreditValue = Convert.ToString(inRuleResponse.BasePoints),
                    PointType = Common.Constants.Application.MilesType,
                    PointSubType = Common.Constants.Application.BaseMilesType,
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlag,
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedBy = Common.Constants.Application.WebMyaChannelID
                });
            }

            if (inRuleResponse.CabinBonusPoints > 0)
            {
                credits.Add(new CreditDetails
                {
                    CreditItemId = Helper.GenerateRandomId(),
                    CreatedBy = Common.Constants.Application.WebMyaChannelID,
                    CreatedOn = DateTime.UtcNow,
                    CreditValue = Convert.ToString(inRuleResponse.CabinBonusPoints),
                    PointType = Common.Constants.Application.MilesType,
                    PointSubType = Common.Constants.Application.CabinBonusType,
                    QualifyingFlag = Common.Constants.Application.MilesQualifyingFlag,
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedBy = Common.Constants.Application.WebMyaChannelID
                });
            }

            if (inRuleResponse.TierBonusPoints > 0)
            {
                credits.Add(new CreditDetails
                {
                    CreditItemId = Helper.GenerateRandomId(),
                    CreatedBy = Common.Constants.Application.WebMyaChannelID,
                    CreatedOn = DateTime.UtcNow,
                    CreditValue = Convert.ToString(inRuleResponse.TierBonusPoints),
                    PointType = Common.Constants.Application.MilesType,
                    PointSubType = "TierBonus",
                    QualifyingFlag = "N",
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedBy = Common.Constants.Application.WebMyaChannelID
                });
            }

            if (inRuleResponse.TierPointsAward > 0)
            {
                credits.Add(new CreditDetails
                {
                    CreditItemId = Helper.GenerateRandomId(),
                    CreatedBy = Common.Constants.Application.WebMyaChannelID,
                    CreatedOn = DateTime.UtcNow,
                    CreditValue = Convert.ToString(inRuleResponse.TierPointsAward),
                    PointType = "TierPoints",
                    PointSubType = "",
                    QualifyingFlag = "Y",
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedBy = Common.Constants.Application.WebMyaChannelID
                });

                if (string.Equals(partnerCode, Application.VSOperatingAirlineCode, StringComparison.OrdinalIgnoreCase))
                {
                    credits.Add(new CreditDetails
                    {
                        CreditItemId = Helper.GenerateRandomId(),
                        CreatedBy = Common.Constants.Application.WebMyaChannelID,
                        CreatedOn = DateTime.UtcNow,
                        CreditValue = Convert.ToString(inRuleResponse.TierPointsAward),
                        PointType = "VSTierPoints",
                        LastUpdatedOn = DateTime.UtcNow,
                        LastUpdatedBy = Common.Constants.Application.WebMyaChannelID,
                        QualifyingFlag = "Y"
                    });

                    credits.Add(new CreditDetails
                    {
                        CreditItemId = Helper.GenerateRandomId(),
                        CreatedBy = Common.Constants.Application.WebMyaChannelID,
                        CreatedOn = DateTime.UtcNow,
                        CreditValue = "1",
                        PointType = "VSFlownSectors",
                        LastUpdatedOn = DateTime.UtcNow,
                        LastUpdatedBy = Common.Constants.Application.WebMyaChannelID,
                        QualifyingFlag = "N"
                    });
                }
            }

            return credits;
        }

        public static StatementTextrequest GetStatementText(string fcNumber, ProductDetails productDetails, PartnerProfileDetails partnerProfileDetails, VSRetroFlownDetail pdmResponse, string codeShareFlag, string cabinClass)
        {
            return new StatementTextrequest()
            {
                ProductName = productDetails.ProductName,
                PartnerName = partnerProfileDetails.Name,
                PartnerCode = partnerProfileDetails.PartnerCode,
                OriginAirport = pdmResponse.DepartureAirport,
                DestinationAirport = pdmResponse.ArrivalAirport,
                TransactionType = Application.AccrualTransactionType,
                TransactionSubType = Application.TxnSubType,
                MemberNumber = fcNumber,
                VACodeShareFlag = codeShareFlag,
                CabinClass = cabinClass,
                ClassCode = pdmResponse.FlownCabinClass
            };
        }

        public static StatementTextrequest GetStatementTextForSkyFlights(Transaction transaction, SkyFlightsPointsClaimRequest request, ProductDetails productDetails, PartnerProfileDetails partnerProfileDetails, SkyClaimResponse skyResponse, string cabinClass)
        {
            return new StatementTextrequest()
            {
                TransactionType = Application.AccrualTransactionType,
                ProductName = productDetails.ProductName,
                ProductDescription = productDetails.Description,
                PartnerName = partnerProfileDetails.Name,
                ActivityNumber = transaction.ActivityNumber,
                PartnerCode = partnerProfileDetails.PartnerCode,
                ExternalReservationNumber = request?.PnrNumber,
                Quantity = transaction.FlightDetails?.Quantity,
                OriginAirport = skyResponse.FlightSegmentOrigin,
                DestinationAirport = skyResponse.FlightSegmentDestination,
                TransactionSubType = Application.TxnSubType,
                MemberNumber = request.FcNumber,
                VACodeShareFlag = "Y",
                CabinClass = cabinClass,
                ClassCode = skyResponse.CabinClassCode,
                MemberMediaCode = string.Empty,
                MarketingCarrierCode = skyResponse.MarketingCarrierCode
            };
        }

        public static TransactionRequest MapTransactionReqforSkyFlights(CustomerCustMS customerCustMS, SegmentResponse inRuleResponse, SkyClaimResponse skyFlightDtls, PartnerProfileDetails partnerCode, string productCode, string segmentDistance, SkyFlightsPointsClaimRequest request, string cabinClass)
        {
            List<PassengerDetails> passengers = new();
            List<SegmentDetails> segmentDetails = new();

            SegmentDetails segmentDetail = new()
            {
                SegmentId = Helper.GenerateRandomId(),
                BookingClass = skyFlightDtls.CabinClassCode,
                DepartureDate = DateTime.ParseExact(skyFlightDtls.ScheduledFlightSegmentDepartureDate, "yyyyMMdd", null),
                DestinationAirportcode = skyFlightDtls.FlightSegmentDestination,
                MarketingCarrierCode = skyFlightDtls.MarketingCarrierCode,
                MarketingFlightNumber = skyFlightDtls.MarketingFlightNumber,
                PartnerCode = partnerCode.PartnerCode,
                OperatingFlightNumber = skyFlightDtls.OperatingFlightNumber,
                OriginAirportcode = skyFlightDtls.FlightSegmentOrigin,
                SegmentMiles = segmentDistance,
                CreatedBy = Application.WebMyaChannelID,
                CreatedOn = DateTime.UtcNow,
                LastUpdatedBy = Application.WebMyaChannelID,
                LastUpdatedOn = DateTime.UtcNow,
            };
            segmentDetails.Add(segmentDetail);

            PassengerDetails passenger = new()
            {
                PassengerId = Helper.GenerateRandomId(),
                PassengerName = $"{customerCustMS.FirstName} {customerCustMS.LastName}",
                Status = customerCustMS.Status,
                CreatedBy = Common.Constants.Application.WebMyaChannelID,
                CreatedOn = DateTime.UtcNow,
                LastUpdatedBy = Application.WebMyaChannelID,
                LastUpdatedOn = DateTime.UtcNow,
                PhysicalTicketNumber = request.TicketNumber,
                ValueToCredit = Convert.ToString(inRuleResponse.PointsAward),
                NumberOfSegments = Convert.ToString(segmentDetails.Count),
                SegmentDetails = segmentDetails
            };
            passengers.Add(passenger);


            return new TransactionRequest()
            {
                TransactionDocument = new Transaction()
                {
                    TransactionId = Helper.GenerateRandomId(),
                    CreatedOn = DateTime.UtcNow,
                    CreatedBy = Common.Constants.Application.WebMyaChannelID,
                    LastUpdatedOn = DateTime.UtcNow,
                    LastUpdatedBy = Common.Constants.Application.WebMyaChannelID,
                    CustomerId = customerCustMS.CustomerId,
                    Comments = inRuleResponse.ProcessingComments,
                    ActivityNumber = inRuleResponse.FlightNumberOperating,
                    MemberNumber = customerCustMS.LoyaltyNumber,
                    TransactionValue = Convert.ToString(inRuleResponse.PointsAward),
                    PartnerId = partnerCode.RowId,
                    TransactionDate = DateTime.UtcNow,
                    ProductId = productCode,
                    StatusCode = Common.Constants.Application.TxnProcessingStatus,
                    TransactionSubType = Common.Constants.Application.TxnSubType,
                    TransactionChannel = Common.Constants.Application.WebMyaChannelID,
                    ActivityDate = Convert.ToDateTime(request.DepartureDate),
                    TierPointsToCredit = Convert.ToString(inRuleResponse.TierPointsAward),
                    TransactionType = Common.Constants.Application.AccrualTransactionType,

                    FlightDetails = new FlightDetails()
                    {
                        Distance = segmentDistance,
                        BookingClass = skyFlightDtls.OperatingCarierBookingClass,
                        OperatingFlightNumber = skyFlightDtls.OperatingFlightNumber,
                        MarketingFlightNumber = skyFlightDtls.MarketingFlightNumber,
                        MarketingPartnerCode = skyFlightDtls.MarketingCarrierCode,
                        OriginAirportcode = request.DepartureFrom,
                        MarketingBookingClass = skyFlightDtls.MarketingCarrierBookingClass,
                        DestinationAirport = request.ArrivingAt,
                        BookingReference = request?.PnrNumber,
                        DepartureDate = DateTime.Parse(request.DepartureDate),
                        ExternalReservationNumber = request?.PnrNumber,
                        Ticketnumber = request.TicketNumber,
                        BookingDate = DateTime.ParseExact(skyFlightDtls.ScheduledFlightSegmentDepartureDate, "yyyyMMdd", null),
                        CabinClass = skyFlightDtls.CabinClassCode,
                        PNRName = customerCustMS.LastName + "/" + customerCustMS.FirstName,
                        PassengerDetails = passengers,
                        RetroFlag = Common.Constants.Application.RetroFlag,
                        RetroOverrideFlag = Common.Constants.Application.RetroOverrideFlag,
                        Quantity = Convert.ToString(passengers.Count()),
                        TravelClass = cabinClass,
                        Seatnumber = request?.SeatNumber,
                        VAPostingTypeCode = Common.Constants.Application.VAPostingTypeCode,
                        OperatingAirlineAuthNumber = skyFlightDtls.UniqueIdentificationNumber
                    },
                    CreditDetails = GetCreditDetails(inRuleResponse, partnerCode.PartnerCode)
                }
            };
        }

        public static MissingPointsResponse<ClaimFlightSegmentDetails> GetSkyMissingPointsClaimResponse(SkyClaimResponse request, string airline, SegmentResponse inRuleResponse, AirportInfo originAirportDetails, AirportInfo destinationAirportDetails)
        {
            List<ClaimFlightSegmentDetails> claimFlightSegmentDetails = new();

            var segmentDetail = new ClaimFlightSegmentDetails
            {
                OperatingAirlineName = airline,
                OperatingAirlineCode = request.OperatingCarrierCode,
                FlightNumber = $"{request.OperatingCarrierCode}{request.OperatingFlightNumber}",
                DateOfFlight = DateTime.ParseExact(request.ScheduledFlightSegmentDepartureDate, "yyyyMMdd", null).ToString(Application.ISODateTimeFormat),
                OriginAirportCode = request.FlightSegmentOrigin,
                OriginAirportName = originAirportDetails.AirportName,
                OriginAirportCountry = originAirportDetails.CountryName,
                DestinationAirportCode = request.FlightSegmentDestination,
                DestinationAirportName = destinationAirportDetails.AirportName,
                DestinationAirportCountry = destinationAirportDetails.CountryName,
                VirginPoints = Convert.ToString(inRuleResponse.PointsAward),
                BonusPoints = Convert.ToString(inRuleResponse?.TierBonusPoints + inRuleResponse?.CabinBonusPoints),
                TierPoints = Convert.ToString(inRuleResponse.TierPointsAward),
            };

            claimFlightSegmentDetails.Add(segmentDetail);

            return new MissingPointsResponse<ClaimFlightSegmentDetails>
            {
                TotalBonusPoints = Convert.ToString(inRuleResponse?.TierBonusPoints + inRuleResponse?.CabinBonusPoints),
                TotalTierPoints = Convert.ToString(inRuleResponse.TierPointsAward),
                TotalVirginPoints = Convert.ToString(inRuleResponse.PointsAward),
                segmentDetails = claimFlightSegmentDetails,
                ErrorResponse = null
            };
        }

        public static CreateTransactionRequest CreateTransactionVRedMapperForSkyFlights(CustomerCustMS custMS, ProductDetails productDetails, SegmentResponse inRuleResponse, PartnerProfileDetails partnerProfileDetails, StatementTextResponse statementTextResponse)
        {
            return new CreateTransactionRequest()
            {
                CustomerId = custMS.CustomerId,
                MemberNumber = custMS.LoyaltyNumber,
                TransactionSource = Application.WebMyaChannelID,
                Comments = inRuleResponse.ProcessingComments,
                PartnerCode = partnerProfileDetails.PartnerCode,
                PartnerName = partnerProfileDetails.Name,
                PartnerCategory = partnerProfileDetails.PartnerSubType,
                ProductName = productDetails.ProductName,
                ProductDescription = productDetails.Description,
                ProductCategory = productDetails.ProductCategory,
                StatementText = statementTextResponse.StatementText,
                TransactionSubType = Application.TxnSubType,
                TransactionId = Helper.GenerateRandomId(),
                TransactionChannel = Application.TransactionChannel,
                ActivityDate = Convert.ToDateTime(inRuleResponse.FlightDate).ToString(Application.ISODateFormat),
                ActivityType = Application.AccrualTransactionType,
                Status = Application.TxnProcessingStatus,
                Points = inRuleResponse.PointsAward,
                CreatedBy = Application.WebMyaChannelID,
            };

        }
    }
}
