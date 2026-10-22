using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Entities.Common;
using VAA.EAPI.NDC.Order.Infrastructure.Model.SeatMS;

namespace VAA.EAPI.NDC.Order.Validations.Seat
{
    public static class SeatMapValidations
    {
        public static IEnumerable<NDCWarning> ValidateSeatDetails(SeatAvailabilityResponse seatAvailabilityResponse, IEnumerable<SeatDetails> seatDetails , List<string> invalidSeats, string currency, decimal totalAmountInReq, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();
            string totalAmount = default;

            foreach (var seat in seatDetails)
            {
                var seatMap = seatAvailabilityResponse.FlightSegmentSeatMap.FirstOrDefault(x => x.FlightSegment.MarketingFlightNumber == seat.AssociatedSegment.MarketingFlightNumber);
                var seatCols = seatMap.Seatmap.SelectMany(x => x.SeatRows).SelectMany(x => x.SeatColumns);

                if (!seatCols.Any(x => x.SeatIdentifer == seat.SeatNumber))
                {
                    warnings.Add(new NDCWarning()
                    {
                        Code = "256_1",
                        Type = "Warning",
                        IsDynamicMessage = true,
                        DynamicMessagesContents =
                        [
                            new ()
                            {
                                Fieldname = "<<SeatNumber>>",
                                ReplaceWith = seat.AssociatedSegment.MarketingCarrier + seat.AssociatedSegment.MarketingFlightNumber + "-" + seat.SeatNumber
                            }
                        ]
                    });

                    invalidSeats.Add(seat.SeatNumber + "-" + seat.AssociatedSegment.MarketingFlightNumber);
                }
                else if(seatCols.First(x => x.SeatIdentifer == seat.SeatNumber).Available == false)
                {
                    warnings.Add(new NDCWarning()
                    {
                        Code = "281_1",
                        Type = "Warning",
                        IsDynamicMessage = true,
                        DynamicMessagesContents =
                        [
                            new ()
                            {
                                Fieldname = "<<SeatNumber>>",
                                ReplaceWith = seat.AssociatedSegment.MarketingCarrier + seat.AssociatedSegment.MarketingFlightNumber + "-" + seat.SeatNumber
                            }
                        ]
                    });

                    invalidSeats.Add(seat.SeatNumber + "-" + seat.AssociatedSegment.MarketingFlightNumber);
                }
                else
                {
                    var catalogueId = seatCols.First(x => x.SeatIdentifer == seat.SeatNumber && x.CatalogueRefIds != null).Eligibility.FirstOrDefault(x => x.CustomerId.First() == seat.Beneficiary.LastNameNumber + "-" + seat.Beneficiary.FirstNameNumber).CatalogueRefId;
                    var seatPrice = seatMap.Catalogue.FirstOrDefault(x => x.CatalogueId == catalogueId).SeatPrice;

                    if (seatPrice.BaseCurrency != currency)
                    {
                        warnings.Add(new()
                        {
                            Code = "727_1",
                            Type = "Warning",
                            IsDynamicMessage = true,
                            DynamicMessagesContents =
                            [
                                new ()
                                {
                                    Fieldname = "<<FlightNumber>>",
                                    ReplaceWith = seat.AssociatedSegment.MarketingCarrier + seat.AssociatedSegment.MarketingFlightNumber
                                }
                            ]
                        });

                        invalidSeats.Add(seat.SeatNumber + "-" + seat.AssociatedSegment.MarketingFlightNumber);
                    }
                    else
                        totalAmount = Convert.ToString(Convert.ToDecimal(totalAmount) + Convert.ToDecimal(seatPrice.MonetaryAmount));
                }
            }

            if (Convert.ToDecimal(totalAmount) > totalAmountInReq && totalAmount != default)
            {
                warnings.Add(new()
                {
                    Code = "290_3",
                    Type = "Warning",
                    IsDynamicMessage = true,
                    DynamicMessagesContents =
                    [
                        new DynamicMessagesContent()
                        {
                            Fieldname = "<<TotalAmount>>",
                            ReplaceWith = totalAmount + currency
                        }
                    ]
                });

                foreach (var seat in seatDetails)
                    invalidSeats.Add(seat.SeatNumber + "-" + seat.AssociatedSegment.MarketingFlightNumber);
            }
            else if (Convert.ToDecimal(totalAmount) < totalAmountInReq && totalAmount != default)
                warnings.Add(new()
                {
                    Code = "992_1",
                    Type = "Warning",
                    IsDynamicMessage = true,
                    DynamicMessagesContents =
                    [
                        new DynamicMessagesContent()
                        {
                            Fieldname = "<<TotalAmount>>",
                            ReplaceWith = totalAmount + currency
                        }
                    ]
                }); 

            return ErrorHelper<NDCWarning>.GetWarnings(warnings , hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateSeatMapResponse(SeatFulfilmentResponse seatFulfilmentResponse, IEnumerable<SeatDetails> seatDetails, IHostEnvironment hostEnvironment, List<NDCWarning> orderWarnings)
        {
            var warnings = new List<NDCWarning>();

            warnings.AddRange(ValidateSeatMapSuccessResponse(seatFulfilmentResponse, seatDetails));

            if (warnings.Any(x => x.Code == "290_2"))
                orderWarnings.Remove(orderWarnings.FirstOrDefault(x => x.Code == "992"));
            else
            {
                warnings.AddRange(ValidateSeatErrorResponse(seatFulfilmentResponse));
                if((bool)seatFulfilmentResponse?.OrderItems?.Any()) UpdatePriceMismatch(seatFulfilmentResponse,orderWarnings);
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings , hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateSeatMapSuccessResponse(SeatFulfilmentResponse seatFulfilmentResponse , IEnumerable<SeatDetails> seatDetails)
        {
            var warnings = new List<NDCWarning>();
            IEnumerable<string> failedSeats = [];
            IEnumerable<string> bookedSeats = [];

            if (seatFulfilmentResponse == null)
                warnings.Add(new() { Code = "290_2", IsDynamicMessage = false, Type = "Warning" });
            else
            {
                if(seatFulfilmentResponse.OrderItems == null || !seatFulfilmentResponse.OrderItems.Any())
                {
                    failedSeats = seatDetails.Select(x => x.AssociatedSegment.MarketingCarrier + x.AssociatedSegment.MarketingFlightNumber + "-" + x.SeatNumber);
                }
                else
                {
                    failedSeats = seatDetails.Where(x => !seatFulfilmentResponse.OrderItems.Any(o => o.FlightIdRef == x.AssociatedSegment.SegmentId && o.SeatNumber == x.SeatNumber)).Select(x => x.AssociatedSegment.MarketingCarrier + x.AssociatedSegment.MarketingFlightNumber + "-" + x.SeatNumber);
                    bookedSeats = seatDetails.Where(x => seatFulfilmentResponse.OrderItems.Any(o => o.FlightIdRef == x.AssociatedSegment.SegmentId && o.SeatNumber == x.SeatNumber)).Select(x => x.AssociatedSegment.MarketingCarrier + x.AssociatedSegment.MarketingFlightNumber + "-" + x.SeatNumber);

                    if (!bookedSeats.Any())
                        warnings.Add(new() { Code = "290_2", IsDynamicMessage = false, Type = "Warning" });
                    else if (bookedSeats.Any() && failedSeats.Any())
                        warnings.Add(new()
                        {
                            Code = "993_1",
                            Type = "Warning",
                            IsDynamicMessage = true,
                            DynamicMessagesContents = 
                            [
                                new()
                                {
                                    Fieldname = "<<Seat Numbers>>",
                                    ReplaceWith = string.Join(", ", bookedSeats)
                                },
                                new()
                                {
                                    Fieldname = "<<SeatNumbers>>",
                                    ReplaceWith = string.Join(", ", failedSeats)
                                }
                            ]
                        });
                }
            }

            return warnings;
        }

        public static IEnumerable<NDCWarning> ValidateSeatErrorResponse(SeatFulfilmentResponse seatFulfilmentResponse)
        {
            if (seatFulfilmentResponse?.Error != null && seatFulfilmentResponse.Error.Any())
            {
                return seatFulfilmentResponse.Error.Select(error =>
                {
                    if (Constants.Application.SeatAddErrors_997.Split("|").Contains(error.Code))
                        return new NDCWarning()
                        {
                            Code = "997_1",
                            Type = "Warning",
                            IsDynamicMessage = true,
                            DynamicMessagesContents =
                            [
                                new DynamicMessagesContent()
                                {
                                    Fieldname = "<<ErrorCode>>",
                                    ReplaceWith = error.Code
                                }
                            ]
                        };
                    else if (Constants.Application.SeatAddErrors_998.Split("|").Contains(error.Code))
                        return new NDCWarning()
                        {
                            Code = "998_1",
                            Type = "Warning",
                            IsDynamicMessage = true,
                            DynamicMessagesContents =
                            [
                                new DynamicMessagesContent()
                                {
                                    Fieldname = "<<ErrorCode>>",
                                    ReplaceWith = error.Code
                                }
                            ]
                        };
                    else if (Constants.Application.SeatAddErrors_999.Split("|").Contains(error.Code))
                        return new NDCWarning()
                        {
                            Code = "999_1",
                            Type = "Warning",
                            IsDynamicMessage = true,
                            DynamicMessagesContents =
                            [
                                new DynamicMessagesContent()
                                {
                                    Fieldname = "<<ErrorCode>>",
                                    ReplaceWith = error.Code
                                }
                            ]
                        };
                    else return null;
                });
            }

            return [];
        }

        public static IEnumerable<NDCWarning> UpdatePriceMismatch(SeatFulfilmentResponse seatFulfilmentResponse , List<NDCWarning> warnings)
        {
            var totalSeatPrice = seatFulfilmentResponse.OrderItems.Sum(x => Convert.ToDecimal(x.Amount.MonetaryAmount)).ToString();
            var currency = seatFulfilmentResponse.OrderItems.First().Amount.BaseCurrency;
            var decimalPrecision = seatFulfilmentResponse.OrderItems.First().Amount.DecimalPrecisionCount;

            if (warnings.Any(x => x.Code == "992"))
            {
                var warning = warnings.First(x => x.Code == "992");
                warning.ShortText = totalSeatPrice == "0" ? warning.ShortText.Split("seats").First() + "seats" + " " + totalSeatPrice + " " + currency : warning.ShortText.Split("seats").First() + "seats" + " " + totalSeatPrice.Insert(totalSeatPrice.Length - decimalPrecision, ".") + " " + currency;
                
            }

            return warnings;
        }

        public static IEnumerable<NDCWarning> ValidateSeatRetrieveResponse(IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings([new NDCWarning() { Code = "290_2", IsDynamicMessage = false, Type = "Warning" }], hostEnvironment);
        }

        public static IEnumerable<NDCWarning> ValidateInvalidSegments(List<string> invalidSeats, IEnumerable<SeatDetails> seatDetails, IHostEnvironment hostEnvironment)
        {
            var warnings = new List<NDCWarning>();
            var invalidSegs = seatDetails.Where(x => invalidSeats.Any(y => y.Split("-").First() == x.SeatNumber && y.Split("-").Last() == x.AssociatedSegment.MarketingFlightNumber)).Select(x => x.AssociatedSegment.SegmentId).Distinct();

            foreach(var seat in seatDetails)
            {
                if (invalidSegs.Any(x => x == seat.AssociatedSegment.SegmentId) && !invalidSeats.Any(x => x.Split("-").First() == seat.SeatNumber && x.Split("-").Last() == seat.AssociatedSegment.MarketingFlightNumber))
                {
                    invalidSeats.Add(seat.SeatNumber + "-" + seat.AssociatedSegment.MarketingFlightNumber);
                    warnings.Add(new NDCWarning()
                    {
                        Code = "290_1",
                        Type = "Warning",
                        IsDynamicMessage = true,
                        DynamicMessagesContents =
                        [
                            new ()
                            {
                                Fieldname = "<<SeatNumber>>",
                                ReplaceWith = seat.AssociatedSegment.MarketingCarrier + seat.AssociatedSegment.MarketingFlightNumber + "-" + seat.SeatNumber
                            }
                        ]
                    });
                }
            }

            return ErrorHelper<NDCWarning>.GetWarnings(warnings, hostEnvironment);
        }
        public static IEnumerable<NDCWarning> ValidateCityCodeResponse(IHostEnvironment hostEnvironment)
        {
            return ErrorHelper<NDCWarning>.GetWarnings([new NDCWarning() { Code = "911_51", IsDynamicMessage = false, Type = "Warning" }], hostEnvironment);
        }
    }
}
