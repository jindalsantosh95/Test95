using VAA.EAPI.Common.Serialisation;

namespace VAA.EAPI.Common.Helpers
{
    public static class Clone
    {
        public static T DeepCopy<T>(this T source)
        {
            if (source is null)
            {
                return default;
            }
            return Json.Deserialize<T>(Json.Serialize(source));
        }
    }
}
