using System.IO;
using System.Text;

namespace VAA.EAPI.GraphQL.Common.Serialisation
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
