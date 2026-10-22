using System;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Domain.OrderCreateRQ;
using VAA.EAPI.NDC.Order.Domain.OrderHistoryRQ;
using VAA.EAPI.NDC.Order.Domain.OrderListRQ;
using VAA.EAPI.NDC.Order.Domain.OrderRetrieveRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model;

namespace VAA.EAPI.NDC.Order.Mapper
{
    public class OrderValidationMapper
    {
        public static IEnumerable<Participant> GetPartiesForOrderCreate(IATA_OrderCreateRQ request)
        {
            var sortedDistributionChain = (request.DistributionChain.Where(x => x.OrgRole != VAA.EAPI.NDC.Order.Domain.OrderCreateRQ.DistributionChainRoleCodeType.Carrier).OrderBy(x => int.Parse(x.Ordinal)));

            return sortedDistributionChain.Select(x =>
            new Participant()
            {
                AgencyID = x.ParticipatingOrg?.OrgID,
                Name = x.ParticipatingOrg?.Name,
                PartyType = !string.IsNullOrEmpty(Convert.ToString(x.OrgRole)) ? (Participant.PartyTypeEnum)Enum.Parse(typeof(Participant.PartyTypeEnum), (x.OrgRole).ToString(), true) : null
            });
        }

        public static IEnumerable<Participant> GetPartiesForOrderRetrieve(IATA_OrderRetrieveRQ request)
        {
            var sortedDistributionChain = (request.DistributionChain.Where(x => x.OrgRole != Domain.OrderRetrieveRQ.DistributionChainRoleCodeType.Carrier).OrderBy(x => int.Parse(x?.Ordinal))); 

            return sortedDistributionChain.Select(x =>
            new Participant()
            {
                AgencyID = x.ParticipatingOrg?.OrgID,
                Name = x.ParticipatingOrg?.Name,
                PartyType = !string.IsNullOrEmpty(Convert.ToString(x.OrgRole)) ? (Participant.PartyTypeEnum)Enum.Parse(typeof(Participant.PartyTypeEnum), (x.OrgRole).ToString(), true) : null
            });
        }
        public static IEnumerable<Participant> GetPartiesForOrderHistory(IATA_OrderHistoryRQ request)
        {
            var sortedDistributionChain = (request.DistributionChain.Where(x => x.OrgRole != Domain.OrderHistoryRQ.DistributionChainRoleCodeType.Carrier).OrderBy(x => int.Parse(x?.Ordinal)));

            return sortedDistributionChain.Select(x =>
            new Participant()
            {
                AgencyID = x.ParticipatingOrg?.OrgID,
                Name = x.ParticipatingOrg?.Name,
                PartyType = !string.IsNullOrEmpty(Convert.ToString(x.OrgRole)) ? (Participant.PartyTypeEnum)Enum.Parse(typeof(Participant.PartyTypeEnum), (x.OrgRole).ToString(), true) : null
            });
        }
        public static IEnumerable<Participant> GetPartiesForOrderChange(IATA_OrderChangeRQ request)
        {
            var sortedDistributionChain = (request.DistributionChain.Where(x => x.OrgRole != Domain.OrderChangeRQ.DistributionChainRoleCodeType.Carrier).OrderBy(x => int.Parse(x?.Ordinal))); 

            return sortedDistributionChain.Select(x =>
            new Participant()
            {
                AgencyID = x.ParticipatingOrg?.OrgID,
                Name = x.ParticipatingOrg?.Name,
                PartyType = !string.IsNullOrEmpty(Convert.ToString(x.OrgRole)) ? (Participant.PartyTypeEnum)Enum.Parse(typeof(Participant.PartyTypeEnum), (x.OrgRole).ToString(), true) : null
            });
        }

        public static IEnumerable<Participant> GetPartiesForOrderList(IATA_OrderListRQ request)
        {
            var sortedDistributionChain = (request.DistributionChain.Where(x => x.OrgRole != Domain.OrderListRQ.DistributionChainRoleCodeType.Carrier).OrderBy(x => int.Parse(x?.Ordinal)));

            return sortedDistributionChain.Select(x =>
            new Participant()
            {
                AgencyID = x.ParticipatingOrg?.OrgID,
                Name = x.ParticipatingOrg?.Name,
                PartyType = !string.IsNullOrEmpty(Convert.ToString(x.OrgRole)) ? (Participant.PartyTypeEnum)Enum.Parse(typeof(Participant.PartyTypeEnum), (x.OrgRole).ToString(), true) : null
            });
        }
    }
}
