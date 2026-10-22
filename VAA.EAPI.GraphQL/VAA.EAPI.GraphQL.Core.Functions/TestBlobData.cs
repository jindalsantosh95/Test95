using System.Text;

namespace VAA.EAPI.GraphQL.Core.Functions
{
    public sealed class TestBlobData
    {
        public Stream BlobStream => new MemoryStream(Encoding.UTF8.GetBytes("Test blob content"));

        public string Name => "testblob.txt";
    }
}
