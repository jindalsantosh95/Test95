using VAA.EAPI.Automation.Core.Extensions;
using Excel = Microsoft.Office.Interop.Excel;

namespace VAA.EAPI.Automation.Core.Helpers
{
    public class ExcelOperations : IDisposable
    {
        private Excel.Application _excelApp;
        private Excel.Workbook _workbook;
        private readonly string _filePath;

        public ExcelOperations(string filePath)
        {
            if (filePath.IsNullOrEmpty() || !File.Exists(filePath))
                throw new FileNotFoundException("The provided Excel file path is invalid.", filePath);

            _filePath = filePath;

            _excelApp = new Excel.Application { Visible = false, DisplayAlerts = false };

            _workbook = _excelApp.Workbooks.Open(_filePath);
        }

        private Excel.Worksheet GetWorksheet(string worksheetName)
        {
            foreach (Excel.Worksheet sheet in _workbook.Sheets)
            {
                if (sheet.Name.Is(worksheetName))
                    return sheet;
            }

            throw new Exception($"Worksheet '{worksheetName}' not found.");
        }

        public string ReadData(string worksheetName, int rowNumber, int columnNumber)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Worksheet worksheet = GetWorksheet(worksheetName);
                Microsoft.Office.Interop.Excel.Range cell = worksheet.Cells[rowNumber, columnNumber] as Excel.Range;
                return cell?.Value?.ToString() ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error reading cell ({rowNumber}, {columnNumber}) from worksheet '{worksheetName}': {ex.Message}", ex);
            }
        }

        public void WriteData(string worksheetName, int rowNumber, int columnNumber, string data)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Worksheet worksheet = GetWorksheet(worksheetName);
                worksheet.Cells[rowNumber, columnNumber] = data;
                _workbook.Save();
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to save data: {ex.Message}", ex);
            }
        }

        public int GetWorkSheetRowCount(string worksheetName)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Worksheet worksheet = GetWorksheet(worksheetName);
                return worksheet.UsedRange.Rows.Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting row count for worksheet '{worksheetName}': {ex.Message}", ex);
            }
        }

        public int GetWorkSheetColumnCount(string worksheetName)
        {
            try
            {
                Microsoft.Office.Interop.Excel.Worksheet worksheet = GetWorksheet(worksheetName);
                return worksheet.UsedRange.Columns.Count;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error getting column count for worksheet '{worksheetName}': {ex.Message}", ex);
            }
        }

        public void Dispose()
        {
            _workbook?.Close(SaveChanges: true);
            _excelApp?.Quit();
        }
    }
}
