using System.ComponentModel;
using System.Runtime.Serialization;

namespace VAA.EAPI.NDC.Offer.Enums
{
    public enum CabinTypeEnum
    {

        UpperClass = 2,
        EconomyLight = 3,
        Premium = 4,
        EconomyClassic = 5,
        EconomyDelight = 6
    }

    public enum ParticipantTypeEnum
    {

        AGG = 1,
        SP = 2,
        TMC = 3

    }

    public enum SenderTypeEnum
    {
        TA = 1,
        CORP = 2
    }

    public enum DistributionChainRoleCodeEnum
    {
        Carrier = 1,
        Distributor = 2,
        Seller = 3
    }

    public enum CabinCodesEnum
    {
        BUS_FIRST = 2,
        PREMIUM_ECONOMY = 4,
        Economy = 5
    }

    public enum ITACabinTypeEnum
    {
        [Description("BUSINESS-FIRST")]
        UpperClass = 2,
        [Description("ECONOMY-LIGHT")]
        EconomyLight = 3,
        [Description("COMFORT-PLUS-PREMIUM-ECONOMY")]
        Premium = 4,
        [Description("ECONOMY")]
        EconomyClassic = 5,
        [Description("ECONOMY-DELIGHT")]
        EconomyDelight = 6
    }

    public enum OrderActionContext
    {
        [EnumMember(Value = "UPDATE")]
        UPDATE,
        [EnumMember(Value = "REPRICE")]
        REPRICE,
        [EnumMember(Value = "CANCEL")]
        CANCEL
    }

    public enum ResultCode
    {
        Ok = 0,
        Error = 2,
        Unauthorized = 3,
        BusinessException = 5,
        BusinessWarnings = 6
    }
}