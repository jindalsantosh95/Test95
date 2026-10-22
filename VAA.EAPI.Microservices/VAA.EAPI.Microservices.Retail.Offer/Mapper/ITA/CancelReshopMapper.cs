using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Customer;
using VAA.EAPI.Infrastructure.PSS.Models;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel;
using VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel.Enums;
using static VAA.EAPI.Domain.Virgin.Models.Common.Price;
using OfferitemPricing = VAA.EAPI.Microservices.Retail.Offer.Domain.OrderCancel.OfferitemPricing;

namespace VAA.EAPI.Microservices.Retail.Offer.Mapper.Ita
{
    public class CancelReshopMapper
    {
        public static CancelReshopResponse RfcReshopResponse(Pnr pnrV6Response, TicketingV3RetrieveResponse ticketingV3RetrieveResponse)
        {
            if (ticketingV3RetrieveResponse is { TicketingV3RequestResponseType: { } })
            {
                return new CancelReshopResponse
                {
                    Offers = new List<ReshopOffer>
                    {
                       new()
                       {
                          OfferItems = ReshopOfferItemMapper(ticketingV3RetrieveResponse.TicketingV3RequestResponseType)
                       }
                    },
                    DataList = new()
                    {
                        CustomerDetails = CustomerDetailMapper(pnrV6Response, ticketingV3RetrieveResponse.TicketingV3RequestResponseType)
                    }
                };
            }

            return null;
        }

        private static List<ReshopOfferItem> ReshopOfferItemMapper(TicketingV3RequestResponseType[] ticketRequestResponse)
        {
            return ticketRequestResponse.Select(ticket => new ReshopOfferItem
            {
                OfferitemPricing = OfferitemPricingMapper(ticket),
                ServiceBeneficiaries = new List<string>
                {
                    ticket.TicketingV3ResponseType.First().AdditionalTicketLevelData.First().CustomerId
                }

            }).ToList();
        }

        private static List<OfferitemPricing> OfferitemPricingMapper(TicketingV3RequestResponseType ticket)
        {
            List<OfferitemPricing> reshopOfferItems = new();
            OfferitemPricing offeritem = new()
            {
                ReshoppingAction = ReshoppingActionType.Refund,
                OriginalSalePrice = new List<Price>
                {
                        new()
                        {
                            PriceComponentType = PriceComponentTypes.BaseFareEnum,
                            PriceComponentName = string.Empty,
                            Amount = new CurrencyAmount
                            {
                                MonetaryAmount = ticket.TicketingV3ResponseType.First().TicketInformation.BaseFare,
                                BaseCurrency = ticket.TicketingV3ResponseType.First().TicketInformation.CurrencyCodeOfBaseFare
                            }

                        },
                        new()
                        {
                            PriceComponentType = PriceComponentTypes.TotalTaxEnum,
                            PriceComponentName = string.Empty,
                            Amount = new CurrencyAmount
                            {
                                MonetaryAmount = ticket.TicketingV3ResponseType.First().TotalTax.TotalTaxAmount,
                                BaseCurrency = ticket.TicketingV3ResponseType.First().TotalTax.TotalTaxCurrencyCode
                            },
                            TaxBreakup = TaxMapper(ticket.TicketingV3ResponseType).ToList()
                        },
                        new()
                        {
                            PriceComponentType = PriceComponentTypes.BasePriceEnum,
                            PriceComponentName = string.Empty,
                            Amount = new CurrencyAmount
                            {
                                MonetaryAmount = ticket.TicketingV3ResponseType.First().TicketInformation.TotalFare,
                                BaseCurrency = ticket.TicketingV3ResponseType.First().TicketInformation.CurrencyCodeOfTotalFare
                            }
                        }
                    },
                SalePrice = new List<Price>
                {
                        new()
                        {
                            PriceComponentType = PriceComponentTypes.BaseFareEnum,
                            PriceComponentName = string.Empty,
                            Amount= new CurrencyAmount
                            {
                                MonetaryAmount = Constants.Application.RfcMonetaryAmount,
                                BaseCurrency = ticket.TicketingV3ResponseType.First().TicketInformation.CurrencyCodeOfBaseFare
                            }
                        },
                        new()
                        {
                            PriceComponentType = PriceComponentTypes.TotalTaxEnum,
                            PriceComponentName = string.Empty,
                            Amount=new CurrencyAmount
                            {
                                MonetaryAmount = Constants.Application.RfcMonetaryAmount,
                                BaseCurrency = ticket.TicketingV3ResponseType.First().TotalTax.TotalTaxCurrencyCode
                            },
                            TaxBreakup = null

                        },
                        new()
                        {
                            PriceComponentType = PriceComponentTypes.BasePriceEnum,
                            PriceComponentName = string.Empty,
                            Amount=new CurrencyAmount
                            {
                                MonetaryAmount = Constants.Application.RfcMonetaryAmount,
                                BaseCurrency = ticket.TicketingV3ResponseType.First().TicketInformation.CurrencyCodeOfTotalFare
                            },

                        }
                    },
            };
            offeritem.SalePriceDifference = new List<Price>
            {
                new()
                {
                    PriceComponentType = PriceComponentTypes.BaseFareEnum,
                    PriceComponentName = string.Empty,
                    Amount = new CurrencyAmount
                    {
                        MonetaryAmount = Convert.ToString(Convert.ToDecimal(offeritem.SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount) - Convert.ToDecimal(offeritem.OriginalSalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BaseFareEnum).Amount.MonetaryAmount)),
                        BaseCurrency = ticket.TicketingV3ResponseType.First().TicketInformation.CurrencyCodeOfBaseFare
                    }

                },
                new()
                {
                    PriceComponentType = PriceComponentTypes.TotalTaxEnum,
                    PriceComponentName = string.Empty,
                    Amount = new CurrencyAmount
                    {
                        MonetaryAmount = Convert.ToString(Convert.ToDecimal(offeritem.SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount) - Convert.ToDecimal(offeritem.OriginalSalePrice.First(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum).Amount.MonetaryAmount)),
                        BaseCurrency = ticket.TicketingV3ResponseType.First().TotalTax.TotalTaxCurrencyCode
                    },
                    TaxBreakup = SaleDifferenceTaxMapper(offeritem.OriginalSalePrice.First(x => x.PriceComponentType == PriceComponentTypes.TotalTaxEnum).TaxBreakup)
                },
                new()
                {
                    PriceComponentType = PriceComponentTypes.BasePriceEnum,
                    PriceComponentName = string.Empty,
                    Amount = new CurrencyAmount
                    {
                        MonetaryAmount = Convert.ToString(Convert.ToDecimal(offeritem.SalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount) - Convert.ToDecimal(offeritem.OriginalSalePrice.First(x => x.PriceComponentType == PriceComponentTypes.BasePriceEnum).Amount.MonetaryAmount)),
                        BaseCurrency = ticket.TicketingV3ResponseType.First().TicketInformation.CurrencyCodeOfTotalFare
                    }
                }
            };

