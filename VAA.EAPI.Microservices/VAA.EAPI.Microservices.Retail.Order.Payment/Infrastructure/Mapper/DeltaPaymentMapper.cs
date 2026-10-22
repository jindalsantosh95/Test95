using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Payment;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentReference;
using VAA.EAPI.Microservices.Retail.Order.Payment.Domain.OrderPayment.PaymentTransaction;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.Air4Payment;
using VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Proxy.PaymentReference;
using static VAA.EAPI.Domain.Virgin.Models.Payment.CardDetails;
using static VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails;

namespace VAA.EAPI.Microservices.Retail.Order.Payment.Infrastructure.Mapper
{
    public class DeltaPaymentMapper
    {
        public static PaymentReferenceRequest PaymentRequestMapper(PaymentRefIdGenerateRequest request)
        {
            if (!request.Selectedoffers.IsNullOrEmpty())
            {
                bool disableSeatFlag = ConfigReader.Try(Constants.Environment.DisableSeatIntegrationFlag, false);

                var passenger = request.CustomerDetails.First(x => x.Ptc == Constants.Application.AdultPtc);
                var paymentDetails = request.paymentAmt.CurrencyEquivalentPrice.Amount;
                decimal argument = Convert.ToDecimal(paymentDetails.MonetaryAmount);
                int count = BitConverter.GetBytes(decimal.GetBits(argument)[3])[2];
                paymentDetails.DecimalPrecisionCount = count;
                string shopType = (request.LoyaltyMemberDetails != null && request.paymentAmt.MilesEquivalentPrice != null) && !request.paymentAmt.MilesEquivalentPrice.CashPlusMiles ? Constants.Application.Award : Constants.Application.Revenue;

                return new PaymentReferenceRequest()
                {
                    AirlineData = new()
                    {
                        Fare = new() { Farebasis = request.Selectedoffers?.First().Services.First()?.FareDetails?.FareBasisCode, FareRestrictionByFlightSeg = disableSeatFlag ? null : GetFareBySeg(request.FlightSegments) },
                        Passengers = MapPassengers(request.CustomerDetails),
                        Product = (Product)request,
                        Profile = new() { Customer = new() { FirstName = passenger.FirstName, LastName = passenger.LastName, LoyaltyId = request?.LoyaltyMemberDetails?.MemberNumber, LoyaltyTier = request?.LoyaltyMemberDetails?.MemberTierCode } },
                        ShopType = shopType,
                        TripType = request.TripType,
                        Trips = MapTripDetails(request.FlightSegments),
                        SeatList = disableSeatFlag ? null : request.SeatDetails?.Select((s, i) => new Proxy.PaymentReference.SeatList()
                        {
                            PassengerSequence = s.PassengerSequence,
                            TripId = s.TripId,
                            TripSeq = (i + 1).ToString(),
                            TripTag = (i + 1).ToString(),
                            PendingSeatNumber = s.PendingSeatNumber,
                            PendingSeatPriceAmount = s.PendingSeatPriceAmount,
                            PendingSeatDecimalPrecisionCount = s.PendingSeatDecimalPrecisionCount,
                            PendingSeatCurrencyCode = s.PendingSeatCurrencyCode,
                            PendingSeatBrandDescription = s.SeatOfferDetails.BrandDescription,
                            PendingSeatProductDescription = s.SeatOfferDetails.ProductDescription,
                            PendingSeatOfferId = s.SeatOfferDetails.DeltaSeatOfferId.Split('|').First(),
                            FreeOfCharge = Convert.ToDecimal(s.PendingSeatPriceAmount) == 0
                        })
                    },
                    CurrencyCode = paymentDetails.Currency,
                    EcommAdditionalData = new() { EmailAddress = passenger.Contacts.Email, IpAddress = request.Pos.IpAddress, IsMoto = false, PhoneNum = passenger.Contacts.Phone },
                    FlightFare = new() { CountryCode = request.Pos.CountryCode, CurrencyAmt = paymentDetails.MonetaryAmount, CurrencyCode = paymentDetails.Currency, CurrencyNbr = request.Pos.CurrencyNumber },
                    PaymentAmt = new()
                    {
                        CurrencyEquivalentPrice = GetTotalPaymentAmount(paymentDetails, request.SeatDetails, request.InsuranceDetails?.Amount?.MonetaryAmount ?? "0"),
                        MilesEquivalentPrice = request.paymentAmt.MilesEquivalentPrice != null ? new Proxy.PaymentReference.MilesEquivalentPrice()
                        {
                            CashPlusMiles = request.paymentAmt.MilesEquivalentPrice.CashPlusMiles,
                            MileCnt = request.paymentAmt.MilesEquivalentPrice.MileCount
                        } : null
                    },
                    AdditionalConfigParams = new() { FailureRedirectUrl = request.AdditionalConfigParams.FailureRedirectUrl, MultiProductFlag = request.SeatDetails != null && request.SeatDetails.Any(x => Convert.ToDecimal(x.PendingSeatPriceAmount) > 0), SessionID = request.AdditionalConfigParams.SessionID, SuccessRedirectUrl = request.AdditionalConfigParams.SuccessRedirectUrl, InsuranceFlag = request.AdditionalConfigParams.IsInsuranceAdded ?? false }
                };
            }
            else
            {
                var passenger = request.CustomerDetails.First();
                return new PaymentReferenceRequest()
                {

                    AirlineData = new()
                    {
                        Fare = request.IsBagsPayment ? null : new() { Farebasis = request.Selectedoffers?.First().Services.First()?.FareDetails?.FareBasisCode?.Replace("/",""), FareRestrictionByFlightSeg = GetFareBySeg(request.FlightSegments) },
                        Passengers = MapPassengers(request.CustomerDetails),
                        Product = (Product)request,
                        Profile = new() { Customer = new() { FirstName = passenger.FirstName, LastName = passenger.LastName, LoyaltyId = request?.LoyaltyMemberDetails?.MemberNumber, LoyaltyTier = request?.LoyaltyMemberDetails?.MemberTierCode } },
                        Trips = MapTripDetails(request.FlightSegments),
                        SeatList = request.SeatDetails?.Select((s, i) => new Proxy.PaymentReference.SeatList()
                        {
                            PassengerSequence = s.PassengerSequence,
                            TripId = s.TripId,
                            TripSeq = (i + 1).ToString(),
                            TripTag = (i + 1).ToString(),
                            PendingSeatNumber = s.PendingSeatNumber,
                            PendingSeatPriceAmount = s.PendingSeatPriceAmount,
                            PendingSeatDecimalPrecisionCount = s.PendingSeatDecimalPrecisionCount,
                            PendingSeatCurrencyCode = s.PendingSeatCurrencyCode,
                            PendingSeatBrandDescription = s.SeatOfferDetails.BrandDescription,
                            PendingSeatProductDescription = s.SeatOfferDetails.ProductDescription,
                            PendingSeatOfferId = s.SeatOfferDetails.DeltaSeatOfferId.Split('|').First(),
                            FreeOfCharge = Convert.ToDecimal(s.PendingSeatPriceAmount) == 0
                        }),
                        Bags = request.BagsDetails?.Select((bag, i) => new Proxy.PaymentReference.Bags()
                        {
                            PassengerSequence = bag.PassengerSequence,
                            TripSeq = (i + 1).ToString(),
                            TripTag = (i + 1).ToString(),
                            ProductDescription = bag.ProductDescription,
                            ProductCurrencyCode = bag.ProductCurrencyCode,
                            ProductCurrencyAmt = bag.ProductCurrencyAmt,
                            ProductDecimalPrecisionCount = bag.ProductDecimalPrecisionCount,
                            ProductCode = bag.ProductCode,
                            ProductCategory = bag.ProductCategory,
                            ProductItem = bag.ProductItem
                        }),
                        ShopType = (request.LoyaltyMemberDetails != null && request.paymentAmt.MilesEquivalentPrice != null) && !request.paymentAmt.MilesEquivalentPrice.CashPlusMiles ? Constants.Application.Award : Constants.Application.Revenue,
                        TripType = request.TripType,
                    },
                    CurrencyCode = request.Pos?.CurrencyCode ?? request.SeatDetails.First().PendingSeatCurrencyCode,
                    EcommAdditionalData = new() { EmailAddress = passenger.Contacts.Email, IpAddress = request.Pos.IpAddress, IsMoto = request.IsBagsPayment ? null : false, PhoneNum = passenger.Contacts.Phone },
                    FlightFare = new() { CountryCode = request.Pos.CountryCode, CurrencyAmt = request.IsBagsPayment ? request?.paymentAmt?.CurrencyEquivalentPrice?.Amount?.MonetaryAmount : "0", CurrencyCode = request.Pos.CurrencyCode, CurrencyNbr = request.Pos.CurrencyNumber },
                    PaymentAmt = new()
                    {
                        CurrencyEquivalentPrice = GetTotalPaymentAmount(request.paymentAmt.CurrencyEquivalentPrice.Amount, request.SeatDetails, null)
                    },
                    AdditionalConfigParams = new() { FailureRedirectUrl = request.AdditionalConfigParams.FailureRedirectUrl, MultiProductFlag = (request.SeatDetails != null && request.SeatDetails.Any(x => Convert.ToDecimal(x.PendingSeatPriceAmount) > 0)) || (request.BagsDetails != null && request.BagsDetails.Any(x => Convert.ToDecimal(x.ProductCurrencyAmt) > 0)), SessionID = request.AdditionalConfigParams.SessionID, SuccessRedirectUrl = request.AdditionalConfigParams.SuccessRedirectUrl, InsuranceFlag = false }
                };
            }

        }

        private static Proxy.PaymentReference.CurrencyEquivalentPrice GetTotalPaymentAmount(Amount paymentDetails, IEnumerable<Domain.OrderPayment.PaymentReference.SeatList> seatLists, string insurancePrice)
        {
            decimal totalSeatAmount = 0;
            decimal amount = 0;

            if (seatLists != null && seatLists.Any())
                totalSeatAmount = seatLists.Sum(seatList => decimal.Parse(seatList.PendingSeatPriceAmount));

            if (!paymentDetails.IsNullOrDefault() && !paymentDetails.MonetaryAmount.IsNullOrEmpty())
            {
                amount = Convert.ToDecimal(paymentDetails.MonetaryAmount) + totalSeatAmount + Convert.ToDecimal(insurancePrice);

                return new()
                {
                    CurrencyAmt = (int)(amount * (decimal)Math.Pow(10, paymentDetails.DecimalPrecisionCount)),
                    CurrencyCode = paymentDetails.Currency,
                    DecimalPrecisionCnt = paymentDetails.DecimalPrecisionCount
                };
            }
                
            else
            {
                return new()
                {
                    CurrencyAmt = (int)(totalSeatAmount * (decimal)Math.Pow(10, Convert.ToInt32(seatLists.FirstOrDefault()?.PendingSeatDecimalPrecisionCount))),
                    CurrencyCode = seatLists.FirstOrDefault()?.PendingSeatCurrencyCode,
                    DecimalPrecisionCnt = Convert.ToInt32(seatLists.FirstOrDefault()?.PendingSeatDecimalPrecisionCount)
                };
            }
        }

        private static IEnumerable<FareRestrictionByFlightSeg> GetFareBySeg(IEnumerable<Segment> segments)
        {
            return segments.Select(fs => new FareRestrictionByFlightSeg()
            {
                OriginAirportCode = fs.OriginAirportCode,
                DestAirportCode = fs.DestinationAirportCode,
                FareBasisCode = new List<string>() { fs.FareBasis?.Replace("/", "") }
            });
        }

        private static IEnumerable<Passenger> MapPassengers(IEnumerable<EAPI.Domain.Virgin.Models.Customer.Customer> customers)
        {
            return customers.Select((customer, i) => new Passenger()
            {
                FirstName = customer.FirstName,
                LastName = customer.LastName,
                PassengerSequence = i + 1,
                DateOfBirth = customer.DateOfBirth
            });
        }

        private static IEnumerable<Trip> MapTripDetails(IEnumerable<Segment> segments)
        {
            return segments.Select((trip, i) => new Trip()
            {
                FlightLegs = MapFlightLegDetails(trip),
                TripSeq = i + 1,
                TripTag = i + 1
            });
        }

        private static IEnumerable<FlightLeg> MapFlightLegDetails(Segment segment)
        {
            return segment.Legs.Select(leg => new FlightLeg()
            {
                ArrivalTime = Convert.ToDateTime(leg.ScheduledArrivalTime.LocalDateTime),
                DepartureTime = Convert.ToDateTime(leg.ScheduledDepartureTime.LocalDateTime),
                BookingClass = segment.ClassOfService,
                DestinationAirportCode = leg.DestinationAirportCode,
                FlightNumber = segment.MarketingFlightNumber,
                MarketingCarrierCode = segment.MarketingCarrier,
                OperatingCarrierCode = segment.OperatingCarrier,
                OriginAirportCode = leg.OriginAirportCode
            });
        }        

