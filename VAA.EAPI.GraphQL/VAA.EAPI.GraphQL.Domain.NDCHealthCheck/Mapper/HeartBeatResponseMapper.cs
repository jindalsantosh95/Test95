using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Models;

namespace VAA.EAPI.GraphQL.Domain.NDCHealthCheck.Mapper
{
    public static class HeartBeatResponseMapper
    {
        public static IEnumerable<HealthCheckResponse> MapResponse(List<string> apis, HeartBeatResponse offerHeartBeatResponse, HeartBeatResponse orderHeartBeatResponse, HealthReportResponse healthReportResponse, HeartBeatResponse paymentFacadeHeartBeatResponse)
        {
            var response = new List<HealthCheckResponse>();

            var validationStatus = GetStatus(offerHeartBeatResponse.Entries, "ValidationMSHealthCheck");
            var offerStatus = GetStatus(offerHeartBeatResponse.Entries, "OfferMSHealthCheck");
            var configStatus = GetStatus(offerHeartBeatResponse.Entries, "ConfigMSHealthCheck");
            var brandStatus = GetStatus(offerHeartBeatResponse.Entries, "BrandMSHealthCheck");
            var offerOrderDataStatus = GetStatus(offerHeartBeatResponse.Entries, "OrderDataMSHealthCheck");
            var offerOrderSearchStatus = GetStatus(offerHeartBeatResponse.Entries, "OrderSearchMSHealthCheck");
            var seatStatus = GetStatus(offerHeartBeatResponse.Entries, "SeatMSHealthCheck");
            var air4OrderSearchStatus = GetStatus(offerHeartBeatResponse.Entries, "Air4OrderSearchMSHealthCheck");
            var flightServicesStatus = GetStatus(offerHeartBeatResponse.Entries, "FlightServicesMSHealthCheck");

            var orderValidationStatus = GetStatus(orderHeartBeatResponse.Entries, "ValidationMSHealthCheck");
            var orderOfferStatus = GetStatus(orderHeartBeatResponse.Entries, "OfferMSHealthCheck");
            var orderConfigStatus = GetStatus(orderHeartBeatResponse.Entries, "ConfigMSHealthCheck");
            var orderBrandStatus = GetStatus(orderHeartBeatResponse.Entries, "BrandMSHealthCheck");
            var orderDataStatus = GetStatus(orderHeartBeatResponse.Entries, "OrderDataMSHealthCheck");
            var orderSearchStatus = GetStatus(orderHeartBeatResponse.Entries, "OrderSearchMSHealthCheck");
            var orderSeatStatus = GetStatus(orderHeartBeatResponse.Entries, "SeatMSHealthCheck");
            var orderAir4OrderSearchStatus = GetStatus(orderHeartBeatResponse.Entries, "Air4OrderSearchMSHealthCheck");
            var orderFlightServicesStatus = GetStatus(orderHeartBeatResponse.Entries, "FlightServicesMSHealthCheck");
            var referenceStatus = GetStatus(orderHeartBeatResponse.Entries, "ReferenceMSHealthCheck");
            var orderStatus = GetStatus(orderHeartBeatResponse.Entries, "OrderMSHealthCheck");
            var orderTicketStatus = GetStatus(orderHeartBeatResponse.Entries, "OrderTicketMSHealthCheck");
            var orderMgmtStatus = GetStatus(orderHeartBeatResponse.Entries, "OrderMgmtMSHealthCheck");
            var air4OrderMgmtStatus = GetStatus(orderHeartBeatResponse.Entries, "Air4OrderMgmtMSHealthCheck");
            var paymentStatus = GetStatus(orderHeartBeatResponse.Entries, "PaymentMSHealthCheck");
            var notificationStatus = GetStatus(orderHeartBeatResponse.Entries, "OrderNotificationMSHealthCheck");
            var ifgPaymentStatus = GetStatus(orderHeartBeatResponse.Entries, "IFGPaymentMSHealthCheck");
            var arcPaymentStatus = GetStatus(orderHeartBeatResponse.Entries, "ARCPaymentMSHealthCheck");

            var lowThreshold = Convert.ToDecimal(ConfigReader.Get(Constants.Environment.LowThreshold));
            var normalthreshold = Convert.ToDecimal(ConfigReader.Get(Constants.Environment.Threshold));
            var highThreshold = Convert.ToDecimal(ConfigReader.Get(Constants.Environment.HighThreshold));

            foreach (var api in apis)
            {
                //Offer
                if (api.Is(OfferAPIs.AirShopping.ToString()))
                {
                    var airshoppingStatus = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green };

                    if (validationStatus?.Status != "2" || offerStatus?.Status != "2" || configStatus?.Status != "2")
                        airshoppingStatus.Status = RAGStatus.Red;

                    else
                    {
                        var airshoppingRealTimeReport = healthReportResponse.HealthReport.Where(x => x.API.Is(api));
                        var airshoppingCalenderSearch = healthReportResponse.HealthReport.Where(x => x.API.Is("AirShoppingCalendarSearch"));

                        var failures = airshoppingRealTimeReport.FirstOrDefault(x => x.StatusInfo.Any(x => x.StatusCode == "500"))?.StatusInfo?.FirstOrDefault(x => x.StatusCode == "500");
                        var calenderSearchFailures = airshoppingCalenderSearch.FirstOrDefault(x => x.StatusInfo.Any(x => x.StatusCode == "500"))?.StatusInfo?.FirstOrDefault(x => x.StatusCode == "500");
                        if (failures != null && failures.Percentage > normalthreshold)
                            airshoppingStatus.Status = RAGStatus.Red;
                        else
                        {
                            if (calenderSearchFailures != null && calenderSearchFailures.Percentage > normalthreshold)
                            {
                                airshoppingStatus.Status = RAGStatus.Amber;
                                airshoppingStatus.Remarks = [new() { ImpactedFunctionality = SubCategories.CalendarSearch, Status = RAGStatus.Red }];
                            }

                            if (failures != null && failures.Percentage > lowThreshold && failures.Percentage <= normalthreshold)
                            {
                                airshoppingStatus.Status = RAGStatus.Amber;
                                if (calenderSearchFailures != null && calenderSearchFailures.Percentage > lowThreshold && calenderSearchFailures.Percentage <= normalthreshold)
                                {
                                    airshoppingStatus.Remarks = [new() { ImpactedFunctionality = SubCategories.CalendarSearch, Status = RAGStatus.Amber }];
                                }
                            }
                        }
                    }

                    response.Add(airshoppingStatus);
                }
                else if (api.Is(OfferAPIs.OfferPrice.ToString()))
                {
                    if (validationStatus?.Status != "2" || offerStatus?.Status != "2" || configStatus?.Status != "2" || brandStatus?.Status != "2")
                        response.Add(new() { Api = api, Status = RAGStatus.Red });
                    else
                    {
                        var failures = healthReportResponse.HealthReport.FirstOrDefault(x => x.API.Is(api)).StatusInfo.FirstOrDefault(s => s.StatusCode == "500");

                        if (failures != null && failures.Percentage > normalthreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Red });
                        else if (failures != null && failures.Percentage > lowThreshold && failures.Percentage <= normalthreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Amber });
                        else
                            response.Add(new() { Api = api, Status = RAGStatus.Green });
                    }
                }
                else if (api.Is(OfferAPIs.OrderQuote.ToString()))
                {
                    if (validationStatus?.Status != "2" || configStatus?.Status != "2" || brandStatus?.Status != "2" || offerOrderSearchStatus?.Status != "2")
                        response.Add(new() { Api = api, Status = RAGStatus.Red });
                    else
                    {
                        var failures = healthReportResponse.HealthReport.First(x => x.API.Contains(api)).StatusInfo.FirstOrDefault(x => x.StatusCode == "500");

                        if (failures != null && failures.Percentage > highThreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Red });
                        else if (failures != null && failures.Percentage > normalthreshold && failures.Percentage <= highThreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Amber });
                        else
                            response.Add(new() { Api = api, Status = RAGStatus.Green });
                    }
                }
                else if (api.Is(OfferAPIs.OrderReshop.ToString()))
                {
                    var reshopStatus = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green };

                    if (validationStatus?.Status != "2" || configStatus?.Status != "2" || offerStatus?.Status != "2" || offerOrderDataStatus?.Status != "2" || brandStatus?.Status != "2" || offerOrderSearchStatus?.Status != "2")
                        reshopStatus.Status = RAGStatus.Red;
                    else
                    {
                        var reshopRealTimeReport = healthReportResponse.HealthReport.Where(x => x.API.Is(api));
                        var reshopCancelReport = healthReportResponse.HealthReport.Where(x => x.API.Is("OrderReshopCancel"));

                        var failures = reshopRealTimeReport.FirstOrDefault(x => x.StatusInfo.Any(x => x.StatusCode == "500"))?.StatusInfo.FirstOrDefault(x => x.StatusCode == "500");
                        var reshopCancelFailures = reshopCancelReport.FirstOrDefault(x => x.StatusInfo.Any(x => x.StatusCode == "500"))?.StatusInfo.FirstOrDefault(x => x.StatusCode == "500");
                        if (failures != null && failures.Percentage > highThreshold)
                            reshopStatus.Status = RAGStatus.Red;
                        else
                        {
                            if (reshopCancelFailures != null && reshopCancelFailures.Percentage > highThreshold)
                            {
                                reshopStatus.Status = RAGStatus.Amber;
                                reshopStatus.Remarks = [new() { ImpactedFunctionality = SubCategories.ReshopCancel, Status = RAGStatus.Red }];
                            }

                            if (failures != null && failures.Percentage > normalthreshold && failures.Percentage <= highThreshold)
                            {
                                reshopStatus.Status = RAGStatus.Amber;
                                if (reshopCancelFailures != null && reshopCancelFailures.Percentage > normalthreshold && reshopCancelFailures.Percentage <= highThreshold)
                                {
                                    reshopStatus.Remarks = [new() { ImpactedFunctionality = SubCategories.ReshopCancel, Status = RAGStatus.Amber }];
                                }
                            }
                        }
                    }

                    response.Add(reshopStatus);
                }
                else if (api.Is(OfferAPIs.SeatAvailability.ToString()))
                {
                    var postSaleResponse = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green };
                    var primeSaleResponse = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green };
                    var seatHealthReport = healthReportResponse.HealthReport.Where(x => x.API.Contains(api));

                    //Prime sale
                    if (validationStatus?.Status != "2" || configStatus?.Status != "2" || offerStatus?.Status != "2" || seatStatus?.Status != "2")
                        primeSaleResponse.Status = RAGStatus.Red;
                    else
                    {
                        var primeSaleReport = seatHealthReport?.FirstOrDefault(x => x.API.Contains("PrimeSale"));

                        if (primeSaleReport != null && primeSaleReport.StatusInfo.Any(x => x.StatusCode == "500" && x.Percentage > normalthreshold))
                            primeSaleResponse.Status = RAGStatus.Red;

                        else if (primeSaleReport != null && primeSaleReport.StatusInfo.Any(x => x.StatusCode == "500" && x.Percentage > lowThreshold && x.Percentage <= normalthreshold))
                            primeSaleResponse.Status = RAGStatus.Amber;
                    }

                    //Post sale
                    if (validationStatus?.Status != "2" || configStatus?.Status != "2" || offerOrderDataStatus?.Status != "2" || brandStatus?.Status != "2" || offerOrderSearchStatus?.Status != "2" || seatStatus?.Status != "2")
                        postSaleResponse.Status = RAGStatus.Red;

                    else
                    {
                        var postSaleReport = seatHealthReport?.FirstOrDefault(x => x.API.Contains("PostSale"));

                        if (postSaleReport != null && postSaleReport.StatusInfo.Any(x => x.StatusCode == "500" && x.Percentage > normalthreshold))
                            postSaleResponse.Status = RAGStatus.Red;

                        else if (postSaleReport != null && postSaleReport.StatusInfo.Any(x => x.StatusCode == "500" && x.Percentage > lowThreshold && x.Percentage <= normalthreshold))
                            postSaleResponse.Status = RAGStatus.Amber;

                        else if (air4OrderSearchStatus?.Status != "2")
                        {
                            postSaleResponse.Status = RAGStatus.Amber;
                            postSaleResponse.Remarks = [new() { ImpactedFunctionality = SubCategories.GDS, Status = RAGStatus.Red }];
                        }
                    }

                    if (primeSaleResponse.Status == RAGStatus.Red && postSaleResponse.Status == RAGStatus.Red)
                        response.Add(new() { Api = api, Status = RAGStatus.Red });
                    else if (primeSaleResponse.Status == RAGStatus.Green && postSaleResponse.Status == RAGStatus.Green)
                        response.Add(new() { Api = api, Status = RAGStatus.Green });
                    else
                    {
                        var seatResponse = new HealthCheckResponse() { Api = api, Status = RAGStatus.Amber, Remarks = [new() { ImpactedFunctionality = SubCategories.PrimeSale, Status = primeSaleResponse.Status }, new() { ImpactedFunctionality = SubCategories.PostSale, Status = postSaleResponse.Status }] };

                        if (postSaleResponse.Remarks != null) seatResponse.Remarks = seatResponse.Remarks.Append(postSaleResponse.Remarks.First());
                        response.Add(seatResponse);
                    }
                }
                else if (api.Is(OfferAPIs.ServiceList.ToString()))
                {
                    var primeServiceResponse = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green }; 
                    var postServiceResponse = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green };

                    var serviceListHealthReport = healthReportResponse.HealthReport.Where(x => x.API.Contains(api));

                    //PrimeSale
                    if (validationStatus?.Status != "2" || offerStatus?.Status != "2" || flightServicesStatus?.Status != "2")
                        primeServiceResponse.Status = RAGStatus.Red;
                    else
                    {
                        var primeServiceReport = serviceListHealthReport?.FirstOrDefault(x => x.API.Contains("PrimeSale"));

                        if (primeServiceReport != null && primeServiceReport.StatusInfo.Any(x => x.StatusCode == "500" && x.Percentage > normalthreshold))
                            primeServiceResponse.Status = RAGStatus.Red;

                        else if (primeServiceReport != null && primeServiceReport.StatusInfo.Any(x => x.StatusCode == "500" && x.Percentage > lowThreshold && x.Percentage <= normalthreshold))
                            primeServiceResponse.Status = RAGStatus.Amber;
                    }

                    //PostSale
                    if (validationStatus?.Status != "2" || offerOrderDataStatus?.Status != "2" || brandStatus?.Status != "2" || offerOrderSearchStatus?.Status != "2" || flightServicesStatus?.Status != "2")
                        postServiceResponse.Status = RAGStatus.Red;

                    else
                    {
                        var postServiceReport = serviceListHealthReport?.FirstOrDefault(x => x.API.Contains("PostSale"));

                        if (postServiceReport != null && postServiceReport.StatusInfo.Any(x => x.StatusCode == "500" && x.Percentage > normalthreshold))
                            postServiceResponse.Status = RAGStatus.Red;

                        else if (postServiceReport != null && postServiceReport.StatusInfo.Any(x => x.StatusCode == "500" && x.Percentage > lowThreshold && x.Percentage <= normalthreshold))
                            postServiceResponse.Status = RAGStatus.Amber;

                    }

                    if (primeServiceResponse.Status == RAGStatus.Red && postServiceResponse.Status == RAGStatus.Red)
                        response.Add(new() { Api = api, Status = RAGStatus.Red });
                    else if (primeServiceResponse.Status == RAGStatus.Green && postServiceResponse.Status == RAGStatus.Green)
                        response.Add(new() { Api = api, Status = RAGStatus.Green });
                    else
                    {
                        var serviceListResponse = new HealthCheckResponse() { Api = api, Status = RAGStatus.Amber, Remarks = [new() { ImpactedFunctionality = SubCategories.PrimeSale, Status = primeServiceResponse.Status }, new() { ImpactedFunctionality = SubCategories.PostSale, Status = postServiceResponse.Status }] };

                        if (postServiceResponse.Remarks != null) serviceListResponse.Remarks = serviceListResponse.Remarks.Append(postServiceResponse.Remarks.First());
                        response.Add(serviceListResponse);
                    }
                }

                //Order
                else if (api.Is(OrderAPIs.OrderCreate.ToString()))
                {
                    var remarks = new List<Remarks>();
                    var orderCreateStatus = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green };

                    if (orderValidationStatus?.Status != "2" || orderOfferStatus?.Status != "2" || orderStatus?.Status != "2")
                        orderCreateStatus.Status = RAGStatus.Red;
                    else
                    {
                        var orderCreateRealTimeReport = healthReportResponse.HealthReport.Where(x => x.API.Contains(api));
                        var holdBooking = orderCreateRealTimeReport.FirstOrDefault(x => x.API.Contains("HoldBooking"));
                        var instantPurchase = orderCreateRealTimeReport.FirstOrDefault(x => x.API.Contains("InstantPurchase"));
                        var seat = orderCreateRealTimeReport.FirstOrDefault(x => x.API.Contains("Seat"));

                        if (instantPurchase != null && instantPurchase.StatusInfo.Any(s => s.StatusCode == "500" && s.Percentage > highThreshold))
                            orderCreateStatus.Status = RAGStatus.Red;
                        if (holdBooking != null && holdBooking.StatusInfo.Any(s => s.StatusCode == "500" && s.Percentage > highThreshold))
                            orderCreateStatus.Status = RAGStatus.Red;
                        else
                        {
                            if (holdBooking != null && holdBooking.StatusInfo.Any(s => s.StatusCode == "500" && s.Percentage > normalthreshold && s.Percentage <= highThreshold))
                            {
                                orderCreateStatus.Status = RAGStatus.Amber;
                                remarks.Add(new() { ImpactedFunctionality = SubCategories.HoldBooking, Status = RAGStatus.Amber });
                            }

                            if (((referenceStatus?.Status != "2" || paymentStatus?.Status != "2" || orderTicketStatus?.Status != "2")) || ifgPaymentStatus?.Status != "2" || arcPaymentStatus?.Status != "2"  || paymentFacadeHeartBeatResponse?.Status != "2"|| (instantPurchase != null && instantPurchase.StatusInfo.Any(s => s.StatusCode == "500" && s.Percentage > normalthreshold && s.Percentage <= highThreshold)))
                            {
                                orderCreateStatus.Status = RAGStatus.Amber;
                                remarks.Add(new() { ImpactedFunctionality = SubCategories.InstantPurchase, Status = RAGStatus.Amber });
                            }

                            if (orderFlightServicesStatus?.Status != "2" && orderCreateStatus.Status != RAGStatus.Red)
                            {
                                orderCreateStatus.Status = RAGStatus.Amber;
                                remarks.Add(new() { ImpactedFunctionality = SubCategories.Service, Status = RAGStatus.Amber });
                            }

                            if (((orderSeatStatus?.Status != "2") && orderCreateStatus.Status != RAGStatus.Red) || (seat != null && (seat.StatusInfo.Any(s => s.StatusCode == "500" && s.Percentage > highThreshold) || seat.StatusInfo.Any(s => s.StatusCode == "500" && s.Percentage > normalthreshold && s.Percentage <= highThreshold))))
                            {
                                orderCreateStatus.Status = RAGStatus.Amber;
                                var remark = new Remarks() { ImpactedFunctionality = SubCategories.Seat };

                                if (orderSeatStatus?.Status != "2" || (seat != null && seat.StatusInfo.Any(s => s.StatusCode == "500" && s.Percentage > highThreshold)))
                                    remark.Status = RAGStatus.Red;
                                else
                                    remark.Status = RAGStatus.Amber;

                                remarks.Add(remark);
                           
                            }
                        }
                    }

                    orderCreateStatus.Remarks = remarks.Count > 0 ? remarks : null;
                    response.Add(orderCreateStatus);
                }
                else if (api.Is(OrderAPIs.OrderRetrieve.ToString()))
                {
                    var remarks = new List<Remarks>();
                    var orderRetrieveStatus = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green };

                    if (orderValidationStatus?.Status != "2" || orderDataStatus?.Status != "2" || orderSearchStatus?.Status != "2" || orderBrandStatus?.Status != "2")
                        orderRetrieveStatus.Status = RAGStatus.Red;

                    else if (orderAir4OrderSearchStatus?.Status != "2")
                    {
                        orderRetrieveStatus.Status = RAGStatus.Amber;
                        remarks.Add(new() { ImpactedFunctionality = SubCategories.GDS, Status = RAGStatus.Amber });
                    }

                    orderRetrieveStatus.Remarks = remarks.Count > 0 ? remarks : null;
                    response.Add(orderRetrieveStatus);
                }
                else if (api.Is(OrderAPIs.OrderChange.ToString()))
                {
                    var remarks = new List<Remarks>();
                    var orderChangeStatus = new HealthCheckResponse() { Api = api, Status = RAGStatus.Green };

                    // Pre-checks for dependent APIs
                    if (orderValidationStatus?.Status != "2" || orderDataStatus?.Status != "2" ||
                        orderSearchStatus?.Status != "2" || referenceStatus?.Status != "2" ||
                        orderMgmtStatus?.Status != "2" || orderOfferStatus?.Status != "2")
                    {
                        orderChangeStatus.Status = RAGStatus.Red;
                    }

                    else
                    {
                        if (flightServicesStatus?.Status != "2" && orderChangeStatus.Status != RAGStatus.Red)
                        {
                            orderChangeStatus.Status = RAGStatus.Amber;
                            remarks.Add(new() { ImpactedFunctionality = SubCategories.Service, Status = RAGStatus.Amber });
                        }

                        if ((air4OrderMgmtStatus?.Status != "2" || seatStatus?.Status != "2") && orderChangeStatus.Status != RAGStatus.Red)
                        {
                            orderChangeStatus.Status = RAGStatus.Amber;
                            remarks.Add(new() { ImpactedFunctionality = SubCategories.PostSaleSeat, Status = RAGStatus.Red });
                        }

                        if (orderTicketStatus?.Status != "2" && orderChangeStatus.Status != RAGStatus.Red)
                        {
                            orderChangeStatus.Status = RAGStatus.Amber;
                            remarks.Add(new() { ImpactedFunctionality = SubCategories.ConfirmHoldBooking, Status = RAGStatus.Amber });
                        }

                        var orderChangeRealTimeReport = healthReportResponse.HealthReport.Where(x => x.API.Contains(api));

                        // Main flows drive overall status
                        var mainFlows = new Dictionary<SubCategories, dynamic>
                        {
                            { SubCategories.AddCol, orderChangeRealTimeReport.FirstOrDefault(x => x.API.Contains("OrderChange_AddCollect")) },
                            { SubCategories.Cancel, orderChangeRealTimeReport.FirstOrDefault(x => x.API.Contains("OrderChange_Cancel")) },
                            { SubCategories.EvenExchange, orderChangeRealTimeReport.FirstOrDefault(x => x.API.Contains("OrderChange_EvenExchange")) },
                            { SubCategories.Refund, orderChangeRealTimeReport.FirstOrDefault(x => x.API.Contains("OrderChange_Refund")) }
                        };

                        // Other flows only add remarks
                        var otherFlows = new Dictionary<SubCategories, dynamic>
                        {
                            { SubCategories.SplitPNR, orderChangeRealTimeReport.FirstOrDefault(x => x.API.Contains("OrderChange_SplitPNR")) },
                            { SubCategories.PostSaleSeat, orderChangeRealTimeReport.FirstOrDefault(x => x.API.Contains("OrderChange_PostSaleSeat")) },
                            { SubCategories.ConfirmHoldBooking, orderChangeRealTimeReport.FirstOrDefault(x => x.API.Contains("OrderChange_HoldBooking")) },
                            { SubCategories.AmendApis, orderChangeRealTimeReport.FirstOrDefault(x => x.API.Contains("OrderChange_AmendApis")) }
                        };

                        int redCountMain = 0;
                        int amberCountMain = 0;

                        // Evaluate main flows (drive overall status)
                        foreach (var kvp in mainFlows)
                        {
                            var category = kvp.Key;
                            var flow = kvp.Value;
                            var status = RAGStatus.Green; // default

                            if (flow?.StatusInfo != null)
                            {
                                foreach (var s in flow.StatusInfo)
                                {
                                    if (s.StatusCode == "500" && s.Percentage > highThreshold || (paymentStatus?.Status != "2" || arcPaymentStatus?.Status != "2" || ifgPaymentStatus?.Status != "2" || paymentFacadeHeartBeatResponse?.Status != "2"))
                                    {
                                        status = RAGStatus.Red;
                                        redCountMain++;
                                        break;
                                    }
                                    else if (s.StatusCode == "500" && s.Percentage > normalthreshold)
                                    {
                                        status = RAGStatus.Amber;
                                        amberCountMain++;
                                        break;
                                    }
                                }
                            }

                            remarks.Add(new() { ImpactedFunctionality = category, Status = status });
                        }

                        // Evaluate other flows (only add remarks, don’t drive overall status)
                        foreach (var kvp in otherFlows)
                        {
                            var category = kvp.Key;
                            var flow = kvp.Value;
                            var status = RAGStatus.Green; // default

                            if (flow?.StatusInfo != null)
                            {
                                foreach (var s in flow.StatusInfo)
                                {
                                    if (s.StatusCode == "500" && s.Percentage > highThreshold && ((category == SubCategories.ConfirmHoldBooking || category == SubCategories.PostSaleSeat) && (paymentStatus?.Status != "2" || arcPaymentStatus?.Status != "2" || ifgPaymentStatus?.Status != "2" || paymentFacadeHeartBeatResponse?.Status != "2")))
                                    {
                                        status = RAGStatus.Red;
                                        break;
                                    }
                                    else if (s.StatusCode == "500" && s.Percentage > normalthreshold)
                                    {
                                        status = RAGStatus.Amber;
                                        break;
                                    }
                                }
                            }

                            remarks.Add(new() { ImpactedFunctionality = category, Status = status });
                        }

                        // Decide overall status based only on main flows
                        if (redCountMain >= 2)
                            orderChangeStatus.Status = RAGStatus.Red;
                        else if (redCountMain == 1 || amberCountMain > 0)
                            orderChangeStatus.Status = RAGStatus.Amber;
                        else
                            orderChangeStatus.Status = RAGStatus.Green;
                    }

                    orderChangeStatus.Remarks = remarks;
                    response.Add(orderChangeStatus);
                }

                else if (api.Is(OrderAPIs.OrderChangeNotif.ToString()))
                {
                    if (orderValidationStatus?.Status != "2" || orderDataStatus?.Status != "2" || orderSearchStatus?.Status != "2" || notificationStatus?.Status != "2")
                        response.Add(new() { Api = api, Status = RAGStatus.Red });

                    else
                    {
                        var failures = healthReportResponse.HealthReport.First(x => x.API.Is(api)).StatusInfo.FirstOrDefault(s => s.StatusCode == "500");

                        if (failures != null && failures.Percentage > highThreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Red });
                        else if (failures != null && failures.Percentage > normalthreshold && failures.Percentage <= highThreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Amber });
                        else
                            response.Add(new() { Api = api, Status = RAGStatus.Green });
                    }
                }
                else if (api.Is(OrderAPIs.OrderList.ToString()))
                {
                    if (orderValidationStatus?.Status != "2" || orderDataStatus?.Status != "2")
                        response.Add(new() { Api = api, Status = RAGStatus.Red });

                    else
                    {
                        var failures = healthReportResponse.HealthReport.First(x => x.API.Contains(api)).StatusInfo.FirstOrDefault(x => x.StatusCode == "500");

                        if (failures != null && failures.Percentage > highThreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Red });
                        else if (failures != null && failures.Percentage > normalthreshold && failures.Percentage <= highThreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Amber });
                        else
                            response.Add(new() { Api = api, Status = RAGStatus.Green });
                    }
                }
                else if (api.Is(OrderAPIs.OrderHistory.ToString()))
                {
                    var remarks = new List<Remarks>();

                    if (orderValidationStatus?.Status != "2" || orderDataStatus?.Status != "2" || orderSearchStatus?.Status != "2")
                        response.Add(new() { Api = api, Status = RAGStatus.Red });

                    else
                    {
                        var failures = healthReportResponse.HealthReport.First(x => x.API.Contains(api)).StatusInfo.FirstOrDefault(x => x.StatusCode == "500");

                        if (failures != null && failures.Percentage > highThreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Red });
                        else if (failures != null && failures.Percentage > normalthreshold && failures.Percentage <= highThreshold)
                            response.Add(new() { Api = api, Status = RAGStatus.Amber });
                        else
                            response.Add(new() { Api = api, Status = RAGStatus.Green });
                    }
                }
            }

            return response;
        }

        private static HeartBeatInfo GetStatus(Dictionary<string, HeartBeatInfo> entries, string key)
        {
            if (entries != null && entries.TryGetValue(key, out var status))
                return status;

            return null;
        }
    }
}