using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Mapper
{
    public class PNRCommonV6ReqResMapper
    {
        public static SetupContextRequestType PNRCommonV6Req(string transactionId, string location, string pnr, Channel channelInfo)
        {
            if (channelInfo == null) throw new ArgumentNullException(nameof(channelInfo));
            return new SetupContextRequestType()
            {
                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = channelInfo.DlApplicationId,
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    Location = location,
                    TransactionId = transactionId,
                    HostContextParams = new HostContextParamsType { PoolType = channelInfo.DlPoolType }
                },
                StampingInfo = new StampingInfoType()
                {
                    UserAuthorizationData = new UserAuthorizationDataType { AgentCityCode = location, RoleIndicator = channelInfo.DlRoleIndicator, SecurityId = channelInfo.DlAgentId },
                    PrinterAuthorizationData = new PrinterAuthorizationDataType { ATBPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), AdminPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), BagPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), BoardingCardPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData) }
                },
                RetrieveInfo = new RetrieveInfoType()
                {
                    PassengerInfo = new PassengerInfoType { RecordLocator = pnr }
                }
            };
        }

        public static PnrSessionDetails PNRCommonV6Res(SetupContextResponseType setupContextResponse)
        {
            List<Passenger> passengerList = new();

            passengerList.AddRange(setupContextResponse.Pnr.Passenger);
            if (setupContextResponse.Pnr.SpecialServiceRequest != null && setupContextResponse.Pnr.SpecialServiceRequest.Any(i => i.RequestCode == Constants.Application.InfantPtc))
            {
                AddInfantDetails(setupContextResponse, passengerList);

            }
            AddSegNoToSSRRequest(setupContextResponse);

            return new PnrSessionDetails
            {
                SessionId = Convert.ToString(setupContextResponse.SessionId),
                OrderDetails = new OrderDetails()
                {
                    Segments = setupContextResponse.Pnr.SegmentDl != null ? (from segment in setupContextResponse.Pnr.SegmentDl
                                select new Segment()
                                {
                                    ClassOfService = segment.ClassOfService,
                                    OriginAirportCode = segment.DeparturePoint,
                                    DestinationAirportCode = segment.ArrivalPoint,
                                    SegmentId = segment.SegmentNumber,
                                    MarketingCarrier = segment.AirlineCode,
                                    MarketingFlightNumber = segment.FlightNumber
                                }).ToList(): null,
                    Customers = (from passenger in passengerList
                                 select new CustomerDetails()
                                 {
                                     CustId = passenger.CustomerId,
                                     FirstName = passenger.FirstName,
                                     LastName = passenger.LastName,
                                     Title = passenger.Title,
                                     FirstNameNumber = passenger.FirstNameNumber,
                                     LastNameNumber = passenger.LastNameNumber
                                 }).ToList(),

                    SpecialServiceRequestInfo = setupContextResponse.Pnr.SpecialServiceRequest is null ? null :
                                            (from speacialServiceReq in setupContextResponse.Pnr.SpecialServiceRequest
                                             select new SpecialServiceRequestInfo()
                                             {
                                                 AirlineCode = speacialServiceReq.AirlineCode,
                                                 FirstNameNumber = speacialServiceReq.FirstNameNumber,
                                                 LastNameNumber = speacialServiceReq.LastNameNumber,
                                                 RequestCode = speacialServiceReq.RequestCode,
                                                 StatusCode = speacialServiceReq.StatusCode,
                                                 AllPassengersFlag = speacialServiceReq.AllPassengersFlag,
                                                 AllSegmentsFlag = speacialServiceReq.AllSegmentsFlag,
                                                 SegmentNumber = speacialServiceReq.SegmentNumber
                                             }).ToList(),
                }
            };
        }
        public static TeardownContextRequestType TeardownContextRequest(string sessionId, string transactiondId, string location, Channel channelInfo)
        {
            if (channelInfo == null) throw new ArgumentNullException(nameof(channelInfo));
            return new TeardownContextRequestType()
            {
                DoNotDestroySessionFlag = Constants.Application.DoNotDestroySessionFlag,
                RequestInfo = new()
                {
                    ApplicationId = channelInfo.DlApplicationId,
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    SessionId = sessionId,
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    Location = location,
                    TransactionId = transactiondId,
                    HostContextParams = new HostContextParamsType { PoolType = channelInfo.DlPoolType }
                },
                StampingInfo = new()
                {
                    UserAuthorizationData = new UserAuthorizationDataType { AgentCityCode = location, RoleIndicator = channelInfo.DlRoleIndicator, SecurityId = channelInfo.DlAgentId },
                    PrinterAuthorizationData = new PrinterAuthorizationDataType { ATBPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), AdminPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), BagPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData), BoardingCardPrinterAddress = ConfigReader.Get(Constants.Environment.PrinterData) }
                },
                EndRequest = new()
                {
                    RedisplayFlag = Constants.Application.RedisplayFlag,
                    AuditFlag = new()
                    {
                        VerifyDTI = Constants.Application.VerifyDTI,
                    }
                }
            };
        }
        public static void AddInfantDetails(SetupContextResponseType setupContextResponse, List<Passenger> list)
        {
            foreach (var X in setupContextResponse.Pnr.TicketData)
            {
                if (X.PassengerName.Substring(0, 2).Contains(Constants.Application.InfIdentificationChar))
                {
                    string[] PassengerName = X.PassengerName.Split(Constants.Application.InfSplitChar);
                    if (PassengerName.Length > 2)
                    {
                        list.Add(new Passenger { FirstName = PassengerName[2], LastName = PassengerName[1], FirstNameNumber = X.FirstNameNumber, LastNameNumber = X.LastNameNumber });

                    }
                }
            }
        }
        public static void AddSegNoToSSRRequest(SetupContextResponseType setupContextResponse)
        {
            if (setupContextResponse.Pnr.SpecialServiceRequest != null && setupContextResponse.Pnr.SegmentDl != null)
            {
                foreach (var splServiceReq in setupContextResponse.Pnr.SpecialServiceRequest)
                {
                    if (splServiceReq.FlightNumber != null)
                    {
                        foreach (var segDl in setupContextResponse.Pnr?.SegmentDl?.Where(item => item.FlightNumber != null && item.FlightNumber.TrimStart('0').Equals(splServiceReq.FlightNumber.TrimStart('0'))))
                        {
                            splServiceReq.SegmentNumber = segDl.SegmentNumber;
                        }
                    }
                }
            }
        }

    }
}