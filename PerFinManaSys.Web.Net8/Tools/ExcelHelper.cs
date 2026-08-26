using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;

namespace PerFinManaSys.Web.Net8.Tools;

/// <summary>
/// Excel 通用辅助（NPOI，net8 兼容）。
/// </summary>
public static class ExcelHelper
{
    /// <summary>创建工作簿（.xls）</summary>
    public static HSSFWorkbook CreateWorkbook()
    {
        return new HSSFWorkbook();
    }

    /// <summary>写入表头</summary>
    public static void WriteHeader(ISheet sheet, params string[] headers)
    {
        var row = sheet.CreateRow(0);
        for (int i = 0; i < headers.Length; i++)
        {
            row.CreateCell(i).SetCellValue(headers[i]);
        }
    }

    /// <summary>追加一行</summary>
    public static void WriteRow(ISheet sheet, int rowIndex, params object[] values)
    {
        var row = sheet.CreateRow(rowIndex);
        for (int i = 0; i < values.Length; i++)
        {
            row.CreateCell(i).SetCellValue(values[i]?.ToString() ?? string.Empty);
        }
    }

    /// <summary>输出为字节数组</summary>
    public static byte[] ToBytes(HSSFWorkbook workbook)
    {
        using var ms = new MemoryStream();
        workbook.Write(ms);
        return ms.ToArray();
    }
}
