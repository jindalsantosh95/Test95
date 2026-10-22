using System.Text;

namespace VAA.EAPI.GraphQL.Stitch.Core.Serialisation
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
