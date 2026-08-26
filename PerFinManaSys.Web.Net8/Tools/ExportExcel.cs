using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace PerFinManaSys.Web.Net8.Tools;

/// <summary>
/// 报表 Excel 导出辅助。
/// </summary>
public static class ExportExcel
{
    public static byte[] Export(string sheetName, string[] headers, IReadOnlyList<object[]> rows)
    {
        var workbook = new HSSFWorkbook();
        var sheet = workbook.CreateSheet(sheetName);
        ExcelHelper.WriteHeader(sheet, headers);
        for (int i = 0; i < rows.Count; i++)
        {
            ExcelHelper.WriteRow(sheet, i + 1, rows[i]);
        }
        return ExcelHelper.ToBytes(workbook);
    }
}
