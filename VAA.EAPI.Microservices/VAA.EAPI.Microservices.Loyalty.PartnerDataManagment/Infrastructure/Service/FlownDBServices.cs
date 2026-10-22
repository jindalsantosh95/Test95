using Dapper;
using Microsoft.Extensions.Logging;
using System.Data;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.FlownPassenger;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Domain.Entities.RetroCredit;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.DataModels;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Models.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Mapper;
using VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Repositories.FlownDB.Interface;
using System.Globalization;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Infrastructure.Service
{
    public class FlownDBServices : IFlownDbServices
    {
        private readonly FlownDbContext _flownDataContext;

        private readonly ILogger<FlownDBServices> _logger;

        private readonly IDbConnection _dbConnection;

        public FlownDBServices(FlownDbContext flownDataContext, ILogger<FlownDBServices> logger)
        {
            _flownDataContext = flownDataContext ?? throw new ArgumentNullException(nameof(flownDataContext));

            _dbConnection = _flownDataContext.CreateConnection();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        public async Task<bool> UpdateFlownRecordWithAutoAccrualFeedbackAsync(DistanceBasedAccrualFeedbackPayload message)
        {
            if (message is null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var uniqueIdentificationNumber = message.UniqueIdentification?.UniqueIdentificationNumber;
            var postingStatus = message.PostingDetails?.PostingStatus;
            if (uniqueIdentificationNumber.IsNullOrWhiteSpace() || postingStatus.IsNullOrWhiteSpace())
            {
                throw new ArgumentException("A valid unique identification number and posting status are required.", nameof(message));
            }

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@UniqueIdentificationNumber", uniqueIdentificationNumber, DbType.String);
                parameters.Add(
                    "@ProcessingStatus",
                    postingStatus.Is(Constants.Application.SuccessfulPostingStatus)
                        ? Constants.Application.AllocatedProcessingStatus
                        : Constants.Application.FreeProcessingStatus,
                    DbType.String);

                var impactedRows = await _dbConnection.ExecuteAsync(Constants.Application.UpdateAutoAccrualProcessingStatus, parameters, commandType: CommandType.StoredProcedure);

                return impactedRows > 0;
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "FlownDb auto-accrual feedback update failed for UIN {Uin}", uniqueIdentificationNumber);

                throw;
            }
        }
        public async Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualAsync(AutoAccrualPayload autoAccrualRequest, string ffpCarrier, CancellationToken cancellationToken)
        {
            try
            {
                DynamicParameters parameters = new();

                parameters.Add("@PartnerCode", ffpCarrier, DbType.String);
                parameters.Add("@FlightCarrier", autoAccrualRequest.FlightCarrier, DbType.String);
                parameters.Add("@FlightNumber", autoAccrualRequest.FlightNumber, DbType.String);
                parameters.Add("@DepartureDate", DateTime.ParseExact(autoAccrualRequest.DepartureDate, "yyyy-MM-dd", CultureInfo.InvariantCulture), DbType.Date);
                parameters.Add("@DepartureAirport", autoAccrualRequest.DepartureAirport, DbType.String);
                parameters.Add("@ArrivalAirport", autoAccrualRequest.ArrivalAirport, DbType.String);

                var command = new CommandDefinition(Constants.Application.GetFlownPassengersForAutoAccrual, parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                var readerData = await _dbConnection.ExecuteReaderAsync(command);
                var objDT = new DataTable();
                objDT.Load(readerData);

                return objDT.AsEnumerable()
                    .Select(item => new AutoAccrualFlownDetails
                    {
                        FFPCarrierCode = item["FD_LOY_PRG_AIRLINE_CD"]?.ToString(),
                        FFPAccountNumber = item["FD_MEMBER_NUMBER"]?.ToString(),
                        LastName = item["FD_LAST_NAME"]?.ToString(),
                        FirstName = item["FD_PNR_NAME"]?.ToString(),
                        MarketingAirlineCode = item["FD_MKT_AIRLINE_CD"]?.ToString(),
                        MarketingFlightNum = item["FD_MKT_FLIGHT_NUM"]?.ToString(),
                        OperatingAirlineCode = item["FD_OPER_AIRLINE_CD"]?.ToString(),
                        OperatingFlightNum = item["FD_OPER_FLIGHT_NUM"]?.ToString(),
                        DepartureDate = item["FD_DEPARTURE_DT"]?.ToString(),
                        OriginPort = item["FD_ORIGIN_PORT"]?.ToString(),
                        DestinationPort = item["FD_DESTINATION_PORT"]?.ToString(),
                        FlownCabinClass = item["FD_FLOWN_CABIN_CLASS"]?.ToString(),
                        MarketingCarrierBookingClass = item["FD_MKT_BKG_CLASS"]?.ToString(),
                        OperatingCarrierBookingClass = item["FD_BKG_CLASS"]?.ToString(),
                        OperatingRecordLocator = item["FD_REC_LOCATOR_OPER"]?.ToString(),
                        CouponNumber = item["FD_COUPON_NUM"]?.ToString(),
                        SeatNumber = item["FD_SEAT_NUM"]?.ToString(),
                        TicketNumber = item["FD_TICKET_NUM"]?.ToString(),
                        OperatingAirAuthNumber = item["FD_OPER_AIR_AUTH_NUM"]?.ToString(),
                        ProcessingStatus = item["PROCESSING_STATUS"]?.ToString(),
                        FdNoShow = item["FD_NO_SHOW"]?.ToString(),
                        SentToPartner = item["SENT_TO_PARTNER"]?.ToString(),
                    })
                    .ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "FlownDb-GetFlownDetailsForAutoAccrualAsync failed due to Exception {ErrorMessage}", ex.Message);

                throw;
            }
        }

        public async Task<RetroCreditFlownDetails> GetUniquePassengerFlownDetailsForRetroCreditAsync(RetroCreditRequest retroCreditRequest)
        {

            RetroCreditFlownDetails objRetroCreditResponse;
            try
            {
                var ObjeDT = new DataTable();
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@PassengerLastName", retroCreditRequest.PassengerLastName, DbType.String);
                parameters.Add("@PassengerFirstName", retroCreditRequest.PassengerFirstName, DbType.String);
                parameters.Add("@OperatingCarrierCode", retroCreditRequest.OperatingCarrierCode, DbType.String);
                parameters.Add("@OperatingFlightNumber", retroCreditRequest.OperatingFlightNumber, DbType.String);
                parameters.Add("@ScheduledFlightSegmentDepartureDate", DateTime.ParseExact(retroCreditRequest.ScheduledFlightSegmentDepartureDate, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyy-MM-dd"), DbType.String);
                parameters.Add("@FlightSegmentOrigin", retroCreditRequest.FlightSegmentOrigin, DbType.String);
                parameters.Add("@FlightSegmentDestination", retroCreditRequest.FlightSegmentDestination, DbType.String);
                parameters.Add("@EticketNumber", retroCreditRequest.EticketNumber, DbType.String);
                //Optional Data
                parameters.Add("@SeatNumber", retroCreditRequest.SeatNumber, DbType.String);
                parameters.Add("@SequenceNumber", retroCreditRequest.SequenceNumber, DbType.String);
                parameters.Add("@OperatingCarrierRecordLocator", retroCreditRequest.OperatingCarrierRecordLocator, DbType.String);

                var readerdata = await _dbConnection.ExecuteReaderAsync(Constants.Application.GetPassengerDetails, parameters, null, null, CommandType.StoredProcedure);

                ObjeDT.Load(readerdata);

                var ErrorData = ObjeDT.Rows != null && ObjeDT.Rows.Count > 0 ? ObjeDT?.Rows[0]?.ItemArray[0]?.ToString() : "Data Not Found";

                if (ErrorData == "Data Not Found" || ErrorData == "ERROR CODE 666")
                {
                    return new RetroCreditFlownDetails { Error = ErrorData };
                }
                else
                {
                    var item = ObjeDT.Rows[0];
                    return new RetroCreditFlownDetails
                    {
                        FFPCarrierCode = item["FD_LOY_PRG_AIRLINE_CD"].ToString(),
                        FFPAccountNumber = item["FD_MEMBER_NUMBER"].ToString(),
                        LastName = item["FD_LAST_NAME"].ToString(),
                        FirstName = item["FD_FIRST_NAME"].ToString(),
                        MarketingAirlineCode = item["FD_MKT_AIRLINE_CD"].ToString(),
                        MarketingFlightNum = item["FD_MKT_FLIGHT_NUM"].ToString(),
                        OperatingAirlineCode = item["FD_OPER_AIRLINE_CD"].ToString(),
                        OperatingFlightNum = item["FD_OPER_FLIGHT_NUM"].ToString(),
                        DepartureDate = ((DateTime)item["FD_DEPARTURE_DT"]).ToString("yyyyMMdd"),
                        FlownCabinClass = item["FD_FLOWN_CABIN_CLASS"].ToString(),
                        MarketingCarrierBookingClass = item["FD_MKT_BKG_CLASS"].ToString(),
                        OperatingCarierBookingClass = item["FD_BKG_CLASS"].ToString(),
                        OrginPort = item["FD_ORIGIN_PORT"].ToString(),
                        DestinationPort = item["FD_DESTINATION_PORT"].ToString(),
                        TicketNumber = item["FD_TICKET_NUM"].ToString(),
                        CoupanNumber = item["FD_COUPON_NUM"].ToString(),
                        OperatingAirAuthNumber = item["FD_OPER_AIR_AUTH_NUM"].ToString(),
                        ProcessingStatus = item["PROCESSING_STATUS"].ToString(),
                        FdNoShow = item["FD_NO_SHOW"].ToString(),
                        PostingDate = ((DateTime)item["FD_RETRO_COMPLETED_DT"]).ToString("yyyyMMdd"),
                    };
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlownDb-GetUniquePassengerFlownDetailsWithAutoCreditAsync failed due to Exception");
                throw;
            }
        }

        public async Task<bool> ResetPendingFeedbackFlownDetailsAsync(string Days)
        {

            bool objRetroFeedbackResponse = false;

            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@Days", Convert.ToInt32(Days), DbType.Int32);
                var impactedRows = await _dbConnection.ExecuteAsync(Constants.Application.ResetPendingFeedBackAutoRetro, parameters, null, null, CommandType.StoredProcedure);
                if (impactedRows > 0)
                {
                    objRetroFeedbackResponse = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlownDb-ResetPendingFeedbackAutoRetroAsync failed due to Exception");
                throw;
            }
            return objRetroFeedbackResponse;
        }

        public async Task<bool> UpdateFlownDetailsForEligibleRetroAsync(string uniqueIdentificationNumber, RetroCreditRequest retroCreditRequest)
        {

            bool objRetroFeedbackResponse = false;

            try
            {
                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UniqueIdentificationNumber", uniqueIdentificationNumber, DbType.String);
                parameters.Add("@FD_MEMBER_NUMBER", retroCreditRequest.FFPAccountNumber, DbType.String);
                parameters.Add("@FD_LOY_PRG_AIRLINE_CD", retroCreditRequest.FFPCarrierCode, DbType.String);
                var impactedRows = await _dbConnection.ExecuteAsync(Constants.Application.UpdateFlownDetailsForEligibleRetro, parameters, null, null, CommandType.StoredProcedure);
                if (impactedRows > 0)
                {
                    objRetroFeedbackResponse = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlownDb-UpdateFlownDetailsForEligibleRetroAsync failed due to Exception");
                throw;
            }
            return objRetroFeedbackResponse;
        }

        public async Task<bool> UpdateFlownDetailsWithFeedbackAsync(RetroFeedbackRequest retroFeedbackRequest)
        {

            bool objRetroFeedbackResponse = false;

            try
            {

                var postingDate = retroFeedbackRequest.PostingDate.IsNullOrWhiteSpace()
                    ? DateTime.UtcNow.ToString("yyyyMMdd", CultureInfo.InvariantCulture)
                    : DateTime.ParseExact(retroFeedbackRequest.PostingDate, "yyyyMMdd", CultureInfo.InvariantCulture).ToString("yyyyMMdd", CultureInfo.InvariantCulture);

                DynamicParameters parameters = new DynamicParameters();
                parameters.Add("@UniqueIdentificationNumber", retroFeedbackRequest.UniqueIdentificationNumber, DbType.String);
                parameters.Add("@BaseMiles", retroFeedbackRequest.BaseMiles, DbType.String);
                parameters.Add("@ClassOfServiceBonusMiles", retroFeedbackRequest.ClassOfServiceBonusMiles, DbType.String);
                parameters.Add("@PostingStatus", retroFeedbackRequest.PostingStatus, DbType.String);
                parameters.Add("@PostingDate", postingDate, DbType.String);
                parameters.Add("@PromotionalBonusMiles", retroFeedbackRequest.PromotionalBonusMiles, DbType.String);
                parameters.Add("@EliteTierBonusMiles", retroFeedbackRequest.EliteTierBonusMiles, DbType.String);
                var impactedRows = await _dbConnection.ExecuteAsync(Constants.Application.UpdateFeedbackDetails, parameters, null, null, CommandType.StoredProcedure);
                if (impactedRows > 0)
                {
                    objRetroFeedbackResponse = true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"FlownDb-UpdateFlownDetailsWithFeedbackAsync failed due to Exception");
                throw;
            }
            return objRetroFeedbackResponse;
        }

        public async Task<bool> UpdateTicketStatus(int id, string ticketNumber, string claimStatus, string? memberNumber, string? loyaltyProgrammeCode, string? postingTypeCode)
        {
            DynamicParameters parameters = new();
            parameters.Add("@Id", id, DbType.Int32);
            parameters.Add("@TicketNumber", ticketNumber, DbType.String);
            parameters.Add("@ClaimStatus", claimStatus, DbType.String);
            if (!memberNumber.IsNullOrWhiteSpace())
            {
                parameters.Add("@MemberNumber", memberNumber, DbType.String);
            }

            if (!loyaltyProgrammeCode.IsNullOrWhiteSpace())
            {
                parameters.Add("@LoyaltyProgrammeCode", loyaltyProgrammeCode, DbType.String);
            }

            if (!postingTypeCode.IsNullOrWhiteSpace())
            {
                parameters.Add("@PostingTypeCode", postingTypeCode, DbType.String);
            }

            var impactedRows = await _dbConnection.ExecuteAsync(Constants.Application.UpdateFlownSegmentStatus, parameters, null, null, CommandType.StoredProcedure);

            return impactedRows > 0;
        }

        public async Task<IReadOnlyCollection<FlownPassengerRequest>> InsertFlownPassengerDetailsAsync(IEnumerable<FlownPassengerRequest> flownPassengerRequest, CancellationToken cancellationToken = default)
        {

            if (flownPassengerRequest is null) throw new ArgumentNullException(nameof(flownPassengerRequest));

            var flownDetailsList = flownPassengerRequest.ToList();
            if (flownDetailsList.Count == 0) return Array.Empty<FlownPassengerRequest>();

            var successfullyInsertedPassengers = new List<FlownPassengerRequest>();

            foreach (var flownDetails in flownDetailsList)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    var command = new CommandDefinition(
                        Constants.Application.InsertFlownPassengersDetail,
                        FlownPassengerParameterMapper.Create(flownDetails),
                        commandType: CommandType.StoredProcedure,
                        cancellationToken: cancellationToken);
                    await _dbConnection.ExecuteAsync(command);
                    successfullyInsertedPassengers.Add(flownDetails);

                }
                catch (MissingRequiredFlownPassengerFieldException ex)
                {
                    _logger.LogWarning("Skipping flown passenger insert because required column {ColumnName} is missing. OrderId: {OrderId}", ex.ColumnName, flownDetails?.OrderId);
                    continue;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Flown passenger insert failed. OrderId: {OrderId}", flownDetails?.OrderId);
                    throw;
                }
            }

            return successfullyInsertedPassengers;
        }

        public async Task<IEnumerable<AutoAccrualFlownDetails>> GetFlownDetailsForAutoAccrualFallbackAsync(AutoAccrualFallbackCriteria criteria, CancellationToken cancellationToken)
        {
            if (criteria is null)
            {
                throw new ArgumentNullException(nameof(criteria));
            }

            try
            {
                var parameters = new DynamicParameters();
                parameters.Add("@ProcessingStatus", criteria.ProcessingStatus, DbType.String);
                parameters.Add("@NoShowIndicator", criteria.NoShowIndicator, DbType.String);
                parameters.Add("@SentToPartnerIndicator", criteria.SentToPartnerIndicator, DbType.String);
                parameters.Add("@DepartureDateBefore", criteria.DepartureDate, DbType.Date);
                parameters.Add("@FfpCarrierCodes", criteria.FfpCarrierCodes, DbType.String);

                var command = new CommandDefinition(Constants.Application.GetFlownPassengersForAutoAccrualFallback, parameters, commandType: CommandType.StoredProcedure, cancellationToken: cancellationToken);
                var readerData = await _dbConnection.ExecuteReaderAsync(command);
                var dataTable = new DataTable();
                dataTable.Load(readerData);

                return dataTable.AsEnumerable()
                    .Select(item => new AutoAccrualFlownDetails
                    {
                        FFPCarrierCode = item["FD_LOY_PRG_AIRLINE_CD"]?.ToString(),
                        FFPAccountNumber = item["FD_MEMBER_NUMBER"]?.ToString(),
                        LastName = item["FD_LAST_NAME"]?.ToString(),
                        FirstName = item["FD_FIRST_NAME"]?.ToString(),
                        PnrName = item["FD_PNR_NAME"]?.ToString(),
                        MarketingAirlineCode = item["FD_MKT_AIRLINE_CD"]?.ToString(),
                        MarketingFlightNum = item["FD_MKT_FLIGHT_NUM"]?.ToString(),
                        OperatingAirlineCode = item["FD_OPER_AIRLINE_CD"]?.ToString(),
                        OperatingFlightNum = item["FD_OPER_FLIGHT_NUM"]?.ToString(),
                        DepartureDate = item["FD_DEPARTURE_DT"]?.ToString(),
                        OriginPort = item["FD_ORIGIN_PORT"]?.ToString(),
                        DestinationPort = item["FD_DESTINATION_PORT"]?.ToString(),
                        FlownCabinClass = item["FD_FLOWN_CABIN_CLASS"]?.ToString(),
                        MarketingCarrierBookingClass = item["FD_MKT_BKG_CLASS"]?.ToString(),
                        OperatingCarrierBookingClass = item["FD_BKG_CLASS"]?.ToString(),
                        OperatingRecordLocator = item["FD_REC_LOCATOR_OPER"]?.ToString(),
                        CouponNumber = item["FD_COUPON_NUM"]?.ToString(),
                        SeatNumber = item["FD_SEAT_NUM"]?.ToString(),
                        TicketNumber = item["FD_TICKET_NUM"]?.ToString(),
                        OperatingAirAuthNumber = item["FD_OPER_AIR_AUTH_NUM"]?.ToString(),
                        ProcessingStatus = item["PROCESSING_STATUS"]?.ToString(),
                        FdNoShow = item["FD_NO_SHOW"]?.ToString(),
                        SentToPartner = item["SENT_TO_PARTNER"]?.ToString(),
                    })
                    .ToList();
            }
            catch (Exception exception)
            {
                _logger.LogError(exception, "FlownDb auto-accrual fallback lookup failed.");

                throw;
            }
        }

    }
}





