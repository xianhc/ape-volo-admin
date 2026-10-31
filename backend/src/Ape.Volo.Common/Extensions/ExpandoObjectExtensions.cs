using System.Data;
using System.Dynamic;

namespace Ape.Volo.Common.Extensions;

public static class ExpandoObjectExtensions
{
    /// <summary>
    /// 添加属性
    /// </summary>
    /// <param name="expandoObj">动态对象</param>
    /// <param name="propertyName">属性名</param>
    /// <param name="value">属性值</param>
    public static void AddProperty(this ExpandoObject expandoObj, string propertyName, object? value)
    {
        var obj = (IDictionary<string, object?>)expandoObj;
        if (obj.ContainsKey(propertyName))
            throw new System.Exception("已存在该属性！");
        obj.Add(propertyName, value);
    }

    /// <summary>
    /// 设置属性
    /// </summary>
    /// <param name="expandoObj">动态对象</param>
    /// <param name="propertyName">属性名</param>
    /// <param name="value">属性值</param>
    public static void SetProperty(this ExpandoObject expandoObj, string propertyName, object? value)
    {
        var obj = (IDictionary<string, object?>)expandoObj;
        obj[propertyName] = value;
    }

    /// <summary>
    /// 获取属性
    /// </summary>
    /// <param name="expandoObj">动态对象</param>
    /// <param name="propertyName">属性名</param>
    /// <returns></returns>
    public static object? GetProperty(this ExpandoObject expandoObj, string propertyName)
    {
        var obj = (IDictionary<string, object?>)expandoObj;
        if (!obj.ContainsKey(propertyName))
            throw new System.Exception("不存在该属性！");
        return obj[propertyName];
    }

    /// <summary>
    /// 获取所有属性
    /// </summary>
    /// <param name="expandoObj">动态对象</param>
    /// <returns></returns>
    public static List<string> GetProperties(this ExpandoObject expandoObj)
    {
        var obj = (IDictionary<string, object?>)expandoObj;
        return obj.Keys.CastToList<string>();
    }

    /// <summary>
    /// 删除属性
    /// </summary>
    /// <param name="expandoObj">动态对象</param>
    /// <param name="propertyName">属性名</param>
    public static void RemoveProperty(this ExpandoObject expandoObj, string propertyName)
    {
        var obj = (IDictionary<string, object?>)expandoObj;
        if (!obj.ContainsKey(propertyName))
            throw new System.Exception("不存在该属性！");
        obj.Remove(propertyName);
    }

    /// <summary>
    /// 将动态属性对象ExpandoObject列表转为DataTable
    /// </summary>
    /// <param name="dataList">数据源</param>
    /// <returns></returns>
    public static DataTable? ToDataTable(this IEnumerable<ExpandoObject>? dataList)
    {
        DataTable dt = new DataTable();
        if (dataList == null)
            return null;
        var rows = dataList.ToList();
        if (rows.Count == 0)
            return dt;
        var properties = rows[0].GetProperties();
        properties.ForEach(aProperty => { dt.Columns.Add(aProperty); });
        rows.ForEach((aData, index) =>
        {
            dt.Rows.Add(dt.NewRow());
            properties.ForEach(aProperty => { dt.Rows[index][aProperty] = aData.GetProperty(aProperty) ?? DBNull.Value; });
        });

        return dt;
    }
}
