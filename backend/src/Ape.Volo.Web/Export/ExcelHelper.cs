using Ape.Volo.Shared.Exceptions;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.IO.Compression;
using System.Reflection;
using System.Text.RegularExpressions;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Shared.Model;
using ClosedXML.Excel;

namespace Ape.Volo.Web.Export;

/// <summary>
/// Excel 导出（基于 ClosedXML）
/// </summary>
public class ExcelHelper
{
    #region Fields

    private const string SheetName = "Sheet1";

    #endregion

    #region Properties

    /// <summary>
    /// 单个文件的最大导出行数，超过后拆分并按 ZIP 打包
    /// </summary>
    public int ExportMaxCount { get; set; }

    /// <summary>
    /// 实际生成的 Excel 文件数量
    /// </summary>
    public int ExportExcelCount { get; set; }

    /// <summary>
    /// 导出文件第一行背景颜色，取值为 HSSFColor 颜色索引，例如：8 黑色、42 浅蓝
    /// </summary>
    public short? ExportTitleBackColor { get; set; }

    /// <summary>
    /// 导出文件第一行文字颜色，取值为 HSSFColor 颜色索引
    /// </summary>
    public short? ExportTitleFontColor { get; set; }

    #endregion

    #region Methods

    /// <summary>
    /// 生成导出文件。单文件返回 xlsx，超过 <see cref="ExportMaxCount"/> 时拆分为多个 xlsx 并打包为 zip
    /// </summary>
    /// <param name="exportRows">待导出数据</param>
    /// <param name="mimeType">输出：文件 MIME 类型</param>
    /// <param name="fileName">输出：文件后缀名（由调用方拼接业务名称）</param>
    /// <returns>文件字节内容</returns>
    public virtual byte[] GenerateExcel(List<ExportBase> exportRows, out string mimeType, out string fileName)
    {
        if (exportRows == null || exportRows.Count == 0)
        {
            throw new BadRequestException("数据不存在");
        }

        mimeType = MimeTypes.TextXlsx; //默认xlsx
        ExportMaxCount = ExportMaxCount == 0 ? 10000 : ExportMaxCount > 10000 ? 10000 : ExportMaxCount;
        ExportExcelCount = exportRows.Count < ExportMaxCount
            ? 1
            : exportRows.Count % ExportMaxCount == 0
                ? exportRows.Count / ExportMaxCount
                : exportRows.Count / ExportMaxCount + 1;

        var stamp = DateTime.Now.ToString("yyyyMMddHHmmssffff");

        //如果是1，直接下载Excel，如果是多个，下载ZIP包
        if (ExportExcelCount == 1)
        {
            fileName = $"_{stamp}.xlsx";
            return WriteWorkbook(exportRows);
        }

        mimeType = MimeTypes.ApplicationZip;
        fileName = $"_{stamp}.zip";
        return WriteZipPackage(exportRows, stamp);
    }

    /// <summary>
    /// 分片生成多个 Excel 并打包为 ZIP。全程在内存中完成，不再读写临时目录
    /// </summary>
    /// <param name="exportRows">待导出数据</param>
    /// <param name="stamp">文件名时间戳</param>
    /// <returns>ZIP 字节内容</returns>
    private byte[] WriteZipPackage(List<ExportBase> exportRows, string stamp)
    {
        using var zipStream = new MemoryStream();
        using (var archive = new ZipArchive(zipStream, ZipArchiveMode.Create, leaveOpen: true))
        {
            for (var i = 0; i < ExportExcelCount; i++)
            {
                var list = exportRows.Skip(i * ExportMaxCount).Take(ExportMaxCount).ToList();
                var bytes = WriteWorkbook(list);
                var entry = archive.CreateEntry($"export_{stamp}_{i + 1}.xlsx", CompressionLevel.Fastest);
                using var entryStream = entry.Open();
                entryStream.Write(bytes, 0, bytes.Length);
            }
        }

        return zipStream.ToArray();
    }

