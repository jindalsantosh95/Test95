using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Domain.Virgin.Models.Ticket;
using VAA.EAPI.Infrastructure.Helper;
using VAA.EAPI.Microservices.Retail.Order.Domain.Order;
using VAA.EAPI.Microservices.Retail.Order.Infrastructure.Proxy.DeltaOrder;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using static VAA.EAPI.Domain.Virgin.Models.Payment.PaymentDetails;
using BookingReference = VAA.EAPI.Domain.Virgin.Models.Order.BookingReference;
using CommonEntities = VAA.EAPI.Domain.Entities.Common;
using Price = VAA.EAPI.Domain.Virgin.Models.Common.Price;
using State = VAA.EAPI.Domain.Virgin.Models.Common.State;
using UtcAndLocalDateTime = VAA.EAPI.Domain.Virgin.Models.Common.UtcAndLocalDateTime;

namespace VAA.EAPI.Microservices.Retail.Order.Infrastructure.Mapper.FlightOrderCreate
{
    public class DeltaOrderMapper
    {
        public static DlOrderRequest OrderRequestMapper(OrderCreateRequest flightOrderCreateRequest)
        {
            return new DlOrderRequest
            {
                PointOfSale = new PointOfSale
                {
                    countryCode = flightOrderCreateRequest.Pos.CountryCode,
                    pointOfSaleCityCode = flightOrderCreateRequest.Pos.CityCode,
                    soldByTravelAgency = true,
                    deviceTypeText = !string.IsNullOrEmpty(flightOrderCreateRequest.Pos.DeviceInfo) ? flightOrderCreateRequest.Pos.DeviceInfo : Constants.Application.DeviceTypeText,
                    ipAddressText = !string.IsNullOrEmpty(flightOrderCreateRequest.Pos.IpAddress) ? flightOrderCreateRequest.Pos.IpAddress : Constants.Application.IpAddressText,
                    browserTypeText = !string.IsNullOrEmpty(flightOrderCreateRequest.Pos.BrowserInfo) ? flightOrderCreateRequest.Pos.BrowserInfo : Constants.Application.BrowserTypeText,
                },
                SelectedOffers = MapSelectedOffers(flightOrderCreateRequest),
                FormsOfPayment = MapFormsOfPayment(flightOrderCreateRequest),
                Customers = MapCustomers(flightOrderCreateRequest)
            };
        }

        private static IEnumerable<SelectedOffer> MapSelectedOffers(OrderCreateRequest flightOrderCreateRequest)
        {
            var selectedOfferItems = flightOrderCreateRequest.SelectedOffers.Select(offer =>
            {
                if (offer.ProductType != "SEAT")
                {
                    var offerPrice = offer.SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum);

                    return new SelectedOffer()
                    {
                        OfferId = offer.OfferId,
                        SelectedOfferItems = offer.OfferItems.Select(offerItem => new SelectedOfferItem { OfferItemId = offerItem.OfferItemId }),
                        TotalOfferAmt = new TotalOfferAmt
                        {
                            CurrencyEquivalentPrice = new CurrencyEquivalentPrice
                            {
                                CurrencyAmt = Convert.ToDouble(offerPrice.Amount.MonetaryAmount),
                                CurrencyCode = offerPrice.Amount.BaseCurrency,
                                CountryCode = flightOrderCreateRequest.Pos.CountryCode,
                                DecimalPrecisionCnt = BitConverter.GetBytes(decimal.GetBits(Convert.ToDecimal(offerPrice.Amount.MonetaryAmount))[3])[2]
                            }
                        },
                        FormOfPaymentId = flightOrderCreateRequest.PaymentDetails.Select(fop => fop.FormOfPaymentId),
                        Remarks = flightOrderCreateRequest.PaymentDetails?.First().PaymentRemarks == null ? null : MapRemarks(flightOrderCreateRequest.PaymentDetails),
                        OrderCriteria = new()
                        {
                            flightOrderCriteria = new()
                            {
                                UpgradePreferenceText = Constants.Application.UpgradePreferenceText,

                                CustomerConsentInfo = flightOrderCreateRequest.CreditCardCheckConsent ? new List<string>()
                                {
                                    Constants.Application.CCCONSENTOBTAINED
                                } : null
                            }
                        }
                    };
                }
                else
                {
                    var totalPrice = offer.SalePrice.First().Amount;
                    return new SelectedOffer
                    {
                        OfferId = offer.OfferId,
                        SelectedOfferItems = offer.OfferItems.Select(offerItem =>
                        {
                            var offerService = offerItem.Services.First();
                            var offerItemPricing = offerItem.OfferitemPricing.First().SalePrice.First().Amount;
                            return new SelectedOfferItem()
                            {
                                OfferItemId = offerItem.OfferItemId,
                                OfferItemTotalAmt = new OfferItemTotalAmt()
                                {
                                    CurrencyEquivalentPrice = new CurrencyEquivalentPrice()
                                    {
                                        CurrencyAmt = Convert.ToDouble(offerItemPricing.MonetaryAmount),
                                        CurrencyCode = offerItemPricing.BaseCurrency,
                                        DecimalPrecisionCnt = offerItemPricing.DecimalPrecisionCount,
                                    }
                                },
                                RetailItemMetaData = new RetailItemMetaData()
                                {
                                    SeatInformation = new Proxy.DeltaOrder.SeatInformation()
                                    {
                                        PassengerId = offerService.ServiceBeneficiaries.First(),
                                        FlightLegId = offerService.SeatInformation.FlightLegId,
                                        SeatLocation = new SeatLocation()
                                        {
                                            SeatColumnName = offerService.SeatInformation.SeatColumn,
                                            SeatProductCode = offerService.SeatInformation.SeatProductCode,
                                            SeatRowNum = offerService.SeatInformation.SeatRowNumber,
                                            EconomyComfort = Convert.ToDouble(offerItemPricing.MonetaryAmount) > 0 ? true : false,
                                            ExitOverride = offerService.SeatInformation.ExitOverride,
                                            ExitRow = offerService.SeatInformation.ExitRow
                                        }
                                    }
                                }
                            };
                        }),
                        TotalOfferAmt = new TotalOfferAmt()
                        {
                            CurrencyEquivalentPrice = new CurrencyEquivalentPrice()
                            {
                                CurrencyAmt = Convert.ToDouble(totalPrice.MonetaryAmount),
                                CurrencyCode = totalPrice.BaseCurrency,
                                DecimalPrecisionCnt = totalPrice.DecimalPrecisionCount
                            }
                        },
                        FormOfPaymentId = new List<string> { flightOrderCreateRequest.PaymentDetails.First().FormOfPaymentId }

                    };
                }

            });

