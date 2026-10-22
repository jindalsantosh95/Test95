using VAA.EAPI.NDC.Order.Domain;
using VAA.EAPI.NDC.Order.Domain.OrderChangeRQ;
using VAA.EAPI.NDC.Order.Infrastructure.Model.OrderManagementMS;
using VAA.EAPI.NDC.Order.Infrastructure.Services.SabreOrderManagementMS;
using OrderDocument = VAA.EAPI.NDC.Order.Infrastructure.Model.OrderMS.OrderDocument;

namespace VAA.EAPI.NDC.Order.Infrastructure.Mapper.SabreOrderManagementMS
{
    public static class SplitPnrRequestMapper
    {
        public static SplitPnrRepoCommand MapSplitPnrRequest(OrderDocument orderDoc, AugmentationPoint augmentationPoint)
        {
            return new SplitPnrRepoCommand
            {
                SplitPnrRequest = new SplitPnrRequest()
                {
                    OrderDocument = orderDoc,
                    splitPaxId = augmentationPoint?.PaxRefID
                }

            };
        }
    }
}