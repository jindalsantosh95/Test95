using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.AzureStorage;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;
using VAA.EAPI.Microservices.Retail.Order.Search.Helper.Cache.Interface;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public sealed class SegmentDetailsMapper
    {
        private static TableStorageService<ReferenceAirCraftDataEntity> _tableStorageService = new();

        static SegmentDetailsMapper()
        {
            _tableStorageService.ConnectionString = ConfigReader.Get(Constants.Environment.ReferenceDataMicroServiceDBKey);
            _tableStorageService.TableName = ConfigReader.Get(Constants.Environment.AirCraftDataTableName);
        }

        public static async Task<List<SegmentDetails>> Air4ToOrderSearchResponseMapper(RetrieveResponseType response, List<Passenger> passengers, ICachedAirport cachedAirport, List<OriginDestinationAssociations> originDestinationAssociations, Dictionary<string, string> segmentODMapper, ICachedSegmentStatusCode cachedSegmentStatusCode, ICachedCabinType cachedCabinType, List<OrderSearchSegmentInfo> orderSearchSegmentInfos)
        {
            var segmentDetails = new List<SegmentDetails>();
            int segmentId = 01;
            AirportLocation originAirportLocation = null;
            AirportLocation destinationAirportLocation = null;
            List<CabinTypeDataEntity> cabinTypeData = await cachedCabinType.GetCabinTypeData(Constants.Application.CabinTypePartitionKey);

            foreach (var segmentInfo in orderSearchSegmentInfos)
            {
                string segmentNumber = segmentInfo.SegDlSegNo;
                if (response.TeardownContextResponse.Pnr.SegmentDl != null && response.TeardownContextResponse.Pnr.SegmentDl.Select(x => x.SegmentNumber).Contains(segmentNumber))
                {
                    var segDl = response.TeardownContextResponse.Pnr.SegmentDl.FirstOrDefault(x => x.SegmentNumber == segmentNumber);
                    if (segDl.DeparturePoint != null && segDl.ArrivalPoint != null)
                    {
                        originAirportLocation = await cachedAirport.GetCachedAirport(segDl.DeparturePoint);
                        destinationAirportLocation = await cachedAirport.GetCachedAirport(segDl.ArrivalPoint);
                        segmentDetails.Add(new()
                        {
                            AirCraft = GetAirCraftNameByCode(response.TeardownContextResponse.Pnr.SegmentAdditionalInfo?.FirstOrDefault(x => x.SegmentNumber == segDl.SegmentNumber).CassEquipment).Result,
                            ClassOfService = segDl.ClassOfService,
                            DestinationAirportCode = segDl.ArrivalPoint,
                            DestinationAirportName = destinationAirportLocation?.AirportName,
                            DestinationCityCode = destinationAirportLocation?.CityCode,
                            DestinationCountryCode = destinationAirportLocation?.CountryCode,
                            DestinationCountryName = destinationAirportLocation?.CountryName,
                            MarketingCarrier = segDl.AirlineCode,
                            MarketingFlightNumber = segDl.FlightNumber,
                            OriginDestinationRefId = segmentODMapper != null && segmentODMapper.Count > 0 && segmentODMapper.ContainsKey(segDl.SegmentNumber) ? segmentODMapper[segDl.SegmentNumber] : GetODLable(originDestinationAssociations, segDl.DeparturePoint, segDl.ArrivalPoint, segmentDetails, response.TeardownContextResponse.Pnr.DtiSegmentInfo),
                            OriginAirportCode = segDl.DeparturePoint,
                            OriginAirportName = originAirportLocation?.AirportName,
                            OriginCityCode = originAirportLocation?.CityCode,
                            OriginCountryCode = originAirportLocation?.CountryCode,
                            OriginCountryName = originAirportLocation?.CountryName,
                            SegmentNumber = segDl.SegmentNumber,
                            NumberInParty = passengers?.Count.ToString(),
                            ScheduledDepartureTime = new ScheduledDepartureTime()
                            {
                                LocalDateTime = DateTimeHelper.Air4DepartDateFormatConversion(segDl.DepartureDate, segDl.SegmentDepartureYear, segDl.DepartureTime)?.ToString(Constants.Application.DateTimeFormat),
                                UtcDateTime = string.Empty
                            },
                            ScheduledArrivalTime = new ScheduledArrivalTime()
                            {
                                LocalDateTime = DateTimeHelper.Air4ArrivalDateFormatConversion(response?.TeardownContextResponse?.Pnr?.SegmentAdditionalInfo?.FirstOrDefault(x => x?.SegmentNumber == segDl?.SegmentNumber)?.LegArrivalDate, segDl.ArrivalTime)?.ToString(Constants.Application.DateTimeFormat),
                                UtcDateTime = string.Empty
                            },
                            FlightDistance = null,
                            FlightDuration = response?.TeardownContextResponse?.Pnr?.SegmentAdditionalInfo?.FirstOrDefault(x => x?.SegmentNumber == segDl?.SegmentNumber && x?.AccumulatedTime != null)?.AccumulatedTime
                                                ?? response?.TeardownContextResponse?.Pnr?.SegmentAdditionalInfo?.FirstOrDefault(x => x?.SegmentNumber == segDl?.SegmentNumber)?.ElapsedTime,
                            Legs = GetLegDetails(response.TeardownContextResponse.Pnr.SegmentAdditionalInfo?.Where(x => x.SegmentNumber == segmentNumber), segDl , null),
                            CassEquipment = response?.TeardownContextResponse?.Pnr?.SegmentAdditionalInfo?.FirstOrDefault(x => x?.SegmentNumber == segDl?.SegmentNumber)?.CassEquipment,
                            OperatingCarrier = response?.TeardownContextResponse?.Pnr?.SegmentAdditionalInfo?.FirstOrDefault(x => x?.SegmentNumber == segDl?.SegmentNumber)?.OperatingCarrierCode,
                            OperatingFlightNumber = response?.TeardownContextResponse?.Pnr?.SegmentAdditionalInfo?.FirstOrDefault(x => x?.SegmentNumber == segDl?.SegmentNumber && x?.FlightAdvisoryText != null)?.FlightAdvisoryText.Split(Constants.Application.DeltaCodeShareFlightSplitString)[1].Trim(),
                            SegmentId = segmentId++.ToString().PadLeft(2, '0'),
                            Status = new Action { Code = segDl.CurrentActionCode, Description = await cachedSegmentStatusCode.GetStatusCodeDescriptionAsync(segDl.CurrentActionCode) },
                            CabinType = new CabinType { CabinCode = cabinTypeData?.FirstOrDefault(x => x.RowKey == segDl.CompartmentNumber).CabinCode, CabinName = cabinTypeData?.FirstOrDefault(x => x.RowKey == segDl.CompartmentNumber).CabinName }
                        });
                    }
                }
                else if (response.TeardownContextResponse.Pnr.SegmentOa != null && response.TeardownContextResponse.Pnr.SegmentOa.Select(x => x.SegmentNumber).Contains(segmentNumber))
                {
                    var segOa = response.TeardownContextResponse.Pnr.SegmentOa.FirstOrDefault(x => x.SegmentNumber == segmentNumber);
                    if (segOa.DeparturePoint != null && segOa.ArrivalPoint != null)
                    {
                        originAirportLocation = await cachedAirport.GetCachedAirport(segOa.DeparturePoint);
                        var flightAdvisoryText = response.TeardownContextResponse.Pnr.SegmentAdditionalInfo?.FirstOrDefault(x => x.ArrivalPoint == segOa.ArrivalPoint && x.DeparturePoint == segOa.DeparturePoint)?.FlightAdvisoryText;
                        destinationAirportLocation = await cachedAirport.GetCachedAirport(segOa.ArrivalPoint);
                        segmentDetails.Add(new()
                        {
                            AirCraft = GetAirCraftNameByCode(response.TeardownContextResponse.Pnr.SegmentAdditionalInfo?.FirstOrDefault(x => x.SegmentNumber == segOa.SegmentNumber).CassEquipment).Result,
                            ClassOfService = segOa.ClassOfService,
                            DestinationAirportCode = segOa.ArrivalPoint,
                            DestinationAirportName = destinationAirportLocation?.AirportName,
                            DestinationCityCode = destinationAirportLocation?.CityCode,
                            DestinationCountryCode = destinationAirportLocation?.CountryCode,
                            DestinationCountryName = destinationAirportLocation?.CountryName,
                            MarketingCarrier = segOa.AirlineCode,
                            MarketingFlightNumber = segOa.FlightNumber,
                            OriginDestinationRefId = segmentODMapper != null && segmentODMapper.Count > 0 ? segmentODMapper[segOa.SegmentNumber] : GetODLable(originDestinationAssociations, segOa.DeparturePoint, segOa.ArrivalPoint, segmentDetails, response.TeardownContextResponse.Pnr.DtiSegmentInfo),
                            OriginAirportCode = segOa.DeparturePoint,
                            OriginAirportName = originAirportLocation?.AirportName,
                            OriginCityCode = originAirportLocation?.CityCode,
                            OriginCountryCode = originAirportLocation?.CountryCode,
                            OriginCountryName = originAirportLocation?.CountryName,
                            SegmentNumber = segOa.SegmentNumber,
                            NumberInParty = passengers?.Count.ToString(),
                            ScheduledDepartureTime = new ScheduledDepartureTime()
                            {
                                LocalDateTime = DateTimeHelper.Air4DepartDateFormatConversion(segOa.DepartureDate, segOa.SegmentDepartureYear, segOa.DepartureTime)?.ToString(Constants.Application.DateTimeFormat),
                                UtcDateTime = string.Empty
                            },
                            ScheduledArrivalTime = new ScheduledArrivalTime()
                            {
                                LocalDateTime = DateTimeHelper.Air4ArrivalDateFormatConversion(response.TeardownContextResponse.Pnr.SegmentAdditionalInfo?.FirstOrDefault(x => x.SegmentNumber == segOa.SegmentNumber).LegArrivalDate, segOa.ArrivalTime)?.ToString(Constants.Application.DateTimeFormat),
                                UtcDateTime = string.Empty
                            },
                            FlightDistance = null,
                            FlightDuration = response.TeardownContextResponse?.Pnr?.SegmentAdditionalInfo?.FirstOrDefault(x => x.SegmentNumber == segOa?.SegmentNumber && x?.AccumulatedTime != null)?.AccumulatedTime
                                                ?? response.TeardownContextResponse?.Pnr?.SegmentAdditionalInfo?.FirstOrDefault(x => x.SegmentNumber == segOa.SegmentNumber)?.ElapsedTime,
                            Legs = GetLegDetails(response.TeardownContextResponse.Pnr.SegmentAdditionalInfo?.Where(x => x.SegmentNumber == segmentNumber), null , segOa),
                            OperatingCarrier = response.TeardownContextResponse.Pnr.SegmentAdditionalInfo?.FirstOrDefault(x => x.ArrivalPoint == segOa.ArrivalPoint && x.DeparturePoint == segOa.DeparturePoint)?.OperatingCarrierCode,
                            CassEquipment = response.TeardownContextResponse.Pnr.SegmentAdditionalInfo?.FirstOrDefault(x => x?.SegmentNumber == segOa?.SegmentNumber)?.CassEquipment,
                            OperatingFlightNumber = flightAdvisoryText != null ? flightAdvisoryText.Contains(Constants.Application.DeltaCodeShareFlightSplitString) ? flightAdvisoryText.Split(Constants.Application.DeltaCodeShareFlightSplitString)[1].Replace(" ", string.Empty) : null : null,
                            SegmentId = segmentId++.ToString().PadLeft(2, '0'),
                            Status = new Action { Code = segOa.CurrentActionCode, Description = await cachedSegmentStatusCode.GetStatusCodeDescriptionAsync(segOa.CurrentActionCode) },
                        });
                    }
                }
            }

            return segmentDetails;
        }

        private static IEnumerable<Legs> GetLegDetails(IEnumerable<SegmentAdditionalInfo> segmentAdditionalInfos, SegmentDl segDl , SegmentOa segOa)
        {
            return segmentAdditionalInfos?.Select((x, index) =>
            new Legs()
            {
                LegDuration = null,
                ScheduledDepartureTime = new()
                {
                    LocalDateTime = (DateTimeHelper.Air4DepartDateFormatConversion(x?.FlightDepartureDate, x?.FlightLegDepartureYear, x?.DepartureTime)?.ToString(Constants.Application.DateTimeFormat)) ?? (segmentAdditionalInfos.Count() == 1 ? (segDl != null ? DateTimeHelper.Air4DepartDateFormatConversion(segDl?.DepartureDate, segDl?.SegmentDepartureYear, segDl?.DepartureTime)?.ToString(Constants.Application.DateTimeFormat) : DateTimeHelper.Air4DepartDateFormatConversion(segOa?.DepartureDate, segOa?.SegmentDepartureYear, segOa?.DepartureTime)?.ToString(Constants.Application.DateTimeFormat)) : string.Empty)
                },
                LegSequenceNumber = System.Convert.ToString(++index),
                OriginAirportCode = (x?.DeparturePoint) ?? (segmentAdditionalInfos.Count() == 1 ? (segDl != null ? segDl.DeparturePoint : segOa.DeparturePoint) : null),
                OperatingCarrier = x?.OperatingCarrierCode,
                ScheduledArrivalTime = new()
                {
                    LocalDateTime = (DateTimeHelper.Air4ArrivalDateFormatConversion(x?.LegArrivalDate, x?.ArrivalTime)?.ToString(Constants.Application.DateTimeFormat)) ?? (segmentAdditionalInfos.Count() == 1 ? (segDl != null ? DateTimeHelper.Air4ArrivalDateFormatConversion(x?.LegArrivalDate, segDl?.ArrivalTime)?.ToString(Constants.Application.DateTimeFormat) : DateTimeHelper.Air4ArrivalDateFormatConversion(x.LegArrivalDate, segOa?.ArrivalTime)?.ToString(Constants.Application.DateTimeFormat)) : string.Empty)
                },
                Destination_Airport_Code = (x?.ArrivalPoint) ?? (segmentAdditionalInfos.Count() == 1 ? (segDl != null ? segDl.ArrivalPoint : segOa.ArrivalPoint) : null)
            });
        }

        private static string GetDurationFormat(string duration, string format)
        {
            string durationHours = duration.Split(Constants.Application.Dot).First().PadLeft(2, '0');
            string durationMinutes = duration.Split(Constants.Application.Dot).Last().PadLeft(2, '0');

            if (duration != null)
                return format switch
                {
                    Constants.Application.DurationHMFormat => $"{durationHours}{Constants.Application.HourDesignator}{durationMinutes}{Constants.Application.MinutesDesignator}",
                    Constants.Application.DurationPTFormat => $"{Constants.Application.PTFormat}{durationHours}{Constants.Application.HourDesignator}{durationMinutes}{Constants.Application.MinutesDesignator}{Constants.Application.ZeroIndicator}{Constants.Application.SecondsDesignator}",
                    _ => $"{durationHours}{Constants.Application.Colon}{durationMinutes}",
                };

            return null;
        }

        private static async Task<string> GetAirCraftNameByCode(string airCraftCode)
        {
            if (string.IsNullOrEmpty(airCraftCode)) return string.Empty;

            var response = await _tableStorageService.RetrieveAsync(Constants.Application.AirCraftPartitionKey, airCraftCode);

            return response?.AircraftName ?? string.Empty;
        }

        private static string GetODLable(List<OriginDestinationAssociations> originDestinationAssociations, string departurePoint, string arrivalPoint, List<SegmentDetails> segmentDetails, DtiSegmentInfo[] dtiSegmentInfos)
        {
            OriginDestinationAssociations originDestinationAssociation = new();

            // Multi-City case
            if (dtiSegmentInfos != null && segmentDetails.Count > 0 && !dtiSegmentInfos.Any(item => item.StopFlag != null && item.StopFlag.Contains(Constants.Application.DestinationStopFlag)))
                originDestinationAssociation = originDestinationAssociations.Where(od => !segmentDetails.Any(s => s.OriginDestinationRefId.Contains(od.ODLabel)) && (od.OriginDestination.OriginAirport == departurePoint || od.OriginDestination.DestinationAirport == arrivalPoint)).FirstOrDefault();
            else if (originDestinationAssociations != null)
            {
                if (segmentDetails.Any(item => item.OriginAirportCode == departurePoint && item.DestinationAirportCode == arrivalPoint))
                    originDestinationAssociation = originDestinationAssociations.Where(item => item.OriginDestination.OriginAirport == departurePoint || item.OriginDestination.DestinationAirport == arrivalPoint).LastOrDefault();
                else
                    originDestinationAssociation = originDestinationAssociations.Where(item => item.OriginDestination.OriginAirport == departurePoint || item.OriginDestination.DestinationAirport == arrivalPoint).FirstOrDefault();
            }

            if (segmentDetails.Any(segment => segment.OriginDestinationRefId == originDestinationAssociation?.ODLabel))
            {
                originDestinationAssociation = originDestinationAssociations.FirstOrDefault(oNd => oNd.OriginDestination.OriginAirport == departurePoint && oNd.OriginDestination.DestinationAirport == arrivalPoint) ??
                    originDestinationAssociations.FirstOrDefault(oNd => oNd.OriginDestination.OriginAirport == departurePoint || oNd.OriginDestination.DestinationAirport == arrivalPoint);
            }

            return originDestinationAssociation != null ? originDestinationAssociation.ODLabel : string.Empty;
        }
    }
}