            return selectedOfferItems;
        }

        public static IEnumerable<Remark> MapRemarks(IEnumerable<EAPI.Domain.Virgin.Models.Payment.PaymentDetails> paymentDetails)
        {
            return paymentDetails.First().PaymentRemarks.Select(x => new Remark()
            {
                MessageText = x.Text.Contains(Constants.Application.RemarkText) ? paymentDetails.First().FormOfPayments switch
                {
                    FormOfPayment.ApplePay => $"{x.Text} {Constants.Application.ApplePayCode}",
                    FormOfPayment.PayPal => $"{x.Text} {Constants.Application.PayPalCode}",
                    FormOfPayment.GooglePay => $"{x.Text} {ConfigReader.Get(Constants.Environment.GooglePayCode)}",
                    _ => null
                } : x.Text,
                RemarkTypeCode = x.Code
            });
        }

        public static OrderResponse OrderResponseMapper(DlOrderResponse dlOrderResponse, OrderCreateRequest orderCreateRequest)
        {
            var order = dlOrderResponse.Orders.First();

            return new OrderResponse
            {
                OrderNumber = new OrderNumber { OrderId = order.OrderId, OwnerAirline = order.OrderOwner.OwnerCode },
                BookingReference = new BookingReference { RLOC = order.BookingReference.First().RecordLocatorId, AirlineDesignator = dlOrderResponse.Orders.First().BookingReference.First().CarrierCode, RLOCCreateDate = dlOrderResponse.Orders.First().BookingReference.First().BookingCreationDate.ToString(Constants.Application.ZuluDateTimeFormat) },
                PointOfSale = orderCreateRequest.Pos,
                CreationDateTime = new UtcAndLocalDateTime { UtcDateTime = order.OrderItems.First().OrderItemCreationUtcTs.ToString(Constants.Application.ZuluDateTimeFormat) },
                PaymentDetails = orderCreateRequest.PaymentDetails,
                DataList = new DataList
                {
                    CustomerDetails = MapCustomerDetails(order),
                    OriginDestinationAssociations = MapOriginDestinationAssociations(order),
                    SegmentDetails = MapSegments(order)
                },
                OrderItems = MapOrderItems(order, orderCreateRequest),
                Warnings = MapOrderWarnings(order),
                BookingType = order.OrderProperties?.FlightOrderProperties?.OrderBookingTypeText,
                OrderType = order.OrderProperties?.FlightOrderProperties?.OrderTypeText
            };
        }

