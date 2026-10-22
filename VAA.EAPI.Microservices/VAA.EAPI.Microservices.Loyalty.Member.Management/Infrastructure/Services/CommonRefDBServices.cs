using CSharpFunctionalExtensions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Serialisation;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.AutoAccrual;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BirthdayBonus;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.BookingClassRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.MemberAccountDetails;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Domain.Entities.PartnersRetrieval;
using VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services.Interface;

namespace VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Services
{
    public class CommonRefDBServices : ICommonRefDBServices
    {
        private readonly CommonReferenceDataContext _commonReferenceDataContext;
        private readonly ILogger<CommonRefDBServices> _logger;
        public CommonRefDBServices(CommonReferenceDataContext commonReferenceDataContext, ILogger<CommonRefDBServices> logger)
        {
            _commonReferenceDataContext = commonReferenceDataContext ?? throw new ArgumentNullException(nameof(commonReferenceDataContext));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<int> GetAutoAccrualValidationCount(string procedure, AutoAccrualRequest request, System.Threading.CancellationToken cancellationToken)
        {
            try
            {
                var date = DateTime.ParseExact(request.ScheduledDepartureDate, Constants.Application.AutoAccrualActivityDateFormat, CultureInfo.InvariantCulture);
                SqlParameter[] parameters;

                if (procedure == Constants.Application.GetMktClassValidity)
                {
                    parameters = new[]
                    {
                        P("@ACTIVITYDATE", date, SqlDbType.Date),
                        P("@OPERATINGAIRLINECODE", request.FlightDetails.OperatingFlight.OperatingCarrierCode),
                        P("@MKTAIRLINECODE", request.FlightDetails.MarketingFlight.MarketingCarrierCode),
                        P("@CREDITCLASS", request.BookingDetails.OperatingCarrierBookingClass),
                        P("@PARTNERCODE", request.Producer)
                    };
                }
                else if (procedure == Constants.Application.GetAirlineActivityDateValidity)
                {
                    parameters = new[]
                    {
                        P("@ACTIVITYDATE", date, SqlDbType.Date),
                        P("@BOOKINGCLASS", request.BookingDetails.OperatingCarrierBookingClass),
                        P("@PARTNERCODE", request.Producer)
                    };
                }
                else
                {
                    parameters = new[]
                    {
                        P("@ORIGINAIRPORTCODE", request.FlightSegment.Origin),
                        P("@DESTINATIONAIRPORTCODE", request.FlightSegment.Destination),
                        P("@ACTIVITYDATE", date, SqlDbType.Date),
                        P("@CREDITCLASS", request.BookingDetails.OperatingCarrierBookingClass),
                        P("@PARTNERCODE", request.Producer),
                        P("@OPERATINGAIRLINECODE", request.FlightDetails.OperatingFlight.OperatingCarrierCode),
                        P("@MKTAIRLINECODE", request.FlightDetails.MarketingFlight.MarketingCarrierCode)
                    };
                }

                var results = await _commonReferenceDataContext.AutoAccrualValidationResults
                    .FromSqlRaw(procedure, parameters)
                    .ToListAsync(cancellationToken);
                var resultCount = results.FirstOrDefault()?.ResultCount ?? 0;
                return resultCount;
            }
            catch (SqlException ex)
            {
                _logger.LogError(ex, "SQL exception while executing auto-accrual reference procedure {Procedure}", procedure);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Exception while executing auto-accrual reference procedure {Procedure}", procedure);
                throw;
            }
        }

        private static SqlParameter P(string name, object value, SqlDbType type = SqlDbType.VarChar) => new(name, type) { Value = value ?? DBNull.Value };

        public async Task<VAA.EAPI.Microservices.Loyalty.Member.Management.Infrastructure.Models.AutoAccrualPartnerTerms> GetAutoAccrualPartnerTerms(string partnerCode, CancellationToken cancellationToken)
        {
            var result = await _commonReferenceDataContext.AutoAccrualPartnerTerms.FromSqlRaw(Constants.Application.GetAutoAccrualPartnerTerms, P("@PARTNERCODE", partnerCode)).ToListAsync(cancellationToken);
            return result.FirstOrDefault();
        }
        public async Task<Result<Response<MemberAccountReferenceData>>> GetMemberAccountReferenceData()
        {
            MemberAccountReferenceData referenceData = _commonReferenceDataContext.MemberAccountReferenceData.FromSqlRaw(Constants.Application.GetMemberAccountReferenceDatas).AsEnumerable().FirstOrDefault();
            if (referenceData != null)
            {
                _logger.LogInformation("Fecth Member Account Reference Data from Reference DB is Success");
                return new Response<MemberAccountReferenceData>
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = referenceData
                };
            }
            else
            {
                _logger.LogInformation("Fecth Member Account Reference Data from Reference DB is Failed");
                return Result.Failure<Response<MemberAccountReferenceData>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        public async Task<Result<Response<PartnersResponse>>> GetPartnersReferenceData()
        {
            var partnersList = await _commonReferenceDataContext.PartnerProfile.FromSqlRaw(Constants.Application.GetPartnersData).ToListAsync();

            if (partnersList != null)
            {
                PartnersResponse partnersResponse = new PartnersResponse();
                partnersResponse.Partners = new List<PartnerProfile>();

                foreach (var partner in partnersList)
                {
                    partnersResponse.Partners.Add(new PartnerProfile()
                    {
                        PartnerId = partner.PartnerId,
                        Name = partner.PartnerName,
                        PartnerCode = partner.PartnerCode,
                        PartnerType = partner.PartnerType,
                        PartnerSubtype = partner.PartnerSubtype,
                        Active = partner.Active,
                        CodeShareFlag = partner.CodeShareFlag,
                        ExceptionBookingClassFlag = partner.ExceptionBookingClassFlag,
                        AutomatedRetroPartner = partner.AutomatedRetroPartner,
                        MaxTxnLimitForPoints = partner.MaxTransactionLimit,
                        PartnerStartDate = partner.PartnerStartDate,
                        PartnerEndDate = partner.PartnerEndDate
                    });
                }

                _logger.LogInformation("Fecth Partners from Reference DB is Success");
                return new Response<PartnersResponse>
                {
                    Status = Status.OK,
                    IgnoreNullValues = false,
                    Value = partnersResponse
                };
            }
            else
            {
                _logger.LogInformation("Fecth Partners from Reference DB is Failed");
                return Result.Failure<Response<PartnersResponse>>(Json.Serialize(ErrorMapper.MapTechnicalExceptionToEAPIException(EAPI.Infrastructure.Constants.Constants.TechnicalError, Guid.NewGuid().ToString(), EAPI.Infrastructure.Constants.Constants.GlobalExceptionFilePath)));
            }
        }

        public async Task<Product> GetProductReferenceDataByName(string ProductName)
        {
            string ProductDetails = Constants.Application.GetProductReferenceData + "'" + ProductName + "'";
            var product = await _commonReferenceDataContext.Products.FromSqlRaw(ProductDetails).ToListAsync();

            return new Product
            {
                ProductId = product.FirstOrDefault().ProductId,
                ProductName = product.FirstOrDefault().ProductName,
            };
        }

        public BookingDetailsResult ProcessBookingClassData(PreValidationPartnerData request, string bookingClass, string flownClass, DateTime activityDate, string listFareValue, string listTicketValue, string listPaxValue)
        {
            var isValid = true;

            // Check ListOfFareValues
            if (request.ListOfValues.Any(l => (l.Key == "VA_FARE_BASIS" && l.Value == listFareValue) || (l.Key == "VA_TICKET_DESGNATOR" && l.Value == listTicketValue) || (l.Key == "VA_PAX_TYPE" && l.Value == listPaxValue)))
            {
                _logger.LogInformation("Booking is not eligible for accrual");
                isValid = false;
            }

            // Check PartnerTerms
            var partnerTerm = request.PartnerTerms.FirstOrDefault();
            if (partnerTerm != null)
            {
                if ((activityDate - DateTime.UtcNow).TotalDays > partnerTerm.RetroClaimPeriod)
                {
                    _logger.LogInformation("Activity Outside Retrospective Claim Period");
                    isValid = false;
                }

                if (activityDate < partnerTerm.AgreementStartDate || (partnerTerm.AgreementEndDate.HasValue && activityDate > partnerTerm.AgreementEndDate.Value))
                {
                    _logger.LogInformation("Travel Date Prior to Partner Join Date");
                    isValid = false;
                }

                if ((DateTime.UtcNow - activityDate).TotalDays <= partnerTerm.RetroClaimPeriod && (DateTime.UtcNow - activityDate).TotalDays <= partnerTerm.RetroClaimMinimumPeriod)
                {
                    _logger.LogInformation("Minimum Posting Date, Please wait few more days");
                    isValid = false;
                }
            }

            // Check PartnerBookingClass
            var validBookingClass = request.PartnerBookingClasses.First(p => p.RedemptionClassIndicator.ToUpper() != "Y" && (p.BookingClass.ToUpper() == bookingClass.ToUpper() || p.BookingClass.ToUpper() == flownClass.ToUpper()) && p.StartDate <= activityDate && (p.EndDate == null || p.EndDate >= activityDate));

            if (validBookingClass == null)
            {
                _logger.LogInformation("Operating Booking Class Not Found");
                isValid = false;
            }

            // Check Distance
            if (request.Distance == null)
            {
                _logger.LogInformation("Bookable Activity not found");
                isValid = false;
            }

            if (isValid)
            {
                return new BookingDetailsResult
                {
                    CabinClass = validBookingClass.CabinClass,
                    BookClass = validBookingClass.BookingClass.ToUpper() == bookingClass.ToUpper() ? "BOOKINGCLASS" : "FLOWNCLASS",
                    OriginCountryCode = request.Distance.OriginCountryCode,
                    DestinationCountryCode = request.Distance.DestinationCountryCode,
                    DistanceInMiles = request.Distance.DistanceInMiles
                };
            }
            else
            {
                return new BookingDetailsResult();
                //Not sure what we want to do if Validation fails (orginal sql just threw an error but can imagine we want to return the reason for validation fail, no?)
            }
        }

        public async Task<Models.Distance> GetDistanceOfRouteForInRuleAsync(string origin, string destination, CancellationToken cancellationToken)
        {
            if (origin.IsNullOrWhiteSpace()) throw new ArgumentException("Origin must be specified.", nameof(origin));
            if (destination.IsNullOrWhiteSpace()) throw new ArgumentException("Destination must be specified.", nameof(destination));

            string sql = $"EXEC {Constants.Application.GetDistanceOfRouteForInRuleProcedure} @ORIGIN, @DESTINATION";
            List<SqlParameter> sqlParameters = new() { new SqlParameter { ParameterName = "@ORIGIN", Value = origin, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 30 }, new SqlParameter { ParameterName = "@DESTINATION", Value = destination, Direction = ParameterDirection.Input, SqlDbType = SqlDbType.VarChar, Size = 30 } };

            var distances = await _commonReferenceDataContext.Distance.FromSqlRaw(sql, sqlParameters.ToArray()).ToListAsync(cancellationToken);
            return distances.SingleOrDefault();
        }
    }
}
