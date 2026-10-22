using Microsoft.Extensions.Logging;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using VAA.EAPI.Common.Helpers;

namespace VAA.EAPI.Infrastructure.Services.Aws
{
    public class AwsS3Buckets : IAwsS3Buckets
    {
        private const string BucektConfig = @"AWS:S3:Bucket\d+:";
        private const string BucektNameConfig = @"AWS:S3:Bucket\d+:Name";
        private const string AccessKeyConfig = @"AWS:S3:Bucket\d+:AccessKey";
        private const string SecretKeyConfig = @"AWS:S3:Bucket\d+:SecretKey";
        private const string RegionEndpointConfig = @"AWS:S3:Bucket\d+:RegionEndpoint";
        private const string TagsConfig = @"AWS:S3:Bucket\d+:Tags";

        private readonly IEnumerable<IAwsS3BucketService> _buckets = [];
        private readonly ILogger<AwsS3BucketService> _logger;

        public AwsS3Buckets(ILogger<AwsS3BucketService> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            _buckets = ConfigReader.Find(BucektConfig)
                                   .GroupBy(x => RegexCompiled.Match(x, BucektConfig).Value)
                                   .Select(x =>
                                   {
                                       var name = ConfigReader.Get(x.FirstOrDefault(x => RegexCompiled.IsMatch(x, BucektNameConfig)));
                                       var accessKey = ConfigReader.Get(x.FirstOrDefault(x => RegexCompiled.IsMatch(x, AccessKeyConfig)));
                                       var secretKey = ConfigReader.Get(x.FirstOrDefault(x => RegexCompiled.IsMatch(x, SecretKeyConfig)));
                                       var regionEndpoint = ConfigReader.Get(x.FirstOrDefault(x => RegexCompiled.IsMatch(x, RegionEndpointConfig)));
                                       var tags = ConfigReader.Try(x.FirstOrDefault(x => RegexCompiled.IsMatch(x, TagsConfig)), string.Empty).Split(',');

                                       return new AwsS3BucketService(name, accessKey, secretKey, regionEndpoint, _logger, tags);
                                   })
                                   .ToArray();
        }

        public IEnumerator<IAwsS3BucketService> GetEnumerator() => _buckets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