        private static IEnumerable<CommonEntities.Warning> MapOrderWarnings(Proxy.DeltaOrder.Order dlOrderResponse)
        {
            var warnings = dlOrderResponse.Warnings != null && dlOrderResponse.Warnings.Any(proximity => proximity.WarningCode == Constants.Application.WarningDeltaProximity)
                ? WarningMapper.MapWarnings(Constants.Application.WarningSEATNONCOMPLIANTPROXIMITY) : null;

            if (dlOrderResponse.OrderItems.Any(oi => oi.ProductCategoryId == Constants.Application.DeltaProductCategoryIdSeats))
            {
                if (dlOrderResponse.OrderItems.Where(oi => oi.ProductCategoryId == Constants.Application.DeltaProductCategoryIdSeats).All(seat => !seat.Errors.IsNullOrEmpty() || seat.RetailItems.First().RetailItemMetaData.SeatInformation.SeatLocation.SeatStatus is null || seat.RetailItems.First().RetailItemMetaData.SeatInformation.SeatLocation.SeatStatus == "FAILED"))
                    warnings = warnings is null ? WarningMapper.MapWarnings(Constants.Application.WarningALLSEATFULFILMENTFAILED) : warnings.Concat(WarningMapper.MapWarnings(Constants.Application.WarningALLSEATFULFILMENTFAILED));
                else if (dlOrderResponse.OrderItems.Where(oi => oi.ProductCategoryId == Constants.Application.DeltaProductCategoryIdSeats).Any(seat => !seat.Errors.IsNullOrEmpty() || seat.RetailItems.First().RetailItemMetaData.SeatInformation.SeatLocation.SeatStatus is null || seat.RetailItems.First().RetailItemMetaData.SeatInformation.SeatLocation.SeatStatus == "FAILED"))
                    warnings = warnings is null ? WarningMapper.MapWarnings(Constants.Application.WarningPARTIALSEATFULFILMENT) : warnings.Concat(WarningMapper.MapWarnings(Constants.Application.WarningPARTIALSEATFULFILMENT));
            }

            return warnings;
        }

