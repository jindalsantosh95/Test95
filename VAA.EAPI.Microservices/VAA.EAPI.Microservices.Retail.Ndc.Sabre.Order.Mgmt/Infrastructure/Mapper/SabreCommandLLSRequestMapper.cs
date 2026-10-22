using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.NDC.Sabre.Proxy.SabreCommandLLS;

namespace VAA.EAPI.Microservices.Retail.Ndc.Sabre.Order.Mgmt.Infrastructure.Mapper
{
    public static class SabreCommandLLSRequestMapper
    {
        public static SabreCommandLLSRQRequest1 GetSabreCommandRequest(string token, string hostcommand)
        {
            return new()
            {
                MessageHeader = new()
                {
                    ConversationId = ConfigReader.Get(Constants.Environment.ConversationId),
                    From = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.FromPartyId) }]
                    },
                    To = new()
                    {
                        PartyId = [new() { Value = ConfigReader.Get(Constants.Environment.ToPartyId) }]
                    },
                    CPAId = "7KFA",
                    Service = new() { type = "OTA", Value = "Air"},
                    Action = Constants.Application.SabreCommandLLSAction
                },
                Security = new() { BinarySecurityToken = token },
                SabreCommandLLSRQ = SabreCommandLLSReqMapper(hostcommand)
            };
        }

        private static SabreCommandLLSRQ SabreCommandLLSReqMapper(string hostcommand)
        {
            return new()
            {
                NumResponses = Constants.Application.SabreCommandLLSNumResponses,
                Version = Constants.Application.SabreCommandLLSVersion,
                Request = new SabreCommandLLSRQRequest()
                {
                    CDATA = "true",
                    Output = SabreCommandLLSRQRequestOutput.SCREEN,
                    OutputSpecified = true,
                    HostCommand = hostcommand
                }
            };
        }
    }
}
