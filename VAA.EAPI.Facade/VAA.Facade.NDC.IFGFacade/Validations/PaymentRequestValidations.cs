using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using VAA.EAPI.Domain.Entities.Common;
using VAA.Facade.NDC.IFGFacade.Domain;

namespace VAA.Facade.NDC.IFGFacade.Validations
{
    public static class PaymentRequestValidations
    {
        private static ErrorDetails Error(string message) =>
            new ErrorDetails { Code = "G009", Message = message };


        public static IEnumerable<ErrorDetails> ValidateAuthRequest(AuthorizeRequest request)
        {
            var errors = new List<ErrorDetails>();

            if (request == null)
            {
                errors.Add(Error("Request cannot be null."));
                return errors;
            }

            errors.AddRange(ValidateOrders(request.Orders));
            errors.AddRange(ValidateBasicInformation(request.BasicInformation));
            errors.AddRange(ValidateAuthFormOfPayments(request.FormOfPayment));

            return errors;
        }

        public static IEnumerable<ErrorDetails> ValidateOrderRequest(OrderRequest request)
        {
            var errors = new List<ErrorDetails>();

            if (request == null)
            {
                errors.Add(Error("Request cannot be null."));
                return errors;
            }

            errors.AddRange(ValidateOrders(request.Orders));
            errors.AddRange(ValidateBasicInformation(request.BasicInformation));

            if (request.TransactionId is null)
                errors.Add(Error("Request TransactionId cannot be null."));

            errors.AddRange(ValidateOrderFormOfPayments(request.FormOfPayment));

            return errors;
        }

        public static IEnumerable<ErrorDetails> ValidateOrders(IEnumerable<Order> orders)
        {
            var errors = new List<ErrorDetails>();

            if (orders is null)
            {
                errors.Add(Error("Request Orders cannot be null."));
                return errors;
            }

            if (!orders.Any())
            {
                errors.Add(Error("Request Orders cannot be empty."));
                return errors;
            }

            foreach (var order in orders)
            {
                if (!order.FlightRecords.Any())
                    errors.Add(Error("Request Orders FlightRecords cannot be empty."));

                foreach (var flightRecord in order.FlightRecords)
                    errors.AddRange(ValidateFlightRecord(flightRecord));
            }
            
            return errors;
        }

        private static IEnumerable<ErrorDetails> ValidateBasicInformation(BasicInformation info)
        {
            var errors = new List<ErrorDetails>();

            if (info is null)
            {
                errors.Add(Error("Request BasicInformation cannot be null."));
                return errors;
            }

            if (info.CountryCode is null)
                errors.Add(Error("Request BasicInformation CountryCode cannot be null."));

            if (info.AgentCode is null)
                errors.Add(Error("Request BasicInformation AgentCode cannot be null."));

            return errors;
        }

        private static IEnumerable<ErrorDetails> ValidateFlightRecord(FlightRecord flightRecord)
        {
            var errors = new List<ErrorDetails>();

            if (flightRecord.DocumentDetails is null)
            {
                errors.Add(Error("Request Orders FlightRecords DocumentDetails cannot be null."));
            }
            else if (flightRecord.DocumentDetails.Itinerary is null)
            {
                errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary cannot be null."));
            }
            else
            {
                foreach (var itinerary in flightRecord.DocumentDetails.Itinerary.Where(x => x.CouponUseIndicator != Constants.Application.ArnkCouponUseIndicator))
                {
                    if (itinerary is null)
                    {
                        errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary entry cannot be null."));
                    }
                    else
                    {
                        errors.AddRange(ValidateItinerary(itinerary));
                    }
                }
            }

            if (flightRecord.PassengerRecord is null)
            {
                errors.Add(Error("Request Orders FlightRecords PassengerRecord cannot be null."));
                return errors;
            }

            if (flightRecord.PassengerRecord.Name is null)
                errors.Add(Error("Request Orders FlightRecords PassengerRecord Name cannot be null."));

            if (flightRecord.PassengerRecord.DateOfBirth is not null)
            {
                if (!DateTime.TryParseExact( flightRecord.PassengerRecord.DateOfBirth, "yyyyMMdd", CultureInfo.InvariantCulture, DateTimeStyles.None,  out var dob))
                {
                    errors.Add(Error($"Request Orders FlightRecords PassengerRecord DateOfBirth '{flightRecord.PassengerRecord.DateOfBirth}' is invalid. Expected format: yyyyMMdd."));
                }                
            }

            if (flightRecord.PassengerRecord.Pnr is null)
                errors.Add(Error("Request Orders FlightRecords PassengerRecord Pnr cannot be null."));
            else if (!flightRecord.PassengerRecord.Pnr.All(c => char.IsLetterOrDigit(c)))
                errors.Add(Error("Request Orders FlightRecords PassengerRecord Pnr must be alphanumeric."));

            return errors;
        }

