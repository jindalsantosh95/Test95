using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Domain.Virgin.Models.Common;
using VAA.EAPI.Domain.Virgin.Models.Order;
using VAA.EAPI.Domain.Virgin.Models.Products.Baggage;
using VAA.EAPI.Domain.Virgin.Models.Products.Flight;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS;
using SourceDataList = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.DataList;

namespace VAA.EAPI.NDC.Order.Infrastructure.Model.InRule
{
    public class DataList
    {
        public IEnumerable<OriginDestinationAssociation> OriginDestinationAssociations { get; set; }
        public IEnumerable<Customer> CustomerDetails { get; set; }
        public IEnumerable<OrderInstructions> OrderInstructions { get; set; }
        public IEnumerable<Segment> SegmentDetails { get; set; }
        public IEnumerable<Penalty> Penalty { get; set; }
        public IEnumerable<PriceClass> PriceClass { get; set; }
        public IEnumerable<ServiceDefinition> ServiceDefinitions { get; set; }
        public IEnumerable<Journey> Journey { get; set; }
        public IEnumerable<BaggageAllowances> BaggageAllowances { get; set; }

        public static explicit operator DataList(SourceDataList source) =>
            source is null
                ? null
                : new DataList
                {
                    OriginDestinationAssociations = source.OriginDestinationAssociations,
                    CustomerDetails = source.CustomerDetails?
                        .Where(customer => customer is not null)
                        .Select(customer => (Customer)customer),
                    OrderInstructions = source.OrderInstructions,
                    SegmentDetails = source.SegmentDetails,
                    Penalty = source.Penalty,
                    PriceClass = source.PriceClass,
                    ServiceDefinitions = source.ServiceDefinitions,
                    Journey = source.Journey,
                    BaggageAllowances = source.BaggageAllowances
                };
    }
}