    /// <summary>
    /// 根据集合生成单个 Excel
    /// </summary>
    /// <param name="exportRows">待导出数据</param>
    /// <returns>xlsx 字节内容</returns>
    private byte[] WriteWorkbook(IReadOnlyList<ExportBase> exportRows)
    {
        using var book = new XLWorkbook();
        var sheet = book.Worksheets.Add(SheetName);
        var props = exportRows[0].GetType().GetProperties(BindingFlags.Instance | BindingFlags.NonPublic |
                                                          BindingFlags.Public);

        //生成表头
        MakeExcelHeader(sheet, props);

        //放入数据
        for (var i = 0; i < exportRows.Count; i++)
        {
            for (var colIndex = 0; colIndex < props.Length; colIndex++)
            {
                var property = props[colIndex];
                var propertyValue = property.GetValue(exportRows[i]);
                var text = Regex.Replace(
                    propertyValue == null ? string.Empty : propertyValue.ToString() ?? string.Empty,
                    @"<[^>]*>", string.Empty);
                var propertyType = property.PropertyType;
                var cell = sheet.Cell(i + 2, colIndex + 1);

                if (propertyType.IsNumber())
                {
                    if (double.TryParse(text, NumberStyles.Any, CultureInfo.CurrentCulture, out var number))
                    {
                        cell.Value = number;
                    }
                    else
                    {
                        cell.Value = text;
                    }
                }
                else if (propertyType.IsEnum)
                {
                    cell.Value = FormatEnum(propertyType, text);
                }
                else if (propertyType.IsBool())
                {
                    cell.Value = text.ToBool() ? "是" : "否";
                }
                else
                {
                    cell.Value = text;
                }

                ApplyBorder(cell);
            }
        }

        using var ms = new MemoryStream();
        book.SaveAs(ms);
        return ms.ToArray();
    }

    /// <summary>
    /// 生成表头行
    /// </summary>
    /// <param name="sheet">工作表</param>
    /// <param name="propertyInfos">导出字段</param>
    private void MakeExcelHeader(IXLWorksheet sheet, PropertyInfo[] propertyInfos)
    {
        var background = LegacyColor(ExportTitleBackColor, XLColor.LightBlue);
        var fontColor = LegacyColor(ExportTitleFontColor, XLColor.Black);

        for (var colIndex = 0; colIndex < propertyInfos.Length; colIndex++)
        {
            var property = propertyInfos[colIndex];
            var display = property
                .GetCustomAttributes(typeof(DisplayAttribute), true)
                .OfType<DisplayAttribute>()
                .FirstOrDefault();

            var cell = sheet.Cell(1, colIndex + 1);
            cell.Value = display == null ? property.Name : display.Name;
            cell.Style.Fill.BackgroundColor = background;
            cell.Style.Font.FontName = "Calibri";
            cell.Style.Font.FontSize = 12;
            cell.Style.Font.FontColor = fontColor;
            ApplyBorder(cell);
        }
    }

    /// <summary>
    /// 应用单元格边框
    /// </summary>
    /// <param name="cell">单元格</param>
    private static void ApplyBorder(IXLCell cell)
    {
        cell.Style.Border.BottomBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.TopBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.LeftBorder = XLBorderStyleValues.Thin;
        cell.Style.Border.RightBorder = XLBorderStyleValues.Thin;
    }

    /// <summary>
    /// 将 HSSFColor 颜色索引转换为 ClosedXML 颜色。仅覆盖配置中可能出现的取值，其余回退默认色
    /// </summary>
    /// <param name="index">HSSFColor 索引</param>
    /// <param name="fallback">默认颜色</param>
    /// <returns>ClosedXML 颜色</returns>
    private static XLColor LegacyColor(short? index, XLColor fallback) => index switch
    {
        null => fallback,
        8 => XLColor.Black,
        9 => XLColor.White,
        10 => XLColor.Red,
        11 => XLColor.Green,
        12 => XLColor.Blue,
        13 => XLColor.Yellow,
        42 => XLColor.LightBlue,
        _ => fallback
    };

    /// <summary>
    /// 将枚举值格式化为 Display 名称
    /// </summary>
    /// <param name="enumType">枚举类型</param>
    /// <param name="text">枚举值文本</param>
    /// <returns>展示名称</returns>
    private static string FormatEnum(Type enumType, string text)
    {
        if (!int.TryParse(text, out var enumValue))
        {
            return text;
        }

        var eName = enumType.GetEnumName(enumValue);
        if (string.IsNullOrEmpty(eName))
        {
            return string.Empty;
        }

        var field = enumType.GetField(eName);
        if (field == null)
        {
            return eName;
        }

        var display = field
            .GetCustomAttributes(typeof(DisplayAttribute), true)
            .OfType<DisplayAttribute>()
            .FirstOrDefault();

        return display == null ? field.Name : display.Name!;
    }

    #endregion
}