        private static IEnumerable<ErrorDetails> ValidateItinerary(Itinerary itinerary)
        {
            var errors = new List<ErrorDetails>();

            if (itinerary.OriginCode is null)
                errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary OriginCode cannot be null."));

            if (itinerary.DestinationCode is null)
                errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary DestinationCode cannot be null."));

            if (itinerary.Carrier is null)
                errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary Carrier cannot be null."));

            if (itinerary.ReservationBooking is null)
                errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary ReservationBooking cannot be null."));

            if (itinerary.FlightDate is null)
                errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary FlightDate cannot be null."));

            if (itinerary.FlightTime is null)
                errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary FlightTime cannot be null."));

            if (itinerary.FlightNumber is null)
                errors.Add(Error("Request Orders FlightRecords DocumentDetails Itinerary FlightNumber cannot be null."));

            return errors;
        }

        private static IEnumerable<ErrorDetails> ValidateAuthFormOfPayments(IEnumerable<FormOfPayment> fops)
        {
            var errors = new List<ErrorDetails>();

            if (fops is null)
            {
                errors.Add(Error("Request FormOfPayment cannot be null."));
                return errors;
            }

            foreach (var fop in fops)
            {
                if (fop.Amount == 0)
                    errors.Add(Error("Request FormOfPayment Amount cannot be 0."));

                if (fop.Currency is null)
                    errors.Add(Error("Request FormOfPayment Currency cannot be null."));

                if (fop.AccountNumber is null)
                    errors.Add(Error("Request FormOfPayment AccountNumber cannot be null."));

                if (fop.ExpiryDate is null)
                    errors.Add(Error("Request FormOfPayment ExpiryDate cannot be null."));

                if (fop.CardSecurityCode is null)
                    errors.Add(Error("Request FormOfPayment CardSecurityCode cannot be null."));

                if (fop.Type is null)
                    errors.Add(Error("Request FormOfPayment Type cannot be null."));

                if (fop.Payer is null)
                {
                    errors.Add(Error("Request FormOfPayment Payer cannot be null."));
                }
                else
                {
                    if (fop.Payer.Name is null)
                        errors.Add(Error("Request FormOfPayment Payer Name cannot be null."));

                    if (fop.Payer.Email is null)
                        errors.Add(Error("Request FormOfPayment Payer Email cannot be null."));
                }

                if (fop.Address is null)
                {
                    errors.Add(Error("Request FormOfPayment Address cannot be null."));
                }

                if (!fop.MotoPayment)
                {
                    if (fop.CardAuthentication is null)
                    {
                        errors.Add(Error("Request FormOfPayment CardAuthentication cannot be null when MotoPayment is false."));
                    }
                    else
                    {
                        if (fop.CardAuthentication.AuthenticationValue is null)
                            errors.Add(Error("Request FormOfPayment CardAuthentication AuthenticationValue cannot be null."));

                        if (fop.CardAuthentication.ServerTransactionId is null)
                            errors.Add(Error("Request FormOfPayment CardAuthentication ServerTransactionId cannot be null."));

                        if (fop.CardAuthentication.EcomInd is null)
                            errors.Add(Error("Request FormOfPayment CardAuthentication EcomInd cannot be null."));
                    }
                }
            }

            return errors;
        }

