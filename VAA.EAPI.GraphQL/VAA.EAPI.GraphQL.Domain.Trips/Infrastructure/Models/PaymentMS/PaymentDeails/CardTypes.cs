using HotChocolate;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace VAA.EAPI.GraphQL.Domain.Trips.Infrastructure.Models.PaymentMS.PaymentDeails
{
    [GraphQLName("GraphqlTripsCardTypes")]

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CardTypes
    {
        VI = 0,
        CA = 1,
        AX = 2,
        TP = 3,
        DC = 4,
        DS = 5,
        TO = 6,

        Visa = VI,
        MasterCard = CA,
        AmericanExpress = AX,
        UATP = TP,
        Diners = DC,
        Discover = DS,
        Maestro = TO

    }
}
