using System.Text;

namespace VAA.EAPI.Automation.Core.Xunit.Contexts
{
    public interface ITestContext
    {
        static ITestContext Current { get; private set; }

        bool CanAccess { get; }

        bool Attach(string filePath);
        bool Attach(string filePath, out Exception exception);

        bool Attach(StringBuilder stringBuilder, string fileName);
        bool Attach(StringBuilder stringBuilder, string fileName, out Exception exception);

        bool Attach(ReadOnlySpan<byte> bytes, string fileName);
        bool Attach(ReadOnlySpan<byte> bytes, string fileName, out Exception exception);

        bool Attach(Stream stream, string fileName);
        bool Attach(Stream stream, string fileName, out Exception exception);

        void Complete();
        void Complete(out Exception exception);
    }
}
