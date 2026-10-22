using AventStack.ExtentReports;
using AventStack.ExtentReports.Reporter;
using System.Runtime.CompilerServices;

namespace VAA.EAPI.Automation.Core.UI.Reporting
{
    public class ExtentReporting
    {

    }
    public class ExtentManager
    {
        private static readonly Lazy<AventStack.ExtentReports.ExtentReports> _lazy = new Lazy<AventStack.ExtentReports.ExtentReports>(() => new AventStack.ExtentReports.ExtentReports());

        public static AventStack.ExtentReports.ExtentReports Instance { get { return _lazy.Value; } }


        static ExtentManager()
        {
            //string codeBase = Assembly.GetCallingAssembly().Location;
            //string actualPath = codeBase.Substring(0, codeBase.LastIndexOf("bin"));
            //string localpath = (new Uri(actualPath)).LocalPath;
            var sparkReporter = new ExtentSparkReporter(Path.Combine(Environment.CurrentDirectory, "TestResults", "ExtentReport.html"));
            Instance.AttachReporter(sparkReporter);
        }

        //private ExtentManager()
        //{
        //}
    }

    public class ExtentTestManager
    {

        public static ExtentTest Test { get; set; }


        //public static  ExtentTest CreateTest(string testName)
        //{

        //    test = ExtentManager.Instance.CreateTest(testName);
        //    return test;
        //}

        [ThreadStatic]
        private static ExtentTest _parentTest;

        [ThreadStatic]
        private static ExtentTest _childTest;

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static ExtentTest CreateParentTest(string testName, string description = null)
        {

            _parentTest = ExtentManager.Instance.CreateTest(testName, description);
            return _parentTest;

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static ExtentTest CreateTest(string testName, string description = null)
        {
            _childTest = _parentTest.CreateNode(testName, description);
            return _childTest;

        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public static ExtentTest GetTest()
        {
            return _childTest;
        }
    }
}
