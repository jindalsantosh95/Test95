using VAA.EAPI.GraphQL.Domain.Book.Models.Common.Enums;

namespace VAA.EAPI.GraphQL.Domain.Book.Models.CustomerEnrollment
{
    public class Preferences
    {
        public PreferenceTypes? PreferenceType { get; set; }
        public string PreferenceValue { get; set; }

    }
}
