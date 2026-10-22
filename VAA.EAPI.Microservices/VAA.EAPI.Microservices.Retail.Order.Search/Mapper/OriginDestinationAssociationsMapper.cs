using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public class OriginDestinationAssociationsMapper
    {
        public static Dictionary<string, string> Air4ToOrderSearchResponseMapper(RetrieveResponseType response, EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType ticketingResponse, List<OriginDestinationAssociations> originDestinationAssociations, List<OrderSearchSegmentInfo> orderSearchSegmentInfos)
        {
            Dictionary<string, string> segmentODMapper = new Dictionary<string, string>();

            if (response != null && response.TeardownContextResponse.Pnr.DtiSegmentInfo != null && response.TeardownContextResponse.Pnr.SegmentDl != null && ticketingResponse == null)
                return GetOriginDestinationAssociations(response, originDestinationAssociations, orderSearchSegmentInfos);

            if (response != null && response.TeardownContextResponse.Pnr.DtiSegmentInfo == null && ticketingResponse != null)
                GetOriginDestinationsForUpgrade(ticketingResponse, originDestinationAssociations);

            return segmentODMapper.Any() ? segmentODMapper : null;
        }
        private static Dictionary<string, string> GetOriginDestinationAssociations(RetrieveResponseType response, List<OriginDestinationAssociations> orginDestinations, List<OrderSearchSegmentInfo> orderSearchSegmentInfos)
        {
            Dictionary<string, string> segmentODMapper = new();
            OriginDestination originDestination = new();
            int odLable = 1;

            var dtiSegmentInfos = response.TeardownContextResponse.Pnr.DtiSegmentInfo.GroupBy(item => item.SegmentNumber).Select(q => q.FirstOrDefault()).Where(item => item.SegmentNumber != null).ToArray();

            if (dtiSegmentInfos.Length == 1)
            {
                string odLabel = $"{Constants.Application.OriginDestPrefix}{odLable}";
                originDestination = GetOriginDestination(response, dtiSegmentInfos.First().SegmentNumber, dtiSegmentInfos.First().SegmentNumber, segmentODMapper, odLabel, orderSearchSegmentInfos);
                orginDestinations.Add(new OriginDestinationAssociations() { OriginDestination = originDestination, ODLabel = odLabel });
            }
            else if (dtiSegmentInfos.Length > 1)
                for (int originCounter = 0; originCounter < dtiSegmentInfos.Length; originCounter++)
                {
                    int destinationIndex = originCounter + 1;
                    string originSegNo = dtiSegmentInfos[originCounter].SegmentNumber;
                    string destinationSegno = string.Empty;

                    for (int designationCounter = destinationIndex; designationCounter < dtiSegmentInfos.Length; designationCounter++)
                    {
                        destinationIndex = designationCounter - 1;
                        if (dtiSegmentInfos[designationCounter].StopFlag == Constants.Application.DestinationStopFlag)
                        {
                            destinationSegno = dtiSegmentInfos[designationCounter].SegmentNumber;
                            continue;
                        }
                        else if (dtiSegmentInfos[designationCounter].StopFlag == Constants.Application.OriginStopFlag)
                        {
                            destinationSegno = dtiSegmentInfos[destinationIndex].SegmentNumber;
                            break;
                        }
                    }

                    if (destinationIndex == dtiSegmentInfos.Length && originCounter == dtiSegmentInfos.Length - 1)
                    {
                        if (dtiSegmentInfos[originCounter].StopFlag == Constants.Application.OriginStopFlag)
                            destinationSegno = dtiSegmentInfos[originCounter].SegmentNumber;

                        destinationIndex = originCounter;
                    }

                    originCounter = destinationIndex;

                    if (!string.IsNullOrEmpty(destinationSegno))
                    {
                        string odLabel = $"{Constants.Application.OriginDestPrefix}{odLable++}";
                        originDestination = GetOriginDestination(response, originSegNo, destinationSegno, segmentODMapper, odLabel, orderSearchSegmentInfos);
                        orginDestinations.Add(new OriginDestinationAssociations() { OriginDestination = originDestination, ODLabel = odLabel });

                    }
                }

            return segmentODMapper;
        }
        private static OriginDestination GetOriginDestination(RetrieveResponseType response, string originSegNo, string destinationSegNo, Dictionary<string, string> segmentODMapper, string odLabel, List<OrderSearchSegmentInfo> orderSearchSegmentInfos)
        {
            OriginDestination originDestination = new();


            originSegNo = orderSearchSegmentInfos.FirstOrDefault(s => s.DtiSegNo == originSegNo).SegDlSegNo;
            destinationSegNo = orderSearchSegmentInfos.FirstOrDefault(s => s.DtiSegNo == destinationSegNo).SegDlSegNo;

            var segmentDIInfos = response.TeardownContextResponse.Pnr.SegmentDl.Where(segDl => segDl.ArrivalPoint != null && segDl.DeparturePoint != null);
            var segmentOaInfos = response.TeardownContextResponse.Pnr?.SegmentOa?.Where(segOa => segOa.ArrivalPoint != null && segOa.DeparturePoint != null);
            var segmentAdditionalInfos = response.TeardownContextResponse.Pnr.SegmentAdditionalInfo.Where(segAddInfo => segAddInfo.SegmentNumber != null);

            originDestination.OriginAirport = segmentDIInfos.Any(seg => seg.SegmentNumber == originSegNo) ? segmentDIInfos.Where(seg => seg.SegmentNumber == originSegNo).First().DeparturePoint : segmentOaInfos.Where(seg => seg.SegmentNumber == originSegNo).First().DeparturePoint;
            originDestination.DestinationAirport = segmentDIInfos.Any(seg => seg.SegmentNumber == destinationSegNo) ? segmentDIInfos.Where(seg => seg.SegmentNumber == destinationSegNo).First().ArrivalPoint : segmentOaInfos.Where(seg => seg.SegmentNumber == destinationSegNo).First().ArrivalPoint;
            if (segmentAdditionalInfos.Where(seg => seg.SegmentNumber == destinationSegNo).First().AccumulatedTime != null)
                originDestination.Duration = segmentAdditionalInfos.Where(seg => seg.SegmentNumber == destinationSegNo).First().AccumulatedTime;
            else
                originDestination.Duration = segmentAdditionalInfos.Where(seg => seg.SegmentNumber == destinationSegNo).First().ElapsedTime;
            if (segmentDIInfos != null && segmentDIInfos.Any(x => x.SegmentNumber.Contains(originSegNo)))
            {
                var segmentDIInfo = segmentDIInfos.First(seg => seg.SegmentNumber == originSegNo);
                originDestination.DepartureDate = DateTimeHelper.Air4DepartDateFormatConversion(segmentDIInfo.DepartureDate, segmentDIInfo.SegmentDepartureYear, segmentDIInfo.DepartureTime)?.ToString(Constants.Application.DateFormat);
            }
            else if (segmentOaInfos != null)
            {
                var segmentOaInfo = segmentOaInfos.First(seg => seg.SegmentNumber == originSegNo);
                originDestination.DepartureDate = DateTimeHelper.Air4DepartDateFormatConversion(segmentOaInfo.DepartureDate, segmentOaInfo.SegmentDepartureYear, segmentOaInfo.DepartureTime)?.ToString(Constants.Application.DateFormat);
            }

            foreach (var segNo in orderSearchSegmentInfos)
            {
                if ((segNo.DtiSegNo == originSegNo || segNo.SegDlSegNo == originSegNo) && !segmentODMapper.ContainsKey(segNo.SegDlSegNo.ToString()))
                    segmentODMapper.Add(segNo.SegDlSegNo, odLabel);
                if ((Convert.ToInt32(segNo.DtiSegNo) <= Convert.ToInt32(destinationSegNo) || Convert.ToInt32(segNo.SegDlSegNo) <= Convert.ToInt32(destinationSegNo)) && !segmentODMapper.ContainsKey(segNo.SegDlSegNo.ToString()))
                    segmentODMapper.Add(segNo.SegDlSegNo, odLabel);
            }

            return originDestination;
        }
        private static void GetOriginDestinationsForUpgrade(EAPI.Infrastructure.PSS.Proxy.TicketingProcessV3Proxy.RetrieveResponseType ticketingResponse, List<OriginDestinationAssociations> originDestinationAssociations)
        {
            var ticketSegmentInfos = ticketingResponse.TicketRequestResponse
                                                      .FirstOrDefault()?
                                                      .TicketResponse
                                                      .FirstOrDefault()?
                                                      .AlternateCouponFltHistory
                                                      .Where(item => item.CouponSequenceNumber != null && !string.IsNullOrEmpty(item.CityCode) && !string.IsNullOrEmpty(item.FlightDestinationAirport) && item.MarketedByFlightNumber != Constants.Application.ARNKSectorVoid && item.OperatedByFlightNumber != Constants.Application.ARNKSectorVoid)
                                                      .ToArray();

            if (ticketSegmentInfos != null)
            {
                int odLable = 1;
                OriginDestination originDestination = new();

                if (ticketSegmentInfos.Length == 1)
                {
                    originDestination.OriginAirport = ticketSegmentInfos.FirstOrDefault().CityCode;
                    originDestination.DestinationAirport = ticketSegmentInfos.FirstOrDefault().FlightDestinationAirport;
                    originDestination.DepartureDate = DateTimeHelper.Air4PNRDateTimeConversion(ticketSegmentInfos.FirstOrDefault().FlightDepartureDate, ticketSegmentInfos.FirstOrDefault().FlightDepartureTime)?.ToString(Constants.Application.DateFormat);
                    originDestinationAssociations.Add(new OriginDestinationAssociations() { OriginDestination = originDestination, ODLabel = Constants.Application.OriginDestPrefix + ticketSegmentInfos.FirstOrDefault().CouponSequenceNumber.Trim(Constants.Application.InstructionTrimChar) });
                }
                else if (ticketSegmentInfos.Length > 1)
                {
                    // Multi-City scenario
                    if (!ticketSegmentInfos.Any(item => item.StopoverCode != null && item.StopoverCode.Contains(Constants.Application.DestinationStopFlag)))
                    {
                        foreach (var ticketSegmentInfo in ticketSegmentInfos)
                        {
                            originDestination.OriginAirport = ticketSegmentInfo.CityCode;
                            originDestination.DestinationAirport = ticketSegmentInfo.FlightDestinationAirport;
                            originDestination.DepartureDate = DateTimeHelper.Air4PNRDateTimeConversion(ticketSegmentInfo.FlightDepartureDate, ticketSegmentInfo.FlightDepartureTime)?.ToString(Constants.Application.DateFormat);
                            originDestinationAssociations.Add(new OriginDestinationAssociations() { OriginDestination = originDestination, ODLabel = Constants.Application.OriginDestPrefix + odLable++.ToString() });
                            originDestination = new();
                        }
                    }
                    else
                    {
                        for (int segmentCounter = 0; segmentCounter < ticketSegmentInfos.Length; segmentCounter++)
                        {
                            if (ticketSegmentInfos[segmentCounter].StopoverCode == null && segmentCounter == 0 || ticketSegmentInfos[segmentCounter].StopoverCode == Constants.Application.OriginStopFlag)
                            {
                                originDestination.OriginAirport = ticketSegmentInfos[segmentCounter].CityCode;
                                if (segmentCounter < ticketSegmentInfos.Length - 1 && ticketSegmentInfos[segmentCounter + 1].StopoverCode == Constants.Application.OriginStopFlag)
                                {
                                    originDestination.DestinationAirport = ticketSegmentInfos[segmentCounter].FlightDestinationAirport;
                                    originDestination.DepartureDate = DateTimeHelper.Air4PNRDateTimeConversion(ticketSegmentInfos[segmentCounter].FlightDepartureDate, ticketSegmentInfos[segmentCounter].FlightDepartureTime)?.ToString(Constants.Application.DateFormat);
                                    originDestinationAssociations.Add(new OriginDestinationAssociations() { OriginDestination = originDestination, ODLabel = Constants.Application.OriginDestPrefix + odLable++.ToString() });
                                    originDestination = new();
                                }
                            }
                            else if (ticketSegmentInfos[segmentCounter].StopoverCode == Constants.Application.DestinationStopFlag && segmentCounter < ticketSegmentInfos.Length - 1 && ticketSegmentInfos[segmentCounter + 1].StopoverCode == Constants.Application.OriginStopFlag)
                            {
                                originDestination.DestinationAirport = ticketSegmentInfos[segmentCounter].FlightDestinationAirport;
                                originDestination.DepartureDate = DateTimeHelper.Air4PNRDateTimeConversion(ticketSegmentInfos[segmentCounter].FlightDepartureDate, ticketSegmentInfos[segmentCounter].FlightDepartureTime)?.ToString(Constants.Application.DateFormat);
                                originDestinationAssociations.Add(new OriginDestinationAssociations() { OriginDestination = originDestination, ODLabel = Constants.Application.OriginDestPrefix + odLable++.ToString() });
                                originDestination = new();
                            }
                        }
                        // To get the last segment info as DestinationAirport
                        if (ticketSegmentInfos[^1].StopoverCode == Constants.Application.DestinationStopFlag || ticketSegmentInfos[^1].StopoverCode == Constants.Application.OriginStopFlag)
                        {
                            originDestination.DestinationAirport = ticketSegmentInfos[^1].FlightDestinationAirport;
                            originDestination.DepartureDate = DateTimeHelper.Air4PNRDateTimeConversion(ticketSegmentInfos[^1].FlightDepartureDate, ticketSegmentInfos[^1].FlightDepartureTime)?.ToString(Constants.Application.DateFormat);
                            originDestinationAssociations.Add(new OriginDestinationAssociations() { OriginDestination = originDestination, ODLabel = Constants.Application.OriginDestPrefix + odLable++.ToString() });
                        }
                    }
                }
            }
        }


    }
}
