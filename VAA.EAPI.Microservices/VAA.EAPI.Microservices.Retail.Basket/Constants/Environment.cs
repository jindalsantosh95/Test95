using System;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Microservices.Retail.Basket.Constants
{
    public static class Environment
    {
        public static readonly TimeSpan RedisExpiryTime = ConfigReader.Try("RedisExpiryTime", TimeSpan.FromMinutes(30));
        public static readonly string RedisConnectionString = "RedisConnectionString";
        public static readonly string BasketApimUrl = "BasketApimURL";
    }
}