        private static IEnumerable<ErrorDetails> ValidateOrderFormOfPayments(IEnumerable<FormOfPayment> fops)
        {
            var errors = new List<ErrorDetails>();

            if (fops is null)
            {
                errors.Add(Error("Request FormOfPayment cannot be null."));
                return errors;
            }

            foreach (var fop in fops)
            {
                if (fop.Amount == 0)
                    errors.Add(Error("Request FormOfPayment Amount cannot be 0."));

                if (fop.Currency is null)
                    errors.Add(Error("Request FormOfPayment Currency cannot be null."));

                if (fop.Type is null)
                    errors.Add(Error("Request FormOfPayment Type cannot be null."));
            }

            return errors;
        }

        public static IEnumerable<ErrorDetails> ValidateRefundRequest(RefundRequest refundRequest)
        {
            var errors = new List<ErrorDetails>();

            if (refundRequest == null)
            {
                errors.Add(Error("Request cannot be null."));
                return errors;
            }

            errors.AddRange(ValidateRefundOrders(refundRequest.Orders));
            errors.AddRange(ValidateRefundFormOfPayments(refundRequest.FormOfPayment));

            if (refundRequest.TransactionId is null)
                errors.Add(Error("Request TransactionId cannot be null."));

            return errors;
        }

        private static IEnumerable<ErrorDetails> ValidateRefundOrders(IEnumerable<Order> orders)
        {
            var errors = new List<ErrorDetails>();

            if (orders is null)
            {
                errors.Add(Error("Request Orders cannot be null."));
                return errors;
            }

            if (!orders.Any())
            {
                errors.Add(Error("Request Orders cannot be empty."));
                return errors;
            }

            foreach (var order in orders)
            {
                if (order is null)
                {
                    errors.Add(Error("Order cannot be null."));
                }
                else
                {
                    if (order.FlightRecords is null || !order.FlightRecords.Any())
                    {
                        errors.Add(Error("Order FlightRecords cannot be null or empty."));
                    }
                    else
                    {
                        foreach (var flightRecord in order.FlightRecords)
                        {
                            if (flightRecord is null)
                            {
                                errors.Add(Error("Request Orders FlightRecords cannot contain null items."));
                                continue;
                            }

                            errors.AddRange(ValidateRefundFlightRecord(flightRecord));
                        }
                    }
                }

            }

            return errors;
        }

        private static IEnumerable<ErrorDetails> ValidateRefundFlightRecord(FlightRecord flightRecord)
        {
            var errors = new List<ErrorDetails>();

            if (flightRecord.PassengerRecord is null)
            {
                errors.Add(Error("Request Orders FlightRecords PassengerRecord cannot be null."));
                return errors;
            }

            if (flightRecord.PassengerRecord.Pnr is null)
                errors.Add(Error("Request Orders FlightRecords PassengerRecord Pnr cannot be null."));

            return errors;
        }

        private static IEnumerable<ErrorDetails> ValidateRefundFormOfPayments(IEnumerable<FormOfPayment> fops)
        {
            var errors = new List<ErrorDetails>();

            if (fops is null || !fops.Any())
            {
                errors.Add(Error("Request FormOfPayment cannot be null or empty."));
                return errors;
            }

            foreach (var fop in fops)
            {
                if (fop is null)
                {
                    errors.Add(Error("Request FormOfPayment cannot contain null items."));
                    continue;
                }

                if (fop.Amount == 0)
                    errors.Add(Error("Request FormOfPayment Amount cannot be 0."));

                if (fop.Currency is null)
                    errors.Add(Error("Request FormOfPayment Currency cannot be null."));

            }

            return errors;
        }


    }
}
