using System.Collections;
using System.Data;
using VAA.EAPI.Automation.Core.Extensions;

namespace VAA.EAPI.Automation.MyAccount.DataProvider
{
    public sealed class EnrolmentTestDataGenerator : IEnumerable<object[]>
    {
        private readonly List<object[]> data = new();

        public EnrolmentTestDataGenerator()
        {
            using (DataTable dataTable = new())
            {
                if (!dataTable.FromDelimitedFile("Tests/Data/EnrolmentMicroService.csv", ","))
                    Assert.Fail("Expected Tests/Data/EnrolmentMicroService.csv to be a valid comma-delimited test-data file, but it could not be loaded.");

                foreach (DataRow dataRow in dataTable?.Rows.OfType<DataRow>())
                    data.Add(dataRow.ItemArray);
            }
        }

        public IEnumerator<object[]> GetEnumerator() => data.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
