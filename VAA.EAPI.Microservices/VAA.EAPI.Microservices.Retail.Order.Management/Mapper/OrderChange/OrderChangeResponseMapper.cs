using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Microservices.Retail.Order.Management.Infrastructure.Proxy.TicketingProcessV3Proxy;
using VAA.EAPI.Microservices.Retail.Order.Management.Domain.OrderChange;

namespace VAA.EAPI.Microservices.Retail.Order.Management.Mapper.OrderChange
{
    public class OrderChangeResponseMapper
    {
        public static OrderChangeResponse ResponseMapper(PerformRevenueResponseType performRevenueResponse, OrderChangeRequestData request)
        {
            List<Passenger> passengerList = new();

            passengerList.AddRange(performRevenueResponse.Pnr.Passenger);

            if (performRevenueResponse.Pnr.SpecialServiceRequest != null && performRevenueResponse.Pnr.SpecialServiceRequest.Any(i => i.RequestCode == Constants.Application.InfantPtc))
            {
                AddInfantDetailsToPassengerList(performRevenueResponse, passengerList);
            }

            return new OrderChangeResponse
            {
                Rloc = performRevenueResponse.Pnr.RecordLocator.FirstOrDefault().RecordLocator1,
                OrderDetails = new()
                {
                    Customers = CustomerMapper.Air4ToOrderChangeCustomerMapper(performRevenueResponse, request, passengerList),
                    AccountableDocumentRefs = DocumentRefMapper.Air4ToOrderChangeDocumentRefMapper(performRevenueResponse, request, passengerList),
                    Segments = SegmentMapper.Air4ToOrderChangeSegmentMapper(performRevenueResponse, request),
                    OriginDestinations = OriginDestinationMapper.Air4ToOrderChangeOnDMapper(request),
                    SpecialServiceRequestInfo = SpecialServiceRequestMapper.Air4ToOrderChangeSpecialServiceRequestMapper(performRevenueResponse, request)

                }
            };
        }
        public static void AddInfantDetailsToPassengerList(PerformRevenueResponseType performRevenueResponse, List<Passenger> list)
        {
            foreach (var X in performRevenueResponse.Pnr.TicketData)
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
    }
}
