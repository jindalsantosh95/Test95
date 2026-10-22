using System;
using VAA.EAPI.GraphQL.Common.Constants;
using VAA.EAPI.GraphQL.Common.Extensions;
using VAA.EAPI.GraphQL.Common.Helper;
using VAA.EAPI.GraphQL.Domain.Account.Models;
using VAA.EAPI.GraphQL.Infrastructure.Entities.CustomerMS;
using VAA.EAPI.GraphQL.Infrastructure.Entities.PdmMS;
using Environment = VAA.EAPI.GraphQL.Common.Constants.Environment;

namespace VAA.EAPI.GraphQL.Domain.Account.Helpers
{
    public static class Helper
    {
        public static Dictionary<string, string> GetHeaders(string channelId)
        {
            return new Dictionary<string, string>
            {
                { "ChannelId", string.IsNullOrEmpty(channelId) ? Environment.EnrolmentChannelId : channelId },
                {  Application.OcpKeyHeader , ConfigReader.Get(Environment.CustomerOcpKey) }
            };
        }

        public static Dictionary<string, string> GetHeadersForIdentityMs(string channelId)
        {
            return new Dictionary<string, string>
            {
                { "ChannelId", string.IsNullOrEmpty(channelId) ? Environment.EnrolmentChannelId : channelId },
                {  Application.OcpKeyHeader , ConfigReader.Get(Environment.IdentityOcpKey) }
            };
        }

        public static string GenerateRandomId()
        {
            long unixTimestamp = DateTimeOffset.Now.ToUnixTimeSeconds();
            Random rand = new Random();
            int dRandom = rand.Next(00000000, 99999999);
            return Convert.ToString(unixTimestamp) + Convert.ToString(dRandom);
        }

        public static bool CheckCustomerName(CustomerCustMS customerDetails, IEnumerable<VSRetroFlownDetail> vSRetroFlownDetails)
        {
            if (customerDetails == null || vSRetroFlownDetails == null || !vSRetroFlownDetails.Any())
                return false;

            string pnrName = customerDetails.PnrName;

            string responseLastName = new string((pnrName?.Split('/').ElementAtOrDefault(0) ?? "").Where(char.IsLetter).ToArray());
            string responseFirstName = new string((pnrName?.Split('/').ElementAtOrDefault(1) ?? "").Where(char.IsLetter).ToArray());
            responseFirstName = responseFirstName.Length >= 3 ? responseFirstName.Substring(0, 3) : responseFirstName;

            var flownDetail = vSRetroFlownDetails.First();

            string ticketedLastName = new string((flownDetail.LastName ?? "").Where(char.IsLetter).ToArray());
            string ticketedFirstName = new string((flownDetail.FirstName ?? "").Where(char.IsLetter).ToArray());
            ticketedFirstName = ticketedFirstName.Length >= 3 ? ticketedFirstName.Substring(0, 3) : ticketedFirstName;

            return responseLastName.Is(ticketedLastName) && responseFirstName.Is(ticketedFirstName);
        }

        public static bool CheckActivityDate(IEnumerable<VSRetroFlownDetail> vSRetroFlownDetails, DateTime currentDate)
        {
            return vSRetroFlownDetails.All(x => x.DepartureDate <= currentDate.AddHours(72) && x.DepartureDate >= currentDate.AddMonths(-6));
        }

        internal static bool CheckMemberStatus(string? status)
        {
            return status is not null
                && RegexCompiled.Replace(status, @"\s+", string.Empty).IsAny(
                    MemberStatus.StatusEnum.Active.ToString(),
                    MemberStatus.StatusEnum.ActivePending.ToString(),
                    MemberStatus.StatusEnum.Inactive.ToString());
        }
    }
}