        private static IEnumerable<FormsOfPayment> MapFormsOfPayment(OrderCreateRequest flightOrderCreateRequest)
        {
            var formsOfPayments = new List<FormsOfPayment>();

            foreach (var payment in flightOrderCreateRequest.PaymentDetails)
            {
                if (payment.FormOfPayments != FormOfPayment.MilesPoint)
                {
                    string[] splitExpiryDate = payment.CardMethod?.ExpiryDate == null ? null : payment.CardMethod.ExpiryDate.Split("-");

                    var formsOfPayment = new FormsOfPayment
                    {
                        FormOfPaymentId = payment.FormOfPaymentId,
                        PaymentMethod = new PaymentMethod
                        {
                            PaymentMethodTypeName = payment.FormOfPayments == FormOfPayment.MoneyPlusMilesPartial || payment.FormOfPayments == FormOfPayment.MoneyPlusMilesFull ? null : payment.FormOfPayments switch
                            {
                                FormOfPayment.ApplePay => Constants.Application.DlApplePay,
                                FormOfPayment.PayPal => Constants.Application.DlPayPal,
                                FormOfPayment.CardEnum => Constants.Application.DlCard,
                                FormOfPayment.GooglePay => ConfigReader.Get(Constants.Environment.DlGooglePay),
                                _ => string.Empty,
                            },
                            PaymentCard = payment.FormOfPayments == FormOfPayment.PayPal || payment.FormOfPayments == FormOfPayment.MoneyPlusMilesFull || payment.FormOfPayments == FormOfPayment.MoneyPlusMilesPartial ? null : new PaymentCard
                            {
                                BillingAdr = new BillingAdr { AddressLine1Text = payment.CardMethod?.BillingAddress?.AddressLine1, CityLocalityName = payment.CardMethod?.BillingAddress?.City, CountryCode = payment.CardMethod?.BillingAddress?.Country, CountrySubdivisionCode = payment.CardMethod?.BillingAddress?.Region ?? string.Empty, PostalCode = payment.CardMethod?.BillingAddress?.PostalCode },
                                CardHolderName = new CardHolderName { FirstName = payment.Payer.FirstName, LastName = payment.Payer.LastName },
                                ExpirationMonthNum = splitExpiryDate?[0],//MM
                                ExpirationYearNum = splitExpiryDate?[1],//YY
                                CardVerificationValueNum = payment.CardMethod?.Cvv,
                                PaymentCardNum = payment.CardMethod?.CardNumber,
                                PaymentCardNetworkCode = payment.CardMethod?.CardType?.ToString(),
                                PaymentCardLast4Num = ""
                            },
                            BillingAddress = payment.FormOfPayments != FormOfPayment.PayPal || payment.FormOfPayments == FormOfPayment.MoneyPlusMilesPartial || payment.FormOfPayments == FormOfPayment.MoneyPlusMilesFull ? null : new()
                            {
                                AddressLine1Text = payment.CardMethod?.BillingAddress?.AddressLine1,
                                CityLocalityName = payment.CardMethod?.BillingAddress?.City,
                                CountryCode = payment.CardMethod?.BillingAddress?.Country,
                                CountrySubdivisionCode = payment.CardMethod?.BillingAddress?.Region ?? string.Empty,
                                PostalCode = payment.CardMethod?.BillingAddress?.PostalCode
                            },
                            FinancialInstitution = payment?.FinancialInstitution == null ? null : new()
                            {
                                FinancialInstitutionAddress = new() { CountryCode = payment?.FinancialInstitution?.Address?.Country },
                                FinancialInstitutionProviderId = payment?.FinancialInstitution?.ProviderId,
                                FinancialInstitutionReturnUrl = payment?.FinancialInstitution?.ReturnUrl,
                                AccountHolderName = payment?.FinancialInstitution?.AccountHolderName
                            },
                            Miles = payment?.MilesPoints == null || payment?.MilesEquivalentPrice == null ? null : new()
                            {
                                PerPassengerDiscountAmt = new()
                                {
                                    CurrencyEquivalentPrice = new()
                                    {
                                        CurrencyAmt = Convert.ToDouble(payment.MilesEquivalentPrice.MonetaryAmount) / flightOrderCreateRequest.CustomerDetails.Count(c => c.Ptc == payment.Payer.PayerType),
                                        CurrencyCode = payment.MilesEquivalentPrice.BaseCurrency,
                                        DecimalPrecisionCnt = BitConverter.GetBytes(decimal.GetBits(Convert.ToDecimal(payment.MilesEquivalentPrice.MonetaryAmount))[3])[2]
                                    },
                                    MilesEquivalentPrice = new()
                                    {
                                        CashPlusMiles = true,
                                        DiscountMileCnt = 0,
                                        MileCnt = Convert.ToInt32(payment.MilesPoints) / flightOrderCreateRequest.CustomerDetails.Count(c => c.Ptc == payment.Payer.PayerType)
                                    }
                                },
                                PassengerCnt = flightOrderCreateRequest.CustomerDetails.Count(c => c.Ptc == payment.Payer.PayerType),
                                PassengerTypeCode = payment.Payer.PayerType,
                                PayWithMilesCode = payment.FormOfPayments switch
                                {
                                    FormOfPayment.MoneyPlusMilesFull => Constants.Application.PayWithMilesFull,
                                    FormOfPayment.MoneyPlusMilesPartial => Constants.Application.PayWithMilesPartial,
                                    _ => string.Empty
                                },
                                LoyaltyMemberId = payment.Payer.LoyaltyMemberDetails.MemberNumber,
                                LoyaltyMemberOwnerCode = "VS",
                                LoyaltyMemberName = new() { FirstName = payment.Payer.FirstName, LastName = payment.Payer.LastName },
                                LoyaltyProgramTierCode = payment.Payer.LoyaltyMemberDetails.MemberType,
                                LoyaltyMemberTravelling = payment.Payer.LoyaltyMemberTravelling
                            }
                        },
                        Price = new Proxy.DeltaOrder.Price
                        {
                            CurrencyEquivalentPrice = payment?.MilesPoints != null || payment?.MilesEquivalentPrice != null ? null : new CurrencyEquivalentPrice
                            {
                                CurrencyAmt = Convert.ToDouble(flightOrderCreateRequest.PaymentDetails?.First().Amount.MonetaryAmount),
                                CurrencyCode = flightOrderCreateRequest.PaymentDetails?.First().Amount.BaseCurrency,
                                CountryCode = flightOrderCreateRequest.Pos.CountryCode,
                                DecimalPrecisionCnt = BitConverter.GetBytes(decimal.GetBits(Convert.ToDecimal(flightOrderCreateRequest.PaymentDetails?.First().Amount.MonetaryAmount))[3])[2]
                            },
                            MilesEquivalentPrice = payment?.MilesPoints == null || payment?.MilesEquivalentPrice == null ? null : new()
                            {
                                CashPlusMiles = true,
                                DiscountMileCnt = 0,
                                MileCnt = Convert.ToInt32(payment.MilesPoints)
                            }
                        },
                        PaymentReferenceId = payment.PaymentReferenceId,
                        AuthenticationData = payment?.PaymentCardAuthorization == null ? null : new() { PaymentReferenceText = payment?.PaymentCardAuthorization?.AcquirerReferenceDataText, PaymentVendorTransactionId = payment?.PaymentCardAuthorization?.VendorPaymentTransactionId }
                    };
                    formsOfPayments.Add(formsOfPayment);
                }
                else
                {
                    var payerDetails = payment.Payer.LoyaltyMemberDetails;
                    var formsOfPayment = new FormsOfPayment
                    {
                        PaymentMethod = new PaymentMethod
                        {
                            Miles = new Miles { PassengerTypeCode = payment.Payer.PayerType, LoyaltyMemberId = payerDetails.MemberNumber, LoyaltyMemberName = new LoyaltyMemberName { FirstName = payment.Payer.FirstName, LastName = payment.Payer.LastName }, LoyaltyMemberTravelling = payment.Payer.LoyaltyMemberTravelling }
                        },
                        Price = new Proxy.DeltaOrder.Price
                        {
                            MilesEquivalentPrice = new MilesEquivalentPrice
                            {
                                CashPlusMiles = false,
                                DiscountMileCnt = 0,
                                MileCnt = Convert.ToInt32(payment.MilesPoints)
                            }
                        }
                    };
                    formsOfPayments.Add(formsOfPayment);
                }
            }
            return formsOfPayments;
        }

