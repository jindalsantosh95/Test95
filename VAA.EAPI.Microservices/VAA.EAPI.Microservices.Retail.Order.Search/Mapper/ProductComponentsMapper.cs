using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Domain.Virgin.Models.Products.Brand;
using VAA.EAPI.Infrastructure.PSS.Repository.Query;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;
using OrderItems = VAA.EAPI.Microservices.Retail.Order.Search.Domain.OrderItems;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public class ProductComponentsMapper
    {
        public static async Task<List<ProductComponents>> GetAmenities(List<SegmentDetails> segmentDetails, List<OrderItems> orderItems, ICachedAmenities cachedAmenities, IMediator mediator)
        {
            var productComponents = new List<ProductComponents>();

            foreach (var seg in segmentDetails)
            {
                if (!(seg.Status.Code == Constants.Application.ScheduleChangeStatusCode))
                {
                    List<FlightSegment> flightSegments = new();

                    if (!string.IsNullOrEmpty(seg.OperatingCarrier) && seg.OperatingCarrier != Constants.Application.VsFlightCode)
                    {
                        await AddCodeShareAmenities(seg, orderItems, cachedAmenities, productComponents);
                    }
                    else if (seg.MarketingCarrier == Constants.Application.VsFlightCode && string.IsNullOrEmpty(seg.OperatingCarrier))
                    {
                        AddSegmentToRetrieveAmenitiesRequest(seg, flightSegments);
                    }
                    if (flightSegments.Count > 0)
                    {
                        await RetrieveAmenities(seg, flightSegments, productComponents, mediator, orderItems);
                    }
                }
            }
            return productComponents;
        }
        private static async Task AddCodeShareAmenities(SegmentDetails segmentDetails, List<OrderItems> orderItems, ICachedAmenities cachedAmenities, List<ProductComponents> ProductComponents)
        {
            string productComponentRefId = null;
            string ProductName = orderItems.FirstOrDefault().Services.FirstOrDefault(x => x.ProductSegmentAssociation.FlightSegmentIds.Contains(segmentDetails.SegmentNumber)).ProductOrder.ProductName.ToUpper();
            string operatingcarrier = segmentDetails.OperatingCarrier;
            if (!string.IsNullOrEmpty(operatingcarrier))
            {
                IEnumerable<AmenitiesTableEntity> amenitiesInfo = await cachedAmenities.GetAmenitiesInformation(Constants.Application.Codeshare);
                if (operatingcarrier == Constants.Application.DeltaFlights)
                    amenitiesInfo = amenitiesInfo.Where(q => q.PriceClass.ToUpper().Contains(ProductName) && q.OperatingCarrier.Is(operatingcarrier) && q.VAACabins.Is(segmentDetails.CabinType?.CabinName));
                else
                    amenitiesInfo = amenitiesInfo.Where(q => q.PriceClass.ToUpper().Contains(ProductName) && q.OperatingCarrier.Is(Constants.Application.OtherFlights) && q.VAACabins.Is(segmentDetails.CabinType.CabinName));
                if (amenitiesInfo != null)
                {
                    foreach (var item in orderItems)
                    {
                        foreach (var service in item.Services)
                        {
                            if (service.ProductSegmentAssociation.FlightSegmentIds.Contains(segmentDetails.SegmentId) && service.CurrentSeatAssignment == null)
                            {
                                foreach (var amenity in amenitiesInfo)
                                {
                                    service.ProductComponentRefIds ??= new();
                                    if (!ProductComponents.Any(x => x.Code == amenity.RowKey && x.Description == amenity.ProductComponentName))
                                    {
                                        int lastProductComponentId = ProductComponents.Any() ? Convert.ToInt32(ProductComponents.Last().Id[4..]) : 0;
                                        productComponentRefId = $"{Constants.Application.ProductComponent}{++lastProductComponentId}";
                                        ProductComponents.Add(new ProductComponents
                                        {
                                            Id = productComponentRefId,
                                            Code = amenity.RowKey,
                                            Description = amenity.ProductComponentName
                                        });
                                        service.ProductComponentRefIds.Add(productComponentRefId);
                                    }
                                    else
                                    {
                                        productComponentRefId = ProductComponents.First(x => x.Code == amenity.RowKey && x.Description == amenity.ProductComponentName).Id;
                                        bool checkProductComponentRefId = service.ProductComponentRefIds.Contains(productComponentRefId);
                                        if (!checkProductComponentRefId)
                                            service.ProductComponentRefIds.Add(productComponentRefId);
                                    }
                                }

                            }
                        }
                    }
                }
            }

        }
        public static void AddSegmentToRetrieveAmenitiesRequest(SegmentDetails segmentDetails, List<FlightSegment> flightSegments)
        {
            flightSegments.Add(new FlightSegment
            {
                Aircraft = new Aircraft { SubFleetTypeCode = segmentDetails.AirCraft },
                OriginAirportCode = segmentDetails.OriginAirportCode,
                DestAirportCode = segmentDetails.DestinationAirportCode,
                OperatingFlightNum = segmentDetails.MarketingFlightNumber,
                OperatingCarrier = new OperatingCarrier { Code = segmentDetails.MarketingCarrier },
                OperatingCos = segmentDetails.ClassOfService,
                SchedDepartLocalTs = $"{segmentDetails.ScheduledDepartureTime.LocalDateTime}:00"
            });
        }
        public static async Task RetrieveAmenities(SegmentDetails segmentDetails, List<FlightSegment> flightSegments, List<ProductComponents> ProductComponents, IMediator mediator, List<OrderItems> orderItems)
        {
            string productComponentRefId = null;

            var amenities = await mediator.Send(new RetrieveAmenitiesQuery { Segment = flightSegments });

            if (amenities != null)
            {
                foreach (var item in orderItems)
                {
                    foreach (var service in item.Services)
                    {
                        if (service.ProductSegmentAssociation.FlightSegmentIds.Contains(segmentDetails.SegmentId) && service.CurrentSeatAssignment == null)
                        {
                            foreach (var amenity in amenities.First(x => x.OriginAirportCode == flightSegments.FirstOrDefault().OriginAirportCode && x.DestAirportCode == flightSegments.FirstOrDefault().DestAirportCode).FlightLeg.First().BrandProduct.First().Product)
                            {
                                service.ProductComponentRefIds ??= new();
                                if (!ProductComponents.Any(x => x.Code == amenity.Id && x.Description == amenity.ShortDesc))
                                {
                                    int lastProductComponentId = ProductComponents.Any() ? Convert.ToInt32(ProductComponents.Last().Id[4..]) : 0;
                                    productComponentRefId = $"{Constants.Application.ProductComponent}{++lastProductComponentId}";
                                    ProductComponents.Add(new ProductComponents
                                    {
                                        Id = productComponentRefId,
                                        Code = amenity.Id,
                                        Description = amenity.ShortDesc
                                    });
                                    service.ProductComponentRefIds.Add(productComponentRefId);
                                }
                                else
                                {
                                    productComponentRefId = ProductComponents.First(x => x.Code == amenity.Id && x.Description == amenity.ShortDesc).Id;
                                    bool checkProductComponentRefId = service.ProductComponentRefIds.Contains(productComponentRefId);
                                    if (!checkProductComponentRefId)
                                        service.ProductComponentRefIds.Add(productComponentRefId);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
