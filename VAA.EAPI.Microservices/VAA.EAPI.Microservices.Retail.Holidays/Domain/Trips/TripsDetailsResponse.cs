using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Net;
using VAA.EAPI.Common;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Holidays.Domain.Enums;

namespace VAA.EAPI.Microservices.Retail.Holidays.Domain.Trips
{
    public class TripsDetailsResponse
    {
        public Booking Booking { get; set; }
        public IEnumerable<ErrorDetails> Errors { get; set; }
        public int Status { get; set; }

        public static explicit operator TripsDetailsResponse(JObject source)
        {
            if (source == null) return default;

            if (source["data"]?[0]?["summary"] == null) return default;
            else
            {
                JToken packageResponse = source["data"]?[0];
                JToken packageSummary = packageResponse?["summary"];

                var bookingDivision = packageSummary?["division"]?.Value<string>();
                if (bookingDivision != null && bookingDivision == Constants.Application.CruiseDivision)
                {
                    return new()
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(Constants.Application.CruiseBookingErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    };
                }

                var bookingStatus = packageSummary?["bookingStatus"]?.Value<int>();
                if(bookingStatus == null || bookingStatus == (int)HolidayBookingStatus.CANCELLED)
                {
                    return new()
                    {
                        Status = (int)HttpStatusCode.BadRequest,
                        Errors = ErrorMapper.MapBusinessExceptionToEAPIException(bookingStatus == null ? Constants.Application.BookingStatusMissingErrorCode : Constants.Application.CancelledBookingErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                    };  
                }
                else
                { 
                    var optionStatus = packageSummary?["optionStatus"]?.Value<int>();
                    if (bookingStatus == (int)HolidayBookingStatus.QUOTE || (optionStatus == null || optionStatus == (int)HolidayBookingOptionStatus.Quote))
                    {
                        return new()
                        {
                            Status = (int)HttpStatusCode.BadRequest,
                            Errors = ErrorMapper.MapBusinessExceptionToEAPIException(optionStatus == null ? Constants.Application.OptionStatusMissingErrorCode : Constants.Application.QuoteBookingErrorCode, AsyncContext.Current.CorrelationId.ToString(), EAPI.Infrastructure.Constants.Constants.ErrorCodesPath)
                        };
                    }
                    else
                    {
                        return new()
                        {
                            Booking = (Booking)source,
                            Status = (int)HttpStatusCode.OK
                        };

                    }
                        
                }

            }

        }
    }
}
