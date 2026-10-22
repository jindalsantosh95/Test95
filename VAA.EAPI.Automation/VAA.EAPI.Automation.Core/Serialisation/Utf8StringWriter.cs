using System.IO;
using System.Text;

namespace VAA.EAPI.Automation.Core.Serialisation
{
    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
