using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.OTACancel;
using VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange;
using static VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Domain.OrderChange.FlightItems;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public class OTACancelLLSRequestMapper
    {
        public static OTA_CancelRQRequest GetOTACancelRequest(string token, IEnumerable<FlightItems> flightItems, [Optional] IEnumerable<Segment> segmentDetails)
        {
            return new()
            {
                OTA_CancelRQ = GetOTACancelLLSRQPerSeg(flightItems, segmentDetails),
                MessageHeader = new()
                {
                    Action = Constants.Application.OTACancelLLSAction,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    }
                },
                Security = new() { BinarySecurityToken = token }
            };
        }
        public static OTA_CancelRQRequest GetOTACancelLLSRQForAllSeg(string token)
        {

            return new()
            {
                OTA_CancelRQ = new() { Segment = [new() { Type = OTA_CancelRQSegmentType.entire, TypeSpecified = true }], Version = Constants.Application.SabreOTACancelVersion, },
                MessageHeader = new()
                {
                    Action = Constants.Application.OTACancelLLSAction,
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    }
                },
                Security = new() { BinarySecurityToken = token }
            };
        }

        private static OTA_CancelRQ GetOTACancelLLSRQPerSeg(IEnumerable<FlightItems> FlightItems, IEnumerable<Segment> segmentDetails)
        {
            return new()
            {
                Segment = segmentDetails != null && segmentDetails.Any() ? GetDisruptedFlightSegments(segmentDetails) : GetFlightSegments(FlightItems)
            };
        }

        private static OTA_CancelRQSegment[] GetFlightSegments(IEnumerable<FlightItems> FlightItems)
        {
            return FlightItems.Where(x => x.ActionContext == ActionContexts.Delete).SelectMany(x => x.Segments).Select(x => new OTA_CancelRQSegment()
            {
                Number = x.SegmentId.Trim('F', 'S', 'E', 'G'),
            }).ToArray();
        }

        private static OTA_CancelRQSegment[] GetDisruptedFlightSegments(IEnumerable<Segment> segmentDetails)
        {
            return segmentDetails.Where(seg => seg.Status.Code == "UN").Select(x => new OTA_CancelRQSegment()
            {
                Number = x.SegmentId.Trim('F', 'S', 'E', 'G'),
            }).ToArray();
        }
    }
}