        public static PaymentTransactionDetails PaymentDetailsResponseMapper(Air4PaymentDetails paymentDetails)
        {
            var priceInfo = paymentDetails?.Price?.CurrencyEquivalentPrice;

            return new PaymentTransactionDetails()
            {
                PaymentReferenceId = paymentDetails.PaymentReferenceId,
                PaymentDetails = new PaymentDetails[]
                   {
                       new PaymentDetails
                       {
                           FormOfPaymentId = Constants.Application.FormOfPaymentId,
                           FormOfPayments = FormOfPayment.CardEnum,
                           CardMethod =  new CardDetails
                           {
                                 CardNumber =paymentDetails.PaymentMethod.PaymentCard.PaymentCardNum,
                                 Cvv =paymentDetails.PaymentMethod.PaymentCard.CardVerificationValueNum,
                                 ExpiryDate = $"{paymentDetails.PaymentMethod.PaymentCard.ExpirationMonthNum}-{paymentDetails.PaymentMethod.PaymentCard.ExpirationYearNum}",
                                 CardType= (CardTypes)Enum.Parse(typeof(CardTypes), paymentDetails.PaymentMethod.PaymentCard.PaymentCardTypeCode) ,
                                 IsTokenised=true,
                           },
                           Payer = new PaymentPayer
                           {
                                 FirstName=paymentDetails.PaymentMethod.PaymentCard.CardHolderName.FirstName,
                                 LastName=paymentDetails.PaymentMethod.PaymentCard.CardHolderName.LastName
                           },
                           Amount = priceInfo == null ? null : new EAPI.Domain.Virgin.Models.Common.CurrencyAmount
                           {
                               BaseCurrency = priceInfo.CurrencyCode,
                               MonetaryAmount = Convert.ToString(priceInfo.CurrencyAmt),
                               DecimalPrecisionCount = priceInfo.DecimalPrecisionCnt
                           },
                           BillingAddress = new EAPI.Domain.Virgin.Models.Customer.Address
                           {
                                 AddressLine1=paymentDetails.PaymentMethod.BillingAddress.AddressLine1Text,
                                 AddressLine2=paymentDetails.PaymentMethod.BillingAddress.AddressLine2Text,
                                 City=paymentDetails.PaymentMethod.BillingAddress.CityLocalityName,
                                 Country=paymentDetails.PaymentMethod.BillingAddress.CountryCode,
                                 PostalCode=paymentDetails.PaymentMethod.BillingAddress.PostalCode,
                                 Region = paymentDetails.PaymentMethod.BillingAddress?.CountrySubdivisionCode
                           }
                       }
                   }
            };
        }

        public static PaymentReferenceRequest MaskSensitiveData(PaymentReferenceRequest paymentReferenceRequest)
        {
            var paymentReferenceClonedRequest = paymentReferenceRequest.DeepCopy();

            string maskedString = string.Empty;
            paymentReferenceClonedRequest.AirlineData.Passengers.ToList().ForEach(s =>
                {
                    s.FirstName = maskedString.PadLeft(s.FirstName.Length, 'X');
                    s.LastName = maskedString.PadLeft(s.LastName.Length, 'X');
                    s.DateOfBirth = maskedString.PadLeft((s.DateOfBirth ?? "").Length , 'X');
                });

            var customer = paymentReferenceClonedRequest.AirlineData.Profile.Customer;
            var ecommAdditionalData = paymentReferenceClonedRequest.EcommAdditionalData;

            customer.FirstName = maskedString.PadLeft(customer.FirstName.Length, 'X');
            customer.LastName = maskedString.PadLeft(customer.LastName.Length, 'X');
            ecommAdditionalData.PhoneNum = maskedString.PadLeft(ecommAdditionalData.PhoneNum.IsNullOrEmpty() ? 0 : ecommAdditionalData.PhoneNum.Length, 'X');
            ecommAdditionalData.IpAddress = maskedString.PadLeft(ecommAdditionalData.IpAddress.IsNullOrEmpty() ? 0 :ecommAdditionalData.IpAddress.Length, 'X');
            ecommAdditionalData.EmailAddress = maskedString.PadLeft(ecommAdditionalData.EmailAddress.IsNullOrEmpty() ? 0 :ecommAdditionalData.EmailAddress.Length, 'X');

            return paymentReferenceClonedRequest;
        }

        public static PaymentTransactionDetails FormsOfPaymentsResponseMapper(Air4formsOfPaymentsResponse air4formsOfPaymentsResponse)
        {
            var paymentDetails = air4formsOfPaymentsResponse.formsOfPayments.First();
            var cardDetails = paymentDetails.PaymentMethod.PaymentCard;
            var billingAddress = paymentDetails?.PaymentMethod?.PaymentCard?.BillingAdr;
            var authorizationData = paymentDetails?.Authorization;
            var priceInfo = paymentDetails?.Price?.CurrencyEquivalentPrice;

            return new PaymentTransactionDetails()
            {
                PaymentReferenceId = paymentDetails.PaymentReferenceId,
                PaymentDetails = new PaymentDetails[]
                   {
                       new PaymentDetails
                       {
                           FormOfPaymentId = Constants.Application.FormOfPaymentId,
                           FormOfPayments = paymentDetails.PaymentMethod.PaymentMethodTypeName == Constants.Application.DlformsOfPaymentsApplePay || paymentDetails.PaymentMethod.PaymentMethodTypeName == Constants.Application.DlApplePay ? FormOfPayment.ApplePay : paymentDetails.PaymentMethod.PaymentMethodTypeName == ConfigReader.Get(Constants.Environment.DlformsOfPaymentsGooglePay) || paymentDetails.PaymentMethod.PaymentMethodTypeName == ConfigReader.Get(Constants.Environment.DlGooglePay) ? FormOfPayment.GooglePay: FormOfPayment.PayPal,
                           CardMethod = cardDetails == null ? null : new CardDetails
                           {
                                 CardNumber = cardDetails.PaymentCardNum,
                                 Cvv = cardDetails.CardVerificationValueNum,
                                 ExpiryDate = cardDetails.ExpirationMonthNum == null && cardDetails.ExpirationYearNum == null? null : $"{cardDetails.ExpirationMonthNum}-{cardDetails.ExpirationYearNum}",
                                 CardType = cardDetails?.PaymentCardTypeCode == null ? null : (CardTypes)Enum.Parse(typeof(CardTypes), cardDetails?.PaymentCardTypeCode) ,
                                 IsTokenised=true
                           },
                           Payer = cardDetails == null ? null : new PaymentPayer
                           {
                                 FirstName = cardDetails.CardHolderName.FirstName,
                                 LastName = cardDetails.CardHolderName.LastName
                           },
                           BillingAddress =  billingAddress ==  null ? null : new EAPI.Domain.Virgin.Models.Customer.Address
                           {
                                 AddressLine1 = billingAddress.AddressLine1Text,
                                 AddressLine2 = billingAddress.AddressLine2Text,
                                 City = billingAddress.CityLocalityName,
                                 Country = billingAddress.CountryCode,
                                 PostalCode = billingAddress.PostalCode,
                                 Region = billingAddress?.CountrySubdivisionCode
                           },
                           PaymentRemarks = paymentDetails.PaymentMethod.PaymentPurchaseAdvisoryMessage == null ? null : paymentDetails.PaymentMethod?.PaymentPurchaseAdvisoryMessage?.Remarks.Select((paymentRemarks, i) => new EAPI.Domain.Virgin.Models.Common.Remark()
                           {
                                 ItemType = EAPI.Domain.Virgin.Models.Common.Remark.RemarksType.OSI,
                                 Text = paymentRemarks.MessageText,
                                 Code =  paymentRemarks.RemarkTypeCode
                           }),
                           PaymentCardAuthorization = authorizationData == null ? null : new PaymentCardAuthorization
                           {
                               ApprovalCode = authorizationData.ApprovalCode,
                               SourceName = authorizationData.SourceName,
                               ResponseCode = authorizationData.ResponseCode,
                               AcquirerReferenceDataText =  authorizationData.FopCode,
                               TransactionCurrencyCode = authorizationData.TransactionCurrencyCode,
                               VendorPaymentTransactionId = authorizationData?.VendorTransactionId
                           },
                           ParentPaymentReferenceId = paymentDetails.ParentPaymentReferenceId,
                           Amount = priceInfo == null ? null : new EAPI.Domain.Virgin.Models.Common.CurrencyAmount
                           {
                               BaseCurrency = priceInfo.CurrencyCode,
                               MonetaryAmount = Convert.ToString(priceInfo.CurrencyAmt)
                           },
                           FinancialInstitution =  paymentDetails.PaymentMethod.PaymentMethodTypeName == Constants.Application.DlformsOfPaymentsApplePay || paymentDetails.PaymentMethod.PaymentMethodTypeName == ConfigReader.Get(Constants.Environment.DlformsOfPaymentsGooglePay) ? null :  new FinancialInstitution
                           {
                               AccountHolderName = ConfigReader.Get(Constants.Environment.MspFinancialInstAccountHolderName),
                               Address = new()
                               {
                                   Country =  ConfigReader.Get(Constants.Environment.MspFinancialInstcountryCode),
                               },
                               ProviderId =  ConfigReader.Get(Constants.Environment.MspFinancialInstProviderId),
                               ReturnUrl =  ConfigReader.Get(Constants.Environment.MspFinancialInstReturnUrl)
                           }
                       }

                   }
            };
        }

    }
}
