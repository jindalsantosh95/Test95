using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;

namespace VAA.EAPI.NDC.Order.Validations.OrderList
{
    public static class OrderListRequestValidations
    {
        public static IEnumerable<ErrorDetail> ValidateFilterCombinations<T>(IATA_OrderListRQ request)
        {
            var errors = new List<ErrorDetail>();
            var filterCriteria = request.Request.OrderSearchFilterCriteria;
            var paymentStatus = request.Request.OrderSearchFilterCriteria.PaymentFilterCriteria?.FirstOrDefault()?.PaymentStatusCode;

            if (!string.IsNullOrEmpty(paymentStatus) && !(paymentStatus.Is("PAID") || paymentStatus.Is("UNPAID")))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_38" });

            if (filterCriteria.PaymentFilterCriteria != null && filterCriteria.PaymentFilterCriteria?.FirstOrDefault()?.PaymentStatusCode.ToUpper() == "PAID" && filterCriteria.OrderCancellationDateFilterCriteria != null && filterCriteria.OrderCancellationDateFilterCriteria.ExpirationDateSpecified)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_39" });

            if (errors.Count == 0 && filterCriteria.PaymentFilterCriteria != null && filterCriteria.CreationDateTimeFilterCriteria == null)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_40" });

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateMandatoryFields<T>(IATA_OrderListRQ request)
        {
            var errors = new List<ErrorDetail>();
            var filterCriteria = request.Request.OrderSearchFilterCriteria;

            if (filterCriteria.CreationDateTimeFilterCriteria != null && ((filterCriteria.CreationDateTimeFilterCriteria.FromDateTimeSpecified && !filterCriteria.CreationDateTimeFilterCriteria.ToDateTimeSpecified) || (!filterCriteria.CreationDateTimeFilterCriteria.FromDateTimeSpecified && filterCriteria.CreationDateTimeFilterCriteria.ToDateTimeSpecified)))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_37" });

            if (errors.Count == 0 && filterCriteria.LastModifiedDateTimeFilterCriteria != null && ((filterCriteria.LastModifiedDateTimeFilterCriteria.FromDateTimeSpecified && !filterCriteria.LastModifiedDateTimeFilterCriteria.ToDateTimeSpecified) || (!filterCriteria.LastModifiedDateTimeFilterCriteria.FromDateTimeSpecified && filterCriteria.LastModifiedDateTimeFilterCriteria.ToDateTimeSpecified)))
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_37" });

            if(errors.Count == 0 && filterCriteria.OrderCancellationDateFilterCriteria != null && !filterCriteria.OrderCancellationDateFilterCriteria.ExpirationDateSpecified)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_37" });

            if(errors.Count == 0 && filterCriteria.TravelDateFilterCriteria != null  && !filterCriteria.TravelDateFilterCriteria.EffectiveDateSpecified)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_37" });

            if(errors.Count == 0 && filterCriteria.PaymentFilterCriteria != null && filterCriteria.PaymentFilterCriteria.First().PaymentStatusCode == null)
                errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_37" });

            return errors;
        }

        public static IEnumerable<ErrorDetail> ValidateDateRange<T>(IATA_OrderListRQ request)
        {
            var errors = new List<ErrorDetail>();
            var filterCriteria = request.Request.OrderSearchFilterCriteria;
            int dateRange = ConfigReader.Get<int>(Constants.Environment.OrderListFilterDateRange);

            if (filterCriteria.TravelDateFilterCriteria != null && filterCriteria.TravelDateFilterCriteria.EffectiveDateSpecified != null)
            {
                if(Convert.ToDateTime(filterCriteria.TravelDateFilterCriteria.EffectiveDate).Date < DateTime.Now.Date)
                    errors.Add(new ErrorDetail { IsDynamicMessage = false, ErrorCode = "912_41" });
            }

            if(errors.Count == 0 && filterCriteria.CreationDateTimeFilterCriteria != null)
            {
                var fromDateTime = Convert.ToDateTime(filterCriteria.CreationDateTimeFilterCriteria.FromDateTime).Date;
                var toDateTime = Convert.ToDateTime(filterCriteria.CreationDateTimeFilterCriteria.ToDateTime).Date;

                if((toDateTime-fromDateTime).TotalDays > dateRange)
                    errors.Add(new ErrorDetail { IsDynamicMessage = true, ErrorCode = "912_42", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "<DateRange>", ReplaceWith = dateRange.ToString() } } });
            }

            if (errors.Count == 0 && filterCriteria.LastModifiedDateTimeFilterCriteria != null)
            {
                var fromDateTime = Convert.ToDateTime(filterCriteria.LastModifiedDateTimeFilterCriteria.FromDateTime).Date;
                var toDateTime = Convert.ToDateTime(filterCriteria.LastModifiedDateTimeFilterCriteria.ToDateTime).Date;

                if ((toDateTime - fromDateTime).TotalDays > dateRange)
                    errors.Add(new ErrorDetail { IsDynamicMessage = true, ErrorCode = "912_42", DynamicMessagesContents = new List<DynamicMessagesContent>() { new() { Fieldname = "<DateRange>", ReplaceWith = dateRange.ToString() } } });
            }

            return errors;
        }
    }
}