        private static IEnumerable<Customer> MapCustomers(OrderCreateRequest orderCreateRequest)
        {
            var customers = new List<Customer>();

            foreach (var customerDetail in orderCreateRequest.CustomerDetails)
            {
                customers.Add(new Customer
                {
                    BirthDate = customerDetail.DateOfBirth,
                    EmailAdr = customerDetail.Contacts.Email,
                    GenderCode = customerDetail.Gender,
                    PersonName = new PersonName { FirstName = customerDetail.FirstName, LastName = customerDetail.LastName },
                    Phones = new List<Phone> { new() { ContactOfCode = "HOME", CountryCallingCode = customerDetail.Contacts.Countrycode, PhoneNum = customerDetail.Contacts.Phone, PhoneAreaNum = customerDetail.Contacts.PhoneAreaNumber } },
                    PassengerId = customerDetail.CustId,
                    PassengerTypeCode = customerDetail.Ptc,
                    AdultWithInfantPassengerId = customerDetail.LinkedInfantId,
                    LoyaltyMember = GetLoyaltyMember(customerDetail)
                });

            }

            return customers;
        }

        private static LoyaltyMember GetLoyaltyMember(EAPI.Domain.Virgin.Models.Customer.Customer customer)
        {
            return customer.LoyaltyMemberDetails?.MemberNumber.IsNullOrWhiteSpace() == false && customer.LoyaltyMemberDetails?.ProgramCode.IsNullOrWhiteSpace() == false ? new LoyaltyMember() { LoyaltyMemberOwnerCode = customer.LoyaltyMemberDetails?.ProgramCode, LoyaltyMemberId = customer.LoyaltyMemberDetails?.MemberNumber } : null;
        }

        private static IEnumerable<OrderItems> MapOrderItems(Proxy.DeltaOrder.Order dlOrderResponse, OrderCreateRequest orderCreateRequest)
        {
            return dlOrderResponse.OrderItems.Select(orderItem => new OrderItems
            {
                OrderItemId = orderItem.OrderItemNum,
                OwnerAirline = orderItem.OrderItemOwner.OwnerCode,
                PriceDetails = MapPriceDetails(orderItem),
                AccountableDocumentRefs = MapAccountableDocumentRef(orderItem, dlOrderResponse),
                PNRReferences = dlOrderResponse.BookingReference.First().RecordLocatorId,
                ItemStatus = new State { Status = orderItem.OrderItemStatusCode, TimeStamp = orderItem.OrderItemCreationUtcTs.ToString() },
                OfferIdReference = orderCreateRequest.SelectedOffers.First().OfferId,
                Services = MapServices(orderItem, dlOrderResponse),
                Errors = orderItem.Errors?.Select(e => new OrderItemError()
                {
                    Code = e.Code,
                    Message = e.Message
                }),
                FareComponents = MapFareComponents(orderItem, dlOrderResponse),
            }).ToList();
        }

        private static FareComponent MapFareComponents(OrderItem orderItem, Proxy.DeltaOrder.Order dlOrderResponse)
        {
            if (orderItem.ProductCategoryId == Constants.Application.DeltaProductCategoryIdFlights)
            {
                //select passengerid from orderitem
                var paxId = orderItem.RetailItems.FirstOrDefault()?.RetailItemMetaData.FlightInformation.PassengerIds.FirstOrDefault();
                //select passengertypecode for the passengerid
                var ptc = dlOrderResponse.OrderDataList.Customers.FirstOrDefault(c => c.PassengerId == paxId).PassengerTypeCode;
                //select ticketDesignatorCode list basis passengertypeCode
                return new FareComponent()
                {
                    TicketDesignatorCodes = dlOrderResponse.OrderDataList.FareByPassengerTypeCode.FirstOrDefault(e => e.PassengerTypeCode == ptc)?.TicketDesignatorCode
                };
            }

            return null;

        }

