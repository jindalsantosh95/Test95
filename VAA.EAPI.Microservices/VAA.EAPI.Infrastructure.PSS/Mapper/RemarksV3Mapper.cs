using VAA.EAPI.Common.Helpers;
using VAA.EAPI.Infrastructure.PSS.Proxy.RemarksV3;
using VAA.EAPI.Infrastructure.PSS.Repository.Command;

namespace VAA.EAPI.Infrastructure.PSS.Mapper
{
    public class RemarksV3Mapper
    {

        public static AddSpecialRemarkRequestType MapSpecialRemarkRequest(AddRemarksCommand addRemarksCommand)
        {
            var addSpecialRemarkRequestType = new AddSpecialRemarkRequestType
            {
                RequestInfo = new RequestInfoType()
                {
                    ApplicationId = ConfigReader.Get(Constants.Environment.ApplicationId),
                    DebugOptions = ConfigReader.Get(Constants.Environment.DebugOptions),
                    TestLab = ConfigReader.Get(Constants.Environment.TestLab),
                    HostContextParams = new HostContextParamsType()
                    {
                        PoolType = ConfigReader.Get(Constants.Environment.PoolType)
                    },
                    SessionId = addRemarksCommand.SessionId
                },

                RemarkInfoFreeFormText = new RemarkInfoFreeFormTextType
                {
                    FreeformText = addRemarksCommand.Remarks,
                }
            };
            return addSpecialRemarkRequestType;
        }
    }

}