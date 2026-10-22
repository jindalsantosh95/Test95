using System.Collections.Generic;
using VAA.EAPI.Infrastructure.PSS.Proxy.PNRCommonV6Proxy;
using VAA.EAPI.Microservices.Retail.Order.Search.Domain;

namespace VAA.EAPI.Microservices.Retail.Order.Search.Mapper
{
    public class OrderInstructionsMapper
    {
        public static List<OrderInstructions> Air4ToOrderInstructionMapper(RetrieveResponseType response)
        {
            List<OrderInstructions> orderInstructions = new();
            int instructionId = 1;

            if (response.TeardownContextResponse.Pnr.SpecialServiceRequest != null)
                foreach (var ssr in response.TeardownContextResponse.Pnr.SpecialServiceRequest)
                {
                    orderInstructions.Add(new OrderInstructions()
                    {
                        ItemCode = ssr.RequestCode,
                        Status = ssr.StatusCode,
                        ItemText = ssr.Remarks,
                        ItemType = OrderInstructions.ItemTypes.SSREnum,
                        InstructionId = $"{Constants.Application.InstructionPrefix}{instructionId++}"
                    });
                }

            return orderInstructions;
        }
    }
}