        private static IEnumerable<Price> MapPriceDetails(OrderItem orderItem)
        {
            if (orderItem.ProductCategoryId == Constants.Application.DeltaProductCategoryIdSeats)
            {
                var seatPrice = new List<Price>
                {
                    new()
                    {
                        Amount = new()
                        {
                            MonetaryAmount = orderItem.OrderItemPricing.TotalAmt.CurrencyEquivalentPrice.CurrencyAmt.ToString(),
                            BaseCurrency = orderItem.OrderItemPricing.TotalAmt.CurrencyEquivalentPrice.CurrencyCode,
                            DecimalPrecisionCount = orderItem.OrderItemPricing.TotalAmt.CurrencyEquivalentPrice.DecimalPrecisionCnt
                        }
                    }
                };

                return seatPrice;
            }

            var prices = new List<Price>
            {
                new()
                {
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount
                    {
                        BaseCurrency = orderItem.OrderItemPricing.TotalAmt?.CurrencyEquivalentPrice.CurrencyCode,
                        DecimalPrecisionCount = orderItem.OrderItemPricing.TotalAmt == null || orderItem.OrderItemPricing.TotalAmt?.CurrencyEquivalentPrice == null ? 0 : orderItem.OrderItemPricing.TotalAmt.CurrencyEquivalentPrice.DecimalPrecisionCnt,
                        MonetaryAmount = orderItem.OrderItemPricing.TotalAmt?.CurrencyEquivalentPrice.CurrencyAmt.ToString()
                    },
                    BaseCurrencyAmount = new EAPI.Domain.Virgin.Models.Common.BaseCurrencyAmount
                    {
                        Currency = orderItem.OrderItemPricing.TotalAmt?.CurrencyEquivalentPrice.CurrencyCode,
                        Amount = orderItem.OrderItemPricing.TotalAmt?.CurrencyEquivalentPrice.CurrencyAmt.ToString()
                    },
                    PriceComponentType = PriceComponentTypes.BasePriceEnum

                },
                new()
                {
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount
                    {
                        BaseCurrency = orderItem.OrderItemPricing.BaseAmt?.CurrencyEquivalentPrice.CurrencyCode,
                        DecimalPrecisionCount = orderItem.OrderItemPricing.BaseAmt == null || orderItem.OrderItemPricing.BaseAmt?.CurrencyEquivalentPrice == null ? 0: orderItem.OrderItemPricing.BaseAmt.CurrencyEquivalentPrice.DecimalPrecisionCnt,
                        MonetaryAmount = orderItem.OrderItemPricing.BaseAmt?.CurrencyEquivalentPrice.CurrencyAmt.ToString()
                    },
                    BaseCurrencyAmount = new EAPI.Domain.Virgin.Models.Common.BaseCurrencyAmount
                    {
                        Currency = orderItem.OrderItemPricing.BaseAmt?.CurrencyEquivalentPrice.CurrencyCode,
                        Amount = orderItem.OrderItemPricing.BaseAmt?.CurrencyEquivalentPrice.CurrencyAmt.ToString()
                    },
                    PriceComponentType = PriceComponentTypes.BaseFareEnum

                },
                 new()
                 {
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount
                    {
                        BaseCurrency = orderItem.OrderItemPricing.TaxSummary?.TotalTaxAmt?.CurrencyEquivalentPrice?.CurrencyCode,
                        DecimalPrecisionCount = orderItem.OrderItemPricing.TaxSummary?.TotalTaxAmt == null || orderItem.OrderItemPricing.TaxSummary?.TotalTaxAmt?.CurrencyEquivalentPrice == null ? 0 : orderItem.OrderItemPricing.TaxSummary.TotalTaxAmt.CurrencyEquivalentPrice.DecimalPrecisionCnt,
                        MonetaryAmount = orderItem.OrderItemPricing.TaxSummary?.TotalTaxAmt?.CurrencyEquivalentPrice?.CurrencyAmt.ToString()
                    },
                    BaseCurrencyAmount = new EAPI.Domain.Virgin.Models.Common.BaseCurrencyAmount
                    {
                        Currency = orderItem.OrderItemPricing.TaxSummary?.TotalTaxAmt?.CurrencyEquivalentPrice?.CurrencyCode,
                        Amount = orderItem.OrderItemPricing.TaxSummary?.TotalTaxAmt?.CurrencyEquivalentPrice?.CurrencyAmt.ToString()
                    },
                    PriceComponentType = PriceComponentTypes.TotalTaxEnum

                }
            };

            if (orderItem.OrderItemPricing.TaxSummary?.Taxes != null)
            {
                prices.AddRange(orderItem.OrderItemPricing.TaxSummary.Taxes.Select(tax => new Price
                {
                    Amount = new EAPI.Domain.Virgin.Models.Common.CurrencyAmount
                    {
                        MonetaryAmount = tax.TaxAmt.CurrencyEquivalentPrice.CurrencyAmt.ToString(),
                        BaseCurrency = tax.TaxAmt.CurrencyEquivalentPrice.CurrencyCode,
                        DecimalPrecisionCount = tax.TaxAmt.CurrencyEquivalentPrice.DecimalPrecisionCnt
                    },
                    PriceComponentType = PriceComponentTypes.TaxEnum
                }));
            }

            return prices;
        }

        private static IEnumerable<AccountableDocumentRef> MapAccountableDocumentRef(OrderItem orderItem, Proxy.DeltaOrder.Order dlOrderResponse)
        {
            var accountableDocumentRefs = new List<AccountableDocumentRef>();

            if (orderItem.RetailItems.First().RetailItemMetaData.FlightInformation != null)
            {
                foreach (var passenger in orderItem.RetailItems.First().RetailItemMetaData.FlightInformation.PassengerIds)
                {
                    var dlAccountableDocRef = dlOrderResponse.AccountableDocuments?.FirstOrDefault(aD => aD.Passenger?.PassengerId == passenger);
                    if (dlAccountableDocRef != null)
                    {
                        accountableDocumentRefs.Add(new AccountableDocumentRef
                        {
                            DocumentNumber = dlAccountableDocRef.AccountableDocumentNum,
                            IssueDate = dlAccountableDocRef.IssueLocalDate,
                            CustomerRefId = dlAccountableDocRef.Passenger.CustomerId,
                            FirstNameNumber = dlAccountableDocRef.Passenger.FirstNameNum,
                            LastNameNumber = dlAccountableDocRef.Passenger.LastNameNum,
                            DocumentType = dlAccountableDocRef.DocumentCategoryCode == Constants.Application.DocumentTypeCodeT ? AccountableDocumentRef.DocumentTypes.TEnum : AccountableDocumentRef.DocumentTypes.YEnum,
                        });
                    }
                }
            }

            return accountableDocumentRefs;
        }

        private static IEnumerable<OriginDestinationAssociation> MapOriginDestinationAssociations(Proxy.DeltaOrder.Order dlOrderResponse)
        {
            return dlOrderResponse.OrderDataList.Trips.Select(trip => new OriginDestinationAssociation
            {
                OdLabel = trip.TripId,
                OriginDestination = new OriginDestination
                {
                    OriginAirport = trip.OriginAirportCode,
                    DestinationAirport = trip.DestinationAirportCode
                }
            }).ToList();
        }

        private static IEnumerable<Segment> MapSegments(Proxy.DeltaOrder.Order dlOrderResponse)
        {
            var segments = new List<Segment>();
            int segmentCounter = 1;


            foreach (var segment in dlOrderResponse.OrderDataList.PassengerItinerarySegment)
            {
                var originDest = dlOrderResponse.OrderDataList.Trips.FirstOrDefault(pI => pI.FlightSegmentIdList.Any(id => id == segment.FlightSegmentId));

                segments.Add(new Segment
                {
                    SegmentId = segment.FlightSegmentId,
                    SegmentNumber = segment.FlightSegmentNum,
                    OriginAirportCode = segment.OriginAirportCode,
                    DestinationAirportCode = segment.DestinationAirportCode,
                    ScheduledArrivalTime = new UtcAndLocalDateTime { LocalDateTime = segment.ScheduledArrivalLocalTs.ToString(Constants.Application.ZuluDateTimeFormat) },
                    ScheduledDepartureTime = new UtcAndLocalDateTime { LocalDateTime = segment.ScheduledDepartureLocalTs.ToString(Constants.Application.ZuluDateTimeFormat) },
                    ClassOfService = segment.MarketingClassOfServiceCode,
                    Legs = new List<Legs>
                    {
                        new()
                        {
                            DestinationAirportCode = segment.FlightLeg.First().DestinationAirportCode,
                            OriginAirportCode = segment.FlightLeg.First().OriginAirportCode,
                            OperatingCarrier = segment.FlightLeg.First().OperatingCarrierCode,
                            ScheduledArrivalTime = new UtcAndLocalDateTime
                            {
                                LocalDateTime = segment.FlightLeg.First().ScheduledArrivalLocalTs.ToString(Constants.Application.ZuluDateTimeFormat)
                            },
                            ScheduledDepartureTime = new UtcAndLocalDateTime
                            {
                                LocalDateTime = segment.FlightLeg.First().ScheduledDepartureLocalTs.ToString(Constants.Application.ZuluDateTimeFormat)
                            },
                            LegSequenceNumber = segment.FlightLeg.First().LegId
                        }
                    },
                    MarketingFlightNumber = segment.MarketingFlightNum,
                    OperatingFlightNumber = segment.OperatingFlightNum,
                    MarketingCarrier = segment.MarketingCarrier.CarrierCode,
                    OperatingCarrier = segment.OperatingCarrier.CarrierCode,
                    OdRefId = originDest?.TripId,
                    SegmentSequence = segmentCounter++
                });
            }

            return segments;
        }

        private static IEnumerable<OrderService> MapServices(OrderItem orderItem, Proxy.DeltaOrder.Order dlOrderResponse)
        {
            if (orderItem.ProductCategoryId == Constants.Application.DeltaProductCategoryIdSeats)
            {
                var seatInfo = orderItem.RetailItems.First().RetailItemMetaData.SeatInformation;

                return new List<OrderService>()
                {
                    new()
                    {
                        CustomerRefIds = new List<string>() { GetPassengerIdIndex(seatInfo.PassengerId, dlOrderResponse.OrderDataList.Customers) },
                        SeatInformation = new() { FlightLegId = seatInfo.FlightLegId, SeatColumnName = seatInfo.SeatLocation.SeatColumnName, SeatProductCode = seatInfo.SeatLocation.SeatProductCode, SeatRowNum = seatInfo.SeatLocation.SeatRowNum.ToString(), EconomyComfort = seatInfo.SeatLocation.EconomyComfort, ExitOverride = seatInfo.SeatLocation.ExitOverride, ExitRow = seatInfo.SeatLocation.ExitRow, SeatStatus = seatInfo.SeatLocation.SeatStatus },
                        Product = new() { ProductType = EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum }
                    }
                };
            }

            var customerIds = orderItem.RetailItems.First().RetailItemMetaData.FlightInformation.PassengerIds.Select(id => dlOrderResponse.OrderDataList.Customers.First(cust => cust.PassengerId == id).CustomerId).ToList();
            var flightSegIds = orderItem.RetailItems.First().RetailItemMetaData.FlightInformation.FlightSegmentIds;
            var tripIds = dlOrderResponse.OrderDataList.Trips.Where(x => x.FlightSegmentIdList.SequenceEqual(flightSegIds)).Select(t => t.TripId);

            return new List<OrderService>() { new() { CustomerRefIds = customerIds, SegmentRefIds = flightSegIds, OriginDestinationRefIds = tripIds, Product = new() { ProductType = EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum } } };
        }

        private static string GetPassengerIdIndex(string passengerId, List<Customer> customers)
        {
            return customers.IndexOf(customers.FirstOrDefault(c => c.PassengerId == passengerId)).ToString();
        }

        private static IEnumerable<EAPI.Domain.Virgin.Models.Customer.Customer> MapCustomerDetails(Proxy.DeltaOrder.Order order)
        {
            return order.OrderDataList.Customers.Select((customer, i) =>
            {
                var document = customer.PassengerTypeCode != Constants.Application.InfantRequestPtc ? order.AccountableDocuments?.First(doc => doc.Passenger.CustomerId == customer.CustomerId) : null;

                return new EAPI.Domain.Virgin.Models.Customer.Customer
                {
                    CustId = i.ToString(),
                    Ptc = customer.PassengerTypeCode,
                    DateOfBirth = customer.BirthDate,
                    FirstName = customer.PersonName.FirstName,
                    LastName = customer.PersonName.LastName,
                    FirstNameNumber = document?.Passenger.FirstNameNum,
                    LastNameNumber = document?.Passenger.LastNameNum,
                    Contacts = new EAPI.Domain.Virgin.Models.Common.Contacts { Email = customer.EmailAdr, Phone = customer.Phones?.First().PhoneNum, Countrycode = customer.Phones?.First().CountryCallingCode, PhoneAreaNumber = customer.Phones?.First().PhoneAreaNum },
                    LoyaltyMemberDetails = new() { MemberNumber = customer?.LoyaltyMember?.LoyaltyMemberId, ProgramCode = customer?.LoyaltyMember?.LoyaltyMemberOwnerCode }
                };
            });
        }

        public static T MaskInfo<T>(T input)
        {
            if (input == null)
            {
                return default;
            }

            string maskedString = string.Empty;

            if (input is DlOrderRequest request)
            {
                MaskCustomerInfo(request.Customers, maskedString);

                MaskPaymentInfo(request.FormsOfPayment, maskedString);

                return input;
            }

            if (input is DlOrderResponse response)
            {
                MaskCustomerInfo(response.Orders.First().OrderDataList.Customers, maskedString);

                if (response.Orders.First().FormsOfPayment != null)
                {
                    MaskPaymentInfo(response.Orders.First().FormsOfPayment, maskedString);
                }

                return input;
            }

            return input;
        }

        public static void MaskCustomerInfo(IEnumerable<Customer> customers, string maskedString)
        {
            foreach (var c in customers)
            {
                var personName = c?.PersonName;
                c.EmailAdr = !string.IsNullOrEmpty(c?.EmailAdr) ? maskedString.PadLeft(c.EmailAdr.Length, 'X') : null;
                c.GenderCode = !string.IsNullOrEmpty(c?.GenderCode) ? maskedString.PadLeft(c.GenderCode.Length, 'X') : null;
                c.PersonName.FirstName = !string.IsNullOrEmpty(personName?.FirstName) ? maskedString.PadLeft(personName.FirstName.Length, 'X') : null;
                c.PersonName.LastName = !string.IsNullOrEmpty(personName?.LastName) ? maskedString.PadLeft(personName.LastName.Length, 'X') : null;
                c.BirthDate = !string.IsNullOrEmpty(c?.BirthDate) ? maskedString.PadLeft(c.BirthDate.Length, 'X') : null;
            }
        }

        public static void MaskPaymentInfo(IEnumerable<FormsOfPayment> formsOfPayment, string maskedString)
        {
            foreach (var p in formsOfPayment)
            {
                var cardHolderDetails = p.PaymentMethod?.PaymentCard?.CardHolderName;
                var paymentCard = p.PaymentMethod?.PaymentCard;

                if (cardHolderDetails != null && paymentCard != null)
                {
                    p.PaymentMethod.PaymentCard.CardHolderName.FirstName = maskedString.PadLeft(cardHolderDetails.FirstName.Length, 'X');
                    p.PaymentMethod.PaymentCard.CardHolderName.LastName = maskedString.PadLeft(cardHolderDetails.LastName.Length, 'X');
                    p.PaymentMethod.PaymentCard.PaymentCardNum = !string.IsNullOrEmpty(paymentCard?.PaymentCardNum) ? maskedString.PadLeft(paymentCard.PaymentCardNum.Length, 'X') : null;
                    p.PaymentMethod.PaymentCard.ExpirationYearNum = !string.IsNullOrEmpty(paymentCard?.ExpirationYearNum) ? maskedString.PadLeft(paymentCard.ExpirationYearNum.Length, 'X') : null;
                    p.PaymentMethod.PaymentCard.ExpirationMonthNum = !string.IsNullOrEmpty(paymentCard?.ExpirationMonthNum) ? maskedString.PadLeft(paymentCard.ExpirationMonthNum.Length, 'X') : null;
                }
            }
        }
    }
}