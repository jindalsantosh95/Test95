using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Order.Infrastructure.Model;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OfferRS;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using static VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS.OrderChangeRequest;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS
{
    public static class OrderCancelRequestMapper
    {
        public static OrderCancelCommand MapCancelHoldBookingRequest(OrderDocument orderDocument, bool isCancelDisruption)
        {
            return new OrderCancelCommand
            {
                OrderChangeRequest = new()
                {
                    RLOC = orderDocument?.BookingReference?.RLOC,
                    Operation = Operations.OrderCancel,
                    CancelPnr = true,
                    Parties = orderDocument?.Parties,
                    DataList = GetDataList(orderDocument?.DataList),
                    IsCancelDisruption = isCancelDisruption,
                    IsCancelHoldBooking = true
                }
            };
        }
        private static Model.OrderMS.DataList GetDataList(VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.DataList dataList)
        {
            return new Model.OrderMS.DataList()
            {
                CustomerDetails = dataList?.CustomerDetails,
                SegmentDetails = dataList?.SegmentDetails
            };
        }

        public static OrderCancelCommand MapOrderCancelRequest(BookingReference request, bool cancelPnr, Operations operation, [Optional] CancelReshopResponse offerDocument, [Optional] Parties parties, [Optional] bool isCancelDisruption, [Optional] OrderDocument orderDoc)
        {
            if (offerDocument != null)
            {
                var cancelOfferItems = offerDocument.Offers.First().OfferItems
                                            .Select(item => new OfferItem
                                            {
                                                OfferItemId = item.OfferItemId,
                                                OfferitemPricing = item.OfferitemPricing,
                                                ServiceBeneficiaries = item.ServiceBeneficiaries,
                                                Services = item.Services
                                            });

                return new OrderCancelCommand
                {
                    OrderChangeRequest = new()
                    {
                        RLOC = request.RLOC,
                        IsRFC = offerDocument.IsRfc,
                        DataList = GetDataList(offerDocument.DataList, orderDoc),
                        SelectedOffers = GetSelectedOffer(offerDocument, orderDoc),
                        CancelPnr = cancelPnr,
                        OrderAmendments = [new()
                        {
                            OrderByReference = [new()
                            {
                                ItemsForDeletion = [new()
                                {
                                  CancelOfferItems = cancelOfferItems
                                }]
                            }]
                        }],
                        Operation = operation,
                        Parties = offerDocument.Parties,
                        IsCancelDisruption = isCancelDisruption,
                        AccountableDocumentRefs = orderDoc.OrderItems.First().AccountableDocumentRefs
                    }
                };
            }

            return new OrderCancelCommand
            {
                OrderChangeRequest = new()
                {
                    RLOC = request?.RLOC,
                    Operation = operation,
                    CancelPnr = cancelPnr,
                    Parties = parties
                }
            };
        }

        private static IEnumerable<Offers> GetSelectedOffer(Model.OfferRS.CancelReshopResponse offers, OrderDocument orderDoc)
        {
            foreach (var offerItem in offers.Offers.First().OfferItems)
            {
                var serviceBeneficiary = offerItem.Services.FirstOrDefault()?.ServiceBeneficiaries?.FirstOrDefault();

                var matchingOrderItemAccountableDocRef = orderDoc.OrderItems.FirstOrDefault(x => x.Services.Any(srv => srv.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum && serviceBeneficiary != null && srv.ServiceBeneficiaries.Contains(serviceBeneficiary)))?.AccountableDocumentRefs;
                if (matchingOrderItemAccountableDocRef?.Any() == true)
                {
                    offerItem.OfferitemPricing.First().FareCalculationText = matchingOrderItemAccountableDocRef.First().FareCalculation;
                }
            }

            return offers.Offers.Select(offer =>
                new Offers()
                {
                    OfferId = offer.OfferId,
                    OfferItems = offer.OfferItems.Select(x =>
                    {
                        return new EAPI.Domain.Virgin.Models.Offer.OfferItem()
                        {
                            OfferItemId = x.OfferItemId,
                            OfferitemPricing = x.OfferitemPricing,
                            Services = x.Services
                        };
                    })
                }
            );
        }

        private static Model.OrderMS.DataList GetDataList(Model.OfferRS.DataList dataList, OrderDocument orderDoc)
        {
            return new Model.OrderMS.DataList()
            {
                CustomerDetails = dataList?.CustomerDetails,
                Penalty = dataList?.Penalty,
                SegmentDetails = orderDoc?.DataList?.SegmentDetails
            };
        }

    }
}

