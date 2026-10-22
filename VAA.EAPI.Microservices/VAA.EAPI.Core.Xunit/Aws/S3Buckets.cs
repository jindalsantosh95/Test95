using System.Collections;
using VAA.EAPI.Common.Extensions;
using VAA.EAPI.Infrastructure.Services.Aws;

namespace VAA.EAPI.Core.Xunit.Aws
{
    public class S3Buckets : IAwsS3Buckets
    {
        private readonly IEnumerable<IAwsS3BucketService> _buckets = [];

        public S3Buckets(params IAwsS3BucketService[] buckets)
        {
            _buckets = buckets ?? throw new ArgumentNullException(nameof(buckets));

            if (_buckets.IsNullOrEmpty()) throw new ArgumentNullException(nameof(buckets));
        }

        public IEnumerator<IAwsS3BucketService> GetEnumerator() => _buckets.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
