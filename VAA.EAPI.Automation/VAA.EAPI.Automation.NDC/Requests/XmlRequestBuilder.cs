using VAA.EAPI.Automation.NDC.Requests.Models.AirShopping;
using VAA.EAPI.Automation.NDC.Requests.Models.OfferPrice;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderChange;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderChangeAmendAPIs;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderChangeCancel;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderChangeSeat;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderCreate;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderHistory;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderList;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderReshop;
using VAA.EAPI.Automation.NDC.Requests.Models.OrderRetrieve;
using VAA.EAPI.Automation.NDC.Requests.Models.SeatAvailability;
using VAA.EAPI.Automation.NDC.Requests.Models.ServiceList;

namespace VAA.EAPI.Automation.NDC.Requests
{
    public static class XmlRequestBuilder
    {
        public static IATA_OrderReshopRQ IATA_OrderReshopRequest()
        {
            IATA_OrderReshopRQ iATA_OrderReshopRQ = new IATA_OrderReshopRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    BookingRef = new(),
                    UpdateOrder = new()
                    {
                        ReshopOrder = new()
                        {
                            ReshopOrderChoice = new()
                            {
                                ServiceOrder = new()
                                {
                                    AddOfferItems = new()
                                    {
                                        FlightRelatedCriteria = new()
                                        {
                                            FlightCriteria = new()
                                            {
                                                FlightCharacteristicsCriteria = new()
                                                {
                                                    CharacteristicCode = "Direct",
                                                    PrefLevel = new()
                                                    {
                                                        PrefLevelCode = "Required"
                                                    }
                                                }
                                            }
                                        },

                                        FlightRequest = new()
                                        {
                                            FlightRequestOriginDestinationsCriteria = new()
                                            {
                                                OriginDestCriteria = new List<Models.OrderReshop.OriginDestCriteria>()
                                            }
                                        }
                                    },
                                    DeleteOrderItem = new List<DeleteOrderItem>()
                                },

                                UpdatePaxName = new()
                                {

                                }
                            }
                        },

                        CancelOrderRef = new()
                        {

                        }
                    }
                }
            };
            return iATA_OrderReshopRQ;
        }

        public static IATA_AirShoppingRQ IATA_AirShoppingRequest()
        {
            IATA_AirShoppingRQ iATA_AirShoppingRQ = new IATA_AirShoppingRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },

                Request = new()
                {
                    FlightRelatedCriteria = new()
                    {
                        FlightCriteria = new()
                        {
                            FlightCharacteristicsCriteria = new()
                            {
                                PrefLevel = new()

                            }
                        }
                    },
                    FlightRequest = new()
                    {
                        FlightRequestOriginDestinationsCriteria = new()
                        {
                            OriginDestCriteria = new List<Models.AirShopping.OriginDestCriteria>()
                        }
                    },
                    PaxList = new()
                    {
                        Pax = new List<Models.AirShopping.Pax>()
                    },

                    OfferCriteria = new()
                    {
                        FareCriteria = new()
                        {
                            PrefLevel = new()
                        },
                        ServiceCriteria = new(),
                        ProgramCriteria = new()
                        {
                            ProgramOwner = new()
                            {
                                Org = new()
                                {

                                }
                            }
                        }
                    },
                    ResponseParameters = new()
                    {
                        CurParameter = new()
                    }

                }
            };
            return iATA_AirShoppingRQ;
        }

        public static IATA_OfferPriceRQ IATA_OfferPriceRequest()
        {
            IATA_OfferPriceRQ iATA_OfferPriceRQ = new IATA_OfferPriceRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    DataLists = new()
                    {
                        PaxList = new()
                        {
                            Pax = new List<Models.OfferPrice.Pax>()
                        }
                    },
                    PricedOffer = new()
                    {
                        SelectedOfferList = new()
                        {
                            SelectedOffer = new()
                            {
                                SelectedOfferItem = new List<Models.OfferPrice.SelectedOfferItem>()
                            }
                        }
                    }
                }
            };
            return iATA_OfferPriceRQ;
        }

        public static IATA_SeatAvailabilityRQ IATA_SeatAvailabilityRequest()
        {
            IATA_SeatAvailabilityRQ iATA_SeatAvailabilityRQ = new IATA_SeatAvailabilityRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    SeatAvailCoreRequest = new()
                    {
                        OfferRequest = new()
                        {
                            Offer = new()
                            {

                            }
                        },

                        OrderRequest = new()
                        {
                            Order = new()
                            {

                            }
                        }
                    }
                }
            };
            return iATA_SeatAvailabilityRQ;
        }

        public static IATA_OrderCreateRQ IATA_OrderCreateRequest()
        {
            IATA_OrderCreateRQ iATA_OrderCreateRQ = new IATA_OrderCreateRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    CreateOrder = new()
                    {
                        AcceptSelectedQuotedOfferList = new()
                        {
                            SelectedPricedOffer = new()
                            {

                            }
                        }
                    },
                    DataLists = new()
                    {
                        ContactInfoList = new()
                        {
                            ContactInfo = new()
                            {

                            }
                        },
                        PaxList = new()
                        {
                            Pax = new()

                        }
                    },
                    PaymentFunctions = new()
                    {
                        OfferAssociation = new(),
                        PaymentMethodCriteria = new(),
                        PaymentProcessingDetails = new()
                        {
                            Amount = new(),
                            PaymentMethod = new()
                            {
                                IATA_EasyPay = new()
                                {

                                },
                                PaymentCard = new()
                                {
                                    SecurePaymentVersion2 = new(),
                                    SecureProgram = new(),
                                    CardholderAddress = new()
                                    {
                                        StreetText = new()
                                    }
                                },
                                SettlementPlan = new()
                                {

                                }
                            },
                            Payer = new()
                            {
                                PayerName = new()
                                {
                                    IndividualName = new()
                                },
                            }
                        }
                    }
                }
            };
            return iATA_OrderCreateRQ;
        }
        public static IATA_OrderChangeRQ IATA_OrderChangeRequest()
        {
            IATA_OrderChangeRQ iATA_OrderChangeRQ = new IATA_OrderChangeRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    BookingRef = new(),
                    ChangeOrderChoice = new()
                    {
                        AcceptSelectedQuotedOfferList = new()
                        {
                            SelectedPricedOffer = new()
                            {
                                SelectedOfferItem = new()
                            }
                        }
                    },
                    Order = new(),
                    DataLists = new()
                    {
                        ContactInfoList = new()
                        {
                            ContactInfo = new()
                            {
                                EmailAddress = new(),
                                Phone = new()
                            }
                        },
                        PaxList = new()
                        {
                            Pax = new()
                        }
                    },
                    PaymentFunctions = new()
                    {

                        PaymentMethodCriteria = new(),

                        PaymentProcessingDetails = new()
                        {
                            PaymentMethod = new()
                            {
                                IATA_EasyPay = new()
                                {

                                },
                                PaymentCard = new()
                                {
                                    SecurePaymentVersion2 = new(),
                                    SecureProgram = new(),

                                    CardholderAddress = new()
                                    {
                                        StreetText = new()
                                    }
                                },
                                OfflinePayment = new()
                                {

                                },
                                SettlementPlan = new()
                                {

                                }
                            },
                            Payer = new()
                            {
                                PayerName = new()
                                {
                                    IndividualName = new()
                                },
                            },
                            Amount = new()
                        }
                    }
                }
            };
            return iATA_OrderChangeRQ;
        }

        public static IATA_OrderChangeAmendAPIsRQ IATA_OrderChangeAmendAPIsRequest()
        {
            IATA_OrderChangeAmendAPIsRQ iATA_OrderChangeRQ = new IATA_OrderChangeAmendAPIsRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    BookingRef = new(),
                    ChangeOrderChoice = new()
                    {
                        UpdatePax = new()
                        {
                            Current = new()
                            {

                            },
                            New = new()
                            {

                            },
                        }
                    },

                    Order = new(),
                    DataLists = new()
                    {
                        PaxList = new()
                        {
                            Pax = new()
                        }
                    },
                }
            };
            return iATA_OrderChangeRQ;
        }

        public static IATA_OrderChangeCancelRQ IATA_OrderChangeCancelRequest()
        {
            IATA_OrderChangeCancelRQ iATA_OrderChangeRQ = new IATA_OrderChangeCancelRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    ChangeOrderChoice = new()
                    {
                        AcceptCancelledOffer = new()
                        {

                        }
                    },
                    Order = new(),
                }
            };
            return iATA_OrderChangeRQ;
        }

        public static IATA_OrderChangeSeatRQ IATA_OrderChangeSeatRequest()
        {
            IATA_OrderChangeSeatRQ iATA_OrderChangeSeatRQ = new IATA_OrderChangeSeatRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    BookingRef = new(),
                    ChangeOrderChoice = new()
                    {
                        AcceptSelectedQuotedOfferList = new()
                        {
                            SelectedPricedOffer = new()
                            {

                            }
                        }
                    },
                    Order = new(),
                    DataLists = new()
                    {
                        PaxList = new()
                        {
                            Pax = new()
                        }
                    },
                    PaymentFunctions = new()
                    {
                        PaymentMethodCriteria = new(),

                        PaymentProcessingDetails = new()
                        {
                            PaymentMethod = new()
                            {
                                IATA_EasyPay = new()
                                {

                                },
                                PaymentCard = new()
                                {
                                    SecurePaymentVersion2 = new(),
                                    SecureProgram = new(),

                                    CardholderAddress = new()
                                    {
                                        StreetText = new()
                                    }
                                },
                                OfflinePayment = new()
                                {

                                },
                                SettlementPlan = new()
                                {

                                }
                            },
                            Payer = new()
                            {
                                PayerName = new()
                                {
                                    IndividualName = new()
                                },
                            },
                            Amount = new()
                        }
                    }
                }
            };
            return iATA_OrderChangeSeatRQ;
        }

        public static Models.OrderQuote.IATA_OrderQuoteRQ IATA_OrderQuoteRequest()
        {
            Models.OrderQuote.IATA_OrderQuoteRQ iATA_OrderQuoteRQ = new Models.OrderQuote.IATA_OrderQuoteRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Payload = new()
                {
                    ExistingOrder = new(),
                    PaxList = new List<Models.OrderQuote.PaxList>(),
                    SelectedOffers = new()
                    {
                        SelectedOfferItem = new List<Models.OrderQuote.SelectedOfferItem>()
                    }
                }
            };
            return iATA_OrderQuoteRQ;
        }

        public static IATA_OrderRetrieveRQ IATA_OrderRetrieveRequest()
        {
            IATA_OrderRetrieveRQ iATA_OrderRetrieveRQ = new IATA_OrderRetrieveRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    OrderValidationFilterCriteria = new()
                    {
                        BookingRefFilterCriteria = new()
                        {
                            Carrier = new()
                        },
                        OrderFilterCriteria = new()
                        {

                        }
                    }
                }
            };
            return iATA_OrderRetrieveRQ;
        }

        public static IATA_ServiceListRQ IATA_ServiceListRequest()
        {
            IATA_ServiceListRQ iATA_ServiceListRQ = new IATA_ServiceListRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    ServiceListCoreRequest = new()
                    {
                        OfferRequest = new()
                        {
                            Offer = new()
                            {

                            }
                        },
                        OrderRequest = new()
                        {
                            Order = new()
                            {

                            }
                        }
                    }
                }
            };
            return iATA_ServiceListRQ;
        }

        public static Models.OrderQuoteHoldBooking.IATA_OrderQuoteRQ IATA_OrderQuoteHoldBookingRequest()
        {
            Models.OrderQuoteHoldBooking.IATA_OrderQuoteRQ iATA_OrderQuoteRQ = new Models.OrderQuoteHoldBooking.IATA_OrderQuoteRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Payload = new()
                {
                    RepricedOrder = new()
                    {

                    }
                }
            };
            return iATA_OrderQuoteRQ;
        }

        public static Models.OrderChangeService.IATA_OrderChangeRQ IATA_OrderChangeServiceRequest()
        {
            Models.OrderChangeService.IATA_OrderChangeRQ iATA_OrderChangeRQ = new Models.OrderChangeService.IATA_OrderChangeRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    BookingRef = new(),
                    ChangeOrderChoice = new()
                    {
                        AcceptSelectedQuotedOfferList = new()
                        {
                            SelectedPricedOffer = new()
                            {

                            }
                        }
                    },
                    Order = new(),
                    DataLists = new()
                    {
                        PaxList = new()
                        {
                            Pax = new()
                        }
                    },
                }
            };
            return iATA_OrderChangeRQ;
        }

        public static Models.OrderchangeSplitPnr.IATA_OrderChangeRQ IATA_OrderChangeSplitPnrRequest()
        {
            Models.OrderchangeSplitPnr.IATA_OrderChangeRQ iATA_OrderChangeRQ = new Models.OrderchangeSplitPnr.IATA_OrderChangeRQ
            {
                AugmentationPoint = new()
                {
                    ChangeOrderChoice = new()
                    {
                        UpdatePax = new List<Models.OrderchangeSplitPnr.UpdatePax>()
                    }
                },

                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    BookingRef = new(),
                    Order = new(),
                }
            };

            return iATA_OrderChangeRQ;
        }

        public static IATA_OrderListRQ IATA_OrderListRequest()
        {
            IATA_OrderListRQ iATA_OrderListRQ = new IATA_OrderListRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    OrderSearchFilterCriteria = new()
                    {
                        CreationDateTimeFilterCriteria = new CreationDateTimeFilterCriteria(),
                        LastModifiedDateTimeFilterCriteria = new LastModifiedDateTimeFilterCriteria(),
                        OrderCancellationDateFilterCriteria = new OrderCancellationDateFilterCriteria(),
                        PaymentFilterCriteria = new PaymentFilterCriteria(),
                        TravelDateFilterCriteria = new TravelDateFilterCriteria(),
                    },
                }
            };
            return iATA_OrderListRQ;
        }

        public static IATA_OrderHistoryRQ IATA_OrderHistoryRQ()
        {
            IATA_OrderHistoryRQ iATA_OrderHistoryRQ = new IATA_OrderHistoryRQ
            {
                DistributionChain = new()
                {
                    DistributionChainLink = new()
                    {

                    }
                },
                Request = new()
                {
                    Order = new()
                    {

                    }
                }

            };
            return iATA_OrderHistoryRQ;
        }

    }
}