using Newtonsoft.Json;
using Newtonsoft.Json.Converters;


namespace VAA.EAPI.Microservices.Retail.Trip.Infrastructure.Proxy.DeltaUpsellOffers
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum TitleType
    {
        ABP = 1,
        ADM = 2,
        ALD = 3,
        AMB = 4,
        BP = 5,
        BRO = 6,
        CDT = 7,
        CHD = 8,
        COL = 9,
        CONG = 10,
        CP = 11,
        CPL = 12,
        DR = 13,
        FR = 14,
        GEN = 15,
        GOV = 16,
        HON = 17,
        JUDG = 18,
        LT = 19,
        MAJ = 20,
        MAYR = 21,
        MISS = 22,
        MISTER = 23,
        MR = 24,
        MRS = 25,
        MS = 26,
        MST = 27,
        MSTR = 28,
        PROF = 29,
        PVT = 30,
        REP = 31,
        REV = 32,
        SEN = 33,
        SGT = 34
    }
}