            offeritem.PenaltyPrices = new List<PenaltyPrice>
            {
                new()
                {
                    Amount = new CurrencyAmount
                    {
                        MonetaryAmount = Constants.Application.RfcMonetaryAmount,
                        BaseCurrency = ticket.TicketingV3ResponseType.First().TicketInformation.CurrencyCodeOfBaseFare

                    }
                }
            };

            reshopOfferItems.Add(offeritem);
            return reshopOfferItems;
        }

        private static IEnumerable<Tax> TaxMapper(IEnumerable<TicketingV3ResponseType> ticketResponseType)
        {
            if (ticketResponseType == null)
            {
                return null;
            }

            return ticketResponseType.First().DTITaxItems.Select(taxItem => new Tax
            {
                Amount = new CurrencyAmount
                {
                    BaseCurrency = taxItem.TaxAmountCurrencyCode,
                    MonetaryAmount = taxItem.TaxAmount
                },
                Code = taxItem.TaxCode,
                Name = string.Empty,
                Refundable = true
            });

        }

        private static IEnumerable<Tax> SaleDifferenceTaxMapper(IEnumerable<Tax> salePriceTaxes)
        {
            return salePriceTaxes.Select(tax => new Tax
            {
                Code = tax.Code,
                Name = tax.Name,
                Amount = new CurrencyAmount
                {
                    MonetaryAmount = Convert.ToString(Convert.ToDecimal(Constants.Application.RfcMonetaryAmount) - Convert.ToDecimal(tax.Amount.MonetaryAmount)),
                    BaseCurrency = tax.Amount.BaseCurrency
                },
                Refundable = true
            });
        }

        private static IEnumerable<string> GetServiceBeneficiaries(TicketingV3RequestResponseType[] ticketRequestResponse)
        {
            return from ticket in ticketRequestResponse from response in ticket.TicketingV3ResponseType from item in response.AdditionalTicketLevelData select item.CustomerId;
        }

        private static List<Customer> CustomerDetailMapper(Pnr pnrV6Response, TicketingV3RequestResponseType[] ticketRequestResponse)
        {
            List<Customer> customers = new();

            foreach (var ticket in pnrV6Response.TicketInformation)
            {
                var ticketNumber = ticket.TicketNumber;
                var passenger = pnrV6Response.Passengers.FirstOrDefault(x => x.FirstNameNumber == ticket.FirstNameNumber && x.LastNameNumber == ticket.LastNameNumber);

                if (pnrV6Response.InfantInArms != null && passenger == null)
                {
                    passenger = pnrV6Response.InfantInArms.FirstOrDefault(x => x.FirstNameNumber == ticket.FirstNameNumber && x.LastNameNumber == ticket.LastNameNumber);
                }

                customers.Add(new Customer
                {
                    FirstName = passenger.FirstName,
                    LastName = passenger.LastName,
                    CustId = passenger.CustomerId,
                    Ptc = ticketRequestResponse.First(x => x.TicketingV3RequestType.ElectronicDocumentNumber == ticketNumber).TicketingV3ResponseType.First().AdditionalTicketLevelData.First().PassengerTypeCode
                });
            }

            return customers;
        }
    }
}
