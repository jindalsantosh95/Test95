using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System;
using System.Linq;
using VAA.EAPI.Common;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Seat.Constants;
using VAA.EAPI.Microservices.Retail.Seat.Domain;
using VAA.EAPI.Microservices.Retail.Seat.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Seat.Domain.SeatAvailability;
using VAA.EAPI.Microservices.Retail.Seat.Infrastructure.Proxy.SeatV8Proxy;

namespace VAA.EAPI.Microservices.Retail.Seat.Mapper.SeatAvailability
{
    public static class SeatFulFilmentRequestMapper
    {
        #region Seat
        public static AddRequestType MapAddSeat(SeatFulfilmentRequest request, Channel channelInfo = null)
        {
            var additionalDetailType = new AdditionalDetailsType();
            if (request.paymentDetails?.FirstOrDefault()?.PaymentReferenceId != null)
            {
                additionalDetailType.VendorFopTypeDetailText = Constants.Application.VendorFopTypeDetailText;
                additionalDetailType.VendorFormattedPaymentReferenceId = request.paymentDetails?.FirstOrDefault()?.PaymentReferenceId;
            }
            else
                additionalDetailType.PaymentTrackingId = request.SelectedSeatInfo.FirstOrDefault()?.OfferId;


            return new AddRequestType
            {
                RequestInfo = GetRequestInfo(request, channelInfo),
                SeatInfo = GetSeatInfo(request),
                FormOfPaymentInfo = GetFormOfPaymentInfo(request),
                PointOfSaleInfo = PointOfSaleInfoType(request, channelInfo),
                AdditionalDetails = additionalDetailType
            };
        }

        private static PointOfSaleInfoType PointOfSaleInfoType(SeatFulfilmentRequest request, Channel channel)
        {
            if (channel == null)
                return new PointOfSaleInfoType
                {
                    AgencyName = request.Parties.Participants.FirstOrDefault()?.AgencyID,
                    CityCode = request.Pos.CityCode,
                    BartsLocation = request.Pos.CityCode != null ? request.Pos.CityCode + "NDC" : null,
                    CountryCode = request.Pos.CountryCode
                };
            else if (channel.ChannelId == Constants.Application.NdcChannel)
            {
                return new PointOfSaleInfoType
                {
                    AgencyName = request.Parties.Participants.FirstOrDefault()?.AgencyID,
                    CityCode = request.Pos.CityCode,
                    BartsLocation = request.Pos.NDCCityCodeLocation,
                    CountryCode = request.Pos.CountryCode
                };
            }
            else
                return new PointOfSaleInfoType
                {
                    AgentId = channel.DlAgentId,
                    CityCode = request.Pos.CityCode,
                    BartsLocation = request.Pos.CityCode != null ? request.Pos.CityCode + "WEB" : null,
                    CountryCode = request.Pos.CountryCode
                };
        }

        private static SeatInfoType[] GetSeatInfo(SeatFulfilmentRequest request)
        {
            return request.SelectedSeatInfo.Select(si =>
                new SeatInfoType()
                {
                    OfferId = si.OfferId,
                    OfferItemNumber = si.OfferItemNumber,
                    SeatId = GetSeatId(si),
                    AssociationId = si.AssociationId,
                    RecordLocator = request.BookingReferences?.RLOC
                }).ToArray();
        }

        private static SeatIdType[] GetSeatId(SelectedSeatInfo seatInfo)
        {
            return new SeatIdType[]
            {
               new SeatIdType()
               {
                 FlightIdRef = seatInfo.FlightIdRef,
                 SeatNumber = seatInfo.SeatNumber,
               }
            };
        }

        private static RequestInfoType GetRequestInfo(SeatFulfilmentRequest request, Channel channelInfo)
        {
            if (channelInfo == null)
                return new RequestInfoType()
                {
                    ApplicationId = "ND",
                    DebugOptions = "N",
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    HostContextParams = new HostContextParamsType()
                    {
                        PoolType = "STAMPING/VAA/NEW/DIST/CAPABILITY",
                        Airline = request.BookingReferences.AirlineDesignator,
                        City = request.Pos.CityCode,
                        DutyCode = Constants.Application.DutyCode,
                    },
                    TransactionId = request.SelectedSeatInfo.FirstOrDefault()?.OfferId,
                    AppChannelName = "NCD",
                    Location = request.Pos.CityCode,
                    Company = request.BookingReferences.AirlineDesignator,
                    ApplicationVersion = Constants.Application.ApplicationVersion,
                    TurnOffResponseInfo = Constants.Application.TurnOffResponseInfo
                };
            else
                return new RequestInfoType()
                {
                    ApplicationId = channelInfo.DlApplicationId,
                    DebugOptions = "N",
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    HostContextParams = new HostContextParamsType()
                    {
                        PoolType = channelInfo.DlPoolType,
                        Airline = request.BookingReferences.AirlineDesignator,
                        City = request.Pos.CityCode,
                        DutyCode = Constants.Application.DutyCode,
                    },
                    TransactionId = AsyncContext.Current.CorrelationId.ToString(),
                    AppChannelName = channelInfo.ChannelName,
                    Location = request.Pos.CityCode,
                    Company = request.BookingReferences.AirlineDesignator,
                    ApplicationVersion = Constants.Application.ApplicationVersion,
                    TurnOffResponseInfo = Constants.Application.TurnOffResponseInfo
                };


        }

