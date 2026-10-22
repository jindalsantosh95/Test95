using System;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Constants
{
    public class Environment
    {
        public static readonly string OpenApiDescription = "OpenApi__Description";
        public static readonly string OpenApiTokenUrl = "OpenApi__TokenUrl";
        public static readonly string OpenApiScope = "OpenApi__Scope";
        public static readonly string CommonCachingSlidingExpirationInDay = "CommonCachingSlidingExpirationInDay";
        public static readonly string CommonCachingAbsoluteExpirationInDay = "CommonCachingAbsoluteExpirationInDay";
        public static readonly string AzureBlobServiceBlobExpiration = "AzureBlobServiceBlobExpiration";
        public static readonly TimeSpan RedisExpiryTime = ConfigReader.Try("RedisExpiryTime", TimeSpan.FromDays(2));
        public static readonly string RedisConnectionString = "RedisConnectionString";

    }
}
