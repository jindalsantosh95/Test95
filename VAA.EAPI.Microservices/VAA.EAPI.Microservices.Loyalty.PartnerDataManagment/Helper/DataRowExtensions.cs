using System.Data;

namespace VAA.EAPI.Microservices.Loyalty.PartnerDataManagment.Helper
{
    public static class DataRowExtensions
    {
        public static string GetString(this DataRow row, string columnName)
        {
            return row[columnName] != DBNull.Value ? Convert.ToString(row[columnName]) : string.Empty;
        }

        public static int GetInt(this DataRow row, string columnName)
        {
            return row[columnName] != DBNull.Value ? Convert.ToInt32(row[columnName]) : 0;
        }

        public static DateTime GetDateTimeValue(this DataRow row, string columnName)
        {
            return Convert.ToDateTime(row[columnName]);
        }
    }
}