        private static FormOfPaymentInfoType GetFormOfPaymentInfo(SeatFulfilmentRequest request)
        {
            if (request.paymentDetails.IsNullOrEmpty())
            {
                return null;
            }
            else
            {
                var amount = GetSeatChargeAmount(request);
                var paymentDetails = request.paymentDetails?.FirstOrDefault();
                return paymentDetails?.FormOfPayments == EAPI.Domain.Virgin.Models.Payment.PaymentDetails.FormOfPayment.CashEnum ?
                new FormOfPaymentInfoType()
                {
                    CashInfo = new()
                    {
                        Amount = amount,
                        AmountDecimalPlaces = paymentDetails?.Amount.DecimalPrecisionCount.ToString(),
                        CurrencyCode = paymentDetails?.Amount.BaseCurrency
                    },
                    MiscellaneousPaymentInfo = [new() {
                        AccountName = request.Parties.Participants.FirstOrDefault()?.Name,
                        AccountNumber = request.Parties.Participants.FirstOrDefault()?.AgencyID,
                        Amount = amount,
                        AmountDecimalPlaces = paymentDetails?.Amount.DecimalPrecisionCount.ToString(),
                        CurrencyCode = paymentDetails?.Amount.BaseCurrency
                    }]
                }
                : new FormOfPaymentInfoType()
                {
                    CreditCardInfo = new CreditCardInfoType[]
                    {
                        new CreditCardInfoType()
                        {
                            CardNumber = paymentDetails?.CardMethod.CardNumber,
                            ChargeAmount = amount,
                            ChargeAmountDecimalPlaces = paymentDetails?.Amount.DecimalPrecisionCount.ToString(),
                            CurrencyCode= paymentDetails?.Amount.BaseCurrency,
                            ExpirationMonth= paymentDetails?.CardMethod.ExpiryDate.Substring(0,2),
                            ExpirationYear=paymentDetails?.CardMethod.ExpiryDate.Substring(3,2),
                            VendorCode= paymentDetails?.CardMethod.CardType.ToString(),
                            SecurityCode= paymentDetails?.CardMethod.CardType.ToString() == "TP" ? null : paymentDetails?.CardMethod.Cvv,
                            CardHolderInfo= new CustomerInfoType
                            {
                                EmailAddress = paymentDetails?.Payer.PayerContacts?.Email,
                                AddressInfo = new AddressInfoType
                                {
                                    AddressLine1 =paymentDetails?.Payer.PayerAddress.AddressLine1,
                                    AddressLine2 =paymentDetails?.Payer.PayerAddress.AddressLine2,
                                    City=paymentDetails?.Payer.PayerAddress.City,
                                    Country =paymentDetails?.Payer.PayerAddress.Country,
                                    PostalCode =paymentDetails?.Payer.PayerAddress.PostalCode,
                                    State = string.IsNullOrEmpty(paymentDetails?.Payer.PayerAddress.Region) ? null : paymentDetails?.Payer.PayerAddress.Region
                                },
                                NameInfo= new NameInfoType
                                {
                                    FirstName =paymentDetails?.Payer.FirstName,
                                    LastName =paymentDetails?.Payer.LastName
                                }
                            }
                        }
                    }
                };
            }
        }

        private static string GetSeatChargeAmount(SeatFulfilmentRequest request)
        {
            var paymentDetails = request.paymentDetails?.FirstOrDefault();
            var isSeatAmountAvailable = request.SelectedSeatInfo.Any(s => !s.Amount.IsNullOrDefault());
            if (isSeatAmountAvailable)
            {
                var totalSeatAmount = request.SelectedSeatInfo.Sum(s => Convert.ToInt32(s.Amount.MonetaryAmount));
                return totalSeatAmount > 0 ? totalSeatAmount.ToString() : paymentDetails?.Amount.MonetaryAmount;
            }
            return paymentDetails?.Amount.MonetaryAmount;
        }
        #endregion

        #region OrderChange
        public static AddRequestType MapOrderAddSeat(OrderChangeRequestData request, Channel channelInfo)
        {
            var additionalDetailType = new AdditionalDetailsType();
            additionalDetailType.PaymentTrackingId = request.OrderAmendments.FirstOrDefault()?.OrderByReference?.FirstOrDefault()?.ItemsForAddition?.OfferRefID;

            var paymentDetails = request.PaymentDetails?.FirstOrDefault();

            return new AddRequestType
            {
                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = channelInfo.DlApplicationId,
                    DebugOptions = "N",
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    HostContextParams = new HostContextParamsType()
                    {
                        PoolType = channelInfo.DlPoolType,
                        Airline = "VS",//request.BookingReferences.AirlineDesignator,
                        City = request.Pos.CityCode,
                        DutyCode = Constants.Application.DutyCode,
                    },
                    TransactionId = request.OrderAmendments.FirstOrDefault()?.OrderByReference.FirstOrDefault()?.ItemsForAddition.OfferRefID,
                    AppChannelName = channelInfo.DlApplicationName,
                    Location = request.Pos.CityCode,
                    Company = "VS",//request.bo.AirlineDesignator,
                    ApplicationVersion = Constants.Application.ApplicationVersion,
                    TurnOffResponseInfo = Constants.Application.TurnOffResponseInfo
                },
                SeatInfo = GetSeatInfo(request),
                FormOfPaymentInfo = paymentDetails?.FormOfPayments == Payment.FormOfPayment.CashEnum ?
                new FormOfPaymentInfoType()
                {
                    CashInfo = new()
                    {
                        Amount = paymentDetails?.Amount.MonetaryAmount,
                        AmountDecimalPlaces = paymentDetails?.Amount.DecimalPrecisionCount.ToString(),
                        CurrencyCode = paymentDetails?.Amount.BaseCurrency
                    },
                    MiscellaneousPaymentInfo = [new() {
                        AccountName = request.Parties?.Participants?.FirstOrDefault()?.Name,
                        AccountNumber = request.Parties?.Participants?.FirstOrDefault()?.AgencyID,
                        Amount = paymentDetails?.Amount.MonetaryAmount,
                        AmountDecimalPlaces = paymentDetails?.Amount.DecimalPrecisionCount.ToString(),
                        CurrencyCode = paymentDetails?.Amount.BaseCurrency
                    }]
                } :
                new FormOfPaymentInfoType()
                {
                    CreditCardInfo = new CreditCardInfoType[]
                    {
                        new CreditCardInfoType()
                        {
                            CardNumber = paymentDetails?.CardMethod.CardNumber,
                            ChargeAmount = paymentDetails?.Amount?.MonetaryAmount?.ToString(),
                            ChargeAmountDecimalPlaces = paymentDetails?.Amount.DecimalPrecisionCount.ToString(),
                            CurrencyCode = paymentDetails?.Amount.BaseCurrency,
                            ExpirationMonth = paymentDetails?.CardMethod.ExpiryDate.Substring(0,2),
                            ExpirationYear =paymentDetails?.CardMethod.ExpiryDate.Substring(3, 2),
                            VendorCode= paymentDetails?.CardMethod.CardType.ToString(),
                            SecurityCode= paymentDetails?.CardMethod.CardType.ToString() == "TP" ? null :paymentDetails?.CardMethod.Cvv,
                            CardHolderInfo= new CustomerInfoType
                            {
                                AddressInfo = new AddressInfoType
                                {
                                    AddressLine1 = paymentDetails?.CardMethod.BillingAddress.AddressLine1,
                                    AddressLine2 = paymentDetails?.CardMethod.BillingAddress.AddressLine2,
                                    City = paymentDetails?.CardMethod.BillingAddress.City,
                                    Country = paymentDetails?.CardMethod.BillingAddress.Country,
                                    PostalCode =paymentDetails?.CardMethod.BillingAddress.PostalCode,
                                    State = string.IsNullOrEmpty(paymentDetails?.CardMethod.BillingAddress.Region) ? null : paymentDetails?.CardMethod.BillingAddress.Region
                                },
                                NameInfo= new NameInfoType
                                {
                                    FirstName = paymentDetails?.Payer.FirstName,
                                    LastName = paymentDetails?.Payer.LastName
                                }
                            }
                        }
                    }
                },
                PointOfSaleInfo = channelInfo?.ChannelId == Constants.Application.NdcChannel ?
                new PointOfSaleInfoType
                {
                    AgencyName = request.Parties.Participants.FirstOrDefault()?.AgencyID,
                    CityCode = request.Pos.CityCode,
                    BartsLocation = request.Pos.CityCode != null ? request.Pos.CityCode + "NDC" : null,
                    CountryCode = request.Pos.CountryCode
                } : new PointOfSaleInfoType
                {
                    AgentId = channelInfo.DlAgentId,
                    CityCode = request.Pos.CityCode,
                    CountryCode = request.Pos.CountryCode
                },
                AdditionalDetails = additionalDetailType
            };
        }

        private static SeatInfoType[] GetSeatInfo(OrderChangeRequestData request)
        {
            var count = 0;
            return request.OrderAmendments.Select(x =>
               new SeatInfoType()
               {
                   OfferId = x.OrderByReference.FirstOrDefault()?.ItemsForAddition.OfferRefID,
                   OfferItemNumber = x.OrderByReference.FirstOrDefault()?.ItemsForAddition.OfferItems.FirstOrDefault()?.OfferItemId,
                   SeatId = GetSeatId(x),
                   AssociationId = $"{++count}"
               }).ToArray();
        }

        private static SeatIdType[] GetSeatId(OrderAmendment orderAmendment)
        {
            return new SeatIdType[]
            {
                new SeatIdType()
                {
                    FlightIdRef = orderAmendment.FlightItems.FirstOrDefault()?.Segments?.SegmentId,
                    SeatNumber = orderAmendment.FlightItems.FirstOrDefault()?.CurrentSeatAssignment,
                }
            };
        }

        #endregion
    }
}
