using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaMessageCentre
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TitleType
    {

        ABP = 0,

        ADM,

        ALD,

        AMB,

        BP,

        BRO,

        CDT,

        CHD,

        COL,

        CONG,

        CP,

        CPL,

        DR,

        FR,

        GEN,

        GOV,

        HON,

        JUDG,

        LT,

        MAJ,

        MAYR,

        MISS,

        MISTER,

        MR,

        MRS,

        MS,

        MST,

        MSTR,

        PROF,

        PVT,

        REP,

        REV,

        SEN,

        SGT
    }
}
