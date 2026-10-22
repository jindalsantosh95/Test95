using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Offer;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.NDC.Offer.Infrastructure.Model.OrderDataMS;

namespace VAA.EAPI.NDC.Offer.Mapper
{
    public static class ServiceListMapper
    {
        public static IEnumerable<OrderItemServices> ServiceListGeneration(OrderDocument orderDoc, Domain.Model.VAA.Reshop.OrderReshopRequest request)
        {
            var orderItemServices = new List<OrderItemServices>();
            var retainSvcId = request.ReshopOrder.SelectMany(x => x.Services).Select(y => y.ServiceId).Distinct().ToList();

            var flightOrderItems = orderDoc.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.FlightEnum);

            var seatOrderItems = orderDoc.OrderItems.Where(x => x.Services?.FirstOrDefault()?.Product?.ProductType == EAPI.Domain.Virgin.Models.Common.Product.ProductTypes.SeatEnum);

            var seatSegmentIds = seatOrderItems.SelectMany(x => x.Services).SelectMany(s => s.SegmentRefIds).Distinct();

            var flightSeatSegmentIds = flightOrderItems.Where(x => x.Services != null).SelectMany(x => x.Services)
                .Where(s => s?.SegmentRefIds != null && s.SegmentRefIds.Any(id => seatSegmentIds.Contains(id)))
                .GroupBy(s => s.ServiceId).ToDictionary(g => g.Key, g => g.SelectMany(s => s.SegmentRefIds)
                          .Where(id => seatSegmentIds.Contains(id)).Distinct());

            var servicesOrderItems = orderDoc.OrderItems.Where(x => x.Services?.FirstOrDefault().Product == null);

            var servicesSegmentIds = servicesOrderItems.SelectMany(x => x.Services).SelectMany(s => s.SegmentRefIds);

            var flightServicesSegmentIds = flightOrderItems.Where(x => x.Services != null).SelectMany(x => x.Services)
              .Where(s => s?.SegmentRefIds != null && s.SegmentRefIds.Any(id => servicesSegmentIds.Contains(id)))
              .GroupBy(s => s.ServiceId).ToDictionary(g => g.Key, g => g.SelectMany(s => s.SegmentRefIds)
                        .Where(id => servicesSegmentIds.Contains(id)).Distinct());

            var checkedBaggageAllowanceIds = orderDoc.DataList.BaggageAllowances.Where(x => x.BaggageAllowanceTypeCode.Equals("Checked", StringComparison.OrdinalIgnoreCase) && x.FreeBaggageAllowance?.AllowedBagCnt == 0).Select(x => x.BaggageAllowanceId);
            var flownSegmentId = orderDoc.DataList.SegmentDetails.Where(x => x.Status?.DeliveryStatusCode == "Delivered").Select(x => x.SegmentId);
           
            // Process flight order items
            var flightServices = flightOrderItems.Select(item =>
            {
                var services = GetOrderServices(item.Services, checkedBaggageAllowanceIds, flownSegmentId);
                var retainSvcIds = request.ReshopOrder.Where(x => x.OrderItemId == item.OrderItemId).SelectMany(x => x.Services).Select(x => x.ServiceId).Distinct().ToList();

                return new OrderItemServices
                {
                    OrderItemId = item.OrderItemId,
                    RetainServices = services.Where(x => retainSvcIds.Contains(x.ServiceId) || retainSvcIds.Contains(x.ParentServiceRefId)).Select(x => x.ServiceId),
                    DeleteServices = services.Where(x => !retainSvcIds.Contains(x.ServiceId) && !retainSvcIds.Contains(x.ParentServiceRefId)).Select(x => x.ServiceId),
                    ServiceBeneficiaries = item.Services.First().ServiceBeneficiaries,
                    OrderProductType = Constants.Application.FlightType
                };
            });

            // Process SSR order items
            var ssrServices = GetSSR_Services(servicesOrderItems, retainSvcId, flightServicesSegmentIds);

            // Process Seat order items
            var seatServices = seatOrderItems.Select(item =>
            {
                var seatSegmentRefIds = item.Services?.FirstOrDefault()?.SegmentRefIds;
                var matchingServiceIds = flightSeatSegmentIds.Where(x => x.Value.SequenceEqual(seatSegmentRefIds)).Select(x => x.Key);

                return new OrderItemServices
                {
                    OrderItemId = item.OrderItemId,
                    RetainServices = matchingServiceIds.Where(x => retainSvcId.Contains(x)),
                    DeleteServices = matchingServiceIds.Where(x => !retainSvcId.Contains(x)),
                    ServiceBeneficiaries = item.Services.First().ServiceBeneficiaries,
                    OrderProductType = Constants.Application.SeatType
                };
            });

            return flightServices.Concat(seatServices).Concat(ssrServices);
        }

        private static IEnumerable<OrderItemServices> GetSSR_Services(IEnumerable<OrderItems> servicesOrderItems, List<string> retainSvcId, Dictionary<string, IEnumerable<string>> flightServicesSegmentIds)
        {
            var ssrServicesBeneficiaryType = servicesOrderItems.SelectMany(item =>
         {
             var otherServices = GetOtherOrderServices(item.Services, flightServicesSegmentIds);

             return otherServices
                 .SelectMany(service => service.ServiceBeneficiaries.Select(beneficiary => new
                 {
                     Beneficiary = beneficiary,
                     Service = service,
                     OrderItemId = item.OrderItemId
                 }));
         })
         // Step 1: Group by full beneficiary
         .GroupBy(x => x.Beneficiary).Select(g =>
         {
             var servicesForBeneficiary = g.Select(x => x.Service).Distinct();

             return new OrderItemServices
             {
                 OrderItemId = g.First().OrderItemId,
                 RetainServices = servicesForBeneficiary.Where(x => retainSvcId.Contains(x.ServiceId) || retainSvcId.Contains(x.ParentServiceRefId))
                     .Select(x => x.ServiceId),
                 DeleteServices = servicesForBeneficiary.Where(x => !retainSvcId.Contains(x.ServiceId) && !retainSvcId.Contains(x.ParentServiceRefId))
                     .Select(x => x.ServiceId),
                 ServiceBeneficiaries = new[] { g.Key },
                 OrderProductType = Constants.Application.SsrType
             };
         })

         // Step 2: Group by first 2 letters of Beneficiary name
         .GroupBy(x => x.ServiceBeneficiaries.First().Substring(0, 2).ToUpper())
         .Select(g => new OrderItemServices
         {
             OrderItemId = g.First().OrderItemId,
             RetainServices = g.SelectMany(x => x.RetainServices).Distinct(),
             DeleteServices = g.SelectMany(x => x.DeleteServices).Distinct(),
             ServiceBeneficiaries = g.SelectMany(x => x.ServiceBeneficiaries).Distinct().ToArray(),
             OrderProductType = Constants.Application.SsrType
         });

            return ssrServicesBeneficiaryType;
        }

        private static IEnumerable<OrderService> GetOtherOrderServices(IEnumerable<OrderService> services, Dictionary<string, IEnumerable<string>> flightServicesSegmentIds)
        {
            int serviceId = 0;
            var distinctOI = services.SelectMany(s => s.OrderInstructionRefs).Distinct();
            var oiToServiceId = distinctOI.Select((oi, index) => new { oi, serviceId = $"{Constants.Application.SV}{index + 1}" })
                               .ToDictionary(x => x.oi, x => x.serviceId);

            var serviceList = distinctOI.Select(oi =>
            {
                var matchingService = services.First(s => s.OrderInstructionRefs.Contains(oi));
                var segmentRefIds = matchingService.SegmentRefIds;
                var matchingServiceIds = flightServicesSegmentIds.Where(x => x.Value.SequenceEqual(segmentRefIds)).Select(x => x.Key);

                return new OrderService
                {
                    ServiceId = oiToServiceId[oi],
                    ServiceBeneficiaries = matchingService.ServiceBeneficiaries,
                    SegmentRefIds = matchingService.SegmentRefIds,
                    ParentServiceRefId = matchingServiceIds.First()
                };
            });

            return serviceList;
        }

        public static IEnumerable<OrderService> GetOrderServices(IEnumerable<OrderService> services, IEnumerable<string> checkedBaggageAllowanceIds, IEnumerable<string> flownSegmentId)
        {
            int serviceId = 0;

            var distinctPax = services.SelectMany(s => s.ServiceBeneficiaries)
                                      .Distinct()
                                      .ToArray();
            var paxServices = services.Where(s => s?.BaggageRefIds != null)
                                      .SelectMany(s => s.BaggageRefIds.Where(baggageRefId => !checkedBaggageAllowanceIds.Contains(baggageRefId)).Select(baggageRefId => new { Service = s, BaggageRefId = baggageRefId }))
                                      .ToArray();
            var flightServices = services.Where(s => s.Product?.ProductType == Product.ProductTypes.FlightEnum && !s.SegmentRefIds.Any(id => flownSegmentId.Contains(id)))
                                         .ToArray();

            var serviceList = distinctPax.SelectMany(pax =>
            {
                return services.Where(s => s.Product.ProductType == Product.ProductTypes.FlightEnum).Select(s =>
                {
                    return new OrderService
                    {
                        ServiceId = $"{Constants.Application.SV}{++serviceId}",
                        ServiceBeneficiaries = [pax],
                        SegmentRefIds = s.SegmentRefIds
                    };
                });
            })
                                         .ToArray();

            serviceList =
            [
                .. serviceList,
                .. distinctPax.SelectMany(pax => paxServices.Select(item =>
                                                                            {
                                                                                var parentId = serviceList.FirstOrDefault(s => s?.ServiceBeneficiaries.FirstOrDefault() == pax && s.SegmentRefIds.FirstOrDefault() == item.Service.SegmentRefIds?.FirstOrDefault())?.ServiceId;

                                                                                return new OrderService
                                                                                {
                                                                                    ServiceId = $"{Constants.Application.SV}{++serviceId}",
                                                                                    ParentServiceRefId = parentId,
                                                                                    ServiceBeneficiaries = [pax],
                                                                                    SegmentRefIds = item.Service.SegmentRefIds
                                                                                };
                                                                            }).Concat(flightServices.Select(service =>
                                                                            {
                                                                                var parentId = serviceList.FirstOrDefault(s => s?.ServiceBeneficiaries.FirstOrDefault() == pax && s.SegmentRefIds.FirstOrDefault() == service.SegmentRefIds?.FirstOrDefault())?.ServiceId;

                                                                                return new OrderService
                                                                                {
                                                                                    ServiceId = $"{Constants.Application.SV}{++serviceId}",
                                                                                    ParentServiceRefId = parentId,
                                                                                    ServiceBeneficiaries = [pax],
                                                                                    SegmentRefIds = service.SegmentRefIds
                                                                                };
                                                                            }))
                                                                        ),
            ];

            return serviceList;
        }
    }
}
