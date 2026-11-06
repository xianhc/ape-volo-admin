using System.Text;

namespace Ape.Volo.Common.Helper;

/// <summary>
/// 文件操作帮助类
/// </summary>
public static class FileHelper
{
    #region 常量

    //GB
    private const long Gb = 1024 * 1024 * 1024;

    //MB
    private const long Mb = 1024 * 1024;

    //KB
    private const long Kb = 1024;

    #endregion

    #region 读操作

    /// <summary>
    /// 判断文件是否存在
    /// </summary>
    /// <param name="path">文件目录</param>
    /// <returns></returns>
    public static bool Exists(string path)
    {
        return File.Exists(path);
    }

    /// <summary>
    /// 获取当前程序根目录
    /// </summary>
    /// <returns></returns>
    public static string GetCurrentDir()
    {
        return AppDomain.CurrentDomain.BaseDirectory;
    }

    /// <summary>
    /// 读取
    /// </summary>
    /// <param name="fileFullPath">文件完整物理路径</param>
    /// <param name="e">编码 默认UTF8</param>
    /// <returns></returns>
    public static string ReadText(string fileFullPath, Encoding? e = null)
    {
        var result = string.Empty;

        if (File.Exists(fileFullPath))
        {
            if (e == null)
            {
                e = Encoding.UTF8;
            }

            result = File.ReadAllText(fileFullPath, e);
        }

        return result;
    }

    public static string ReadFile(string path, Encoding encode)
    {
        string s = "";
        if (!File.Exists(path))
            s = "不存在相应的目录";
        else
        {
            StreamReader f2 = new StreamReader(path, encode);
            s = f2.ReadToEnd();
            f2.Close();
            f2.Dispose();
        }

        return s;
    }

    #endregion

    #region 写操作

    public static void SaveFile(byte[] imageBytes, string path)
    {
        System.IO.File.WriteAllBytes(path, imageBytes); // ✅
    }


    /// <summary>
    /// 输出字符串到文件
    /// 注：使用系统默认编码;若文件不存在则创建新的,若存在则覆盖
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="path">文件路径</param>
    public static void WriteTxt(string content, string path)
    {
        WriteTxt(content, path, null, null);
    }

    /// <summary>
    /// 输出字符串到文件
    /// 注：使用自定义编码;若文件不存在则创建新的,若存在则覆盖
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="path">文件路径</param>
    /// <param name="encoding">编码</param>
    public static void WriteTxt(string content, string path, Encoding encoding)
    {
        WriteTxt(content, path, encoding, null);
    }

    /// <summary>
    /// 输出字符串到文件
    /// 注：使用自定义模式,使用UTF-8编码
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="path">文件路径</param>
    /// <param name="fileModel">输出方法</param>
    public static void WriteTxt(string content, string path, FileMode fileModel)
    {
        WriteTxt(content, path, Encoding.UTF8, fileModel);
    }

    /// <summary>
    /// 输出字符串到文件
    /// 注：使用自定义编码以及写入模式
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="path">文件路径</param>
    /// <param name="encoding">字符编码</param>
    /// <param name="fileModel">写入模式</param>
    public static void WriteTxt(string content, string path, Encoding encoding, FileMode fileModel)
    {
        WriteTxt(content, path, encoding, (FileMode?)fileModel);
    }

    /// <summary>
    /// 输出字符串到文件
    /// 注：使用自定义编码以及写入模式
    /// </summary>
    /// <param name="content">内容</param>
    /// <param name="path">文件路径</param>
    /// <param name="encoding">字符编码</param>
    /// <param name="fileModel">写入模式</param>
    private static void WriteTxt(string content, string path, Encoding? encoding, FileMode? fileModel)
    {
        encoding = encoding ?? Encoding.UTF8;
        fileModel = fileModel ?? FileMode.Create;
        string dir = Path.GetDirectoryName(path)!;
        if (!Directory.Exists(dir))
            if (dir != null)
                Directory.CreateDirectory(dir);

        using (FileStream fileStream = new FileStream(path, fileModel.Value))
        {
            using (StreamWriter streamWriter = new StreamWriter(fileStream, encoding))
            {
                streamWriter.Write(content);
                streamWriter.Flush();
            }
        }
    }

    #endregion

    #region 文件相关

    /// <summary>
    /// 获取文件大小
    /// </summary>
    /// <param name="size"></param>
    /// <returns></returns>
    public static string GetFileSize(long size)
    {
        string fileSize;
        if (size / Gb >= 1)
        {
            //如果当前Byte的值大于等于1GB
            fileSize = (size * 1.0f / Gb).ToString("F") + "GB";
        }
        else if (size / Mb >= 1)
        {
            //如果当前Byte的值大于等于1MB
            fileSize = (size * 1.0f / Mb).ToString("F") + "MB";
        }
        else if (size / Kb >= 1)
        {
            //如果当前Byte的值大于等于1KB
            fileSize = (size * 1.0f / Kb).ToString("F") + "KB";
        }
        else
        {
            fileSize = size + "B";
        }

        return fileSize;
    }

    /// <summary>
    /// 获取文件扩展名
    /// </summary>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static string GetExtensionName(string fileName)
    {
        string extensionName;
        try
        {
            extensionName = Path.GetExtension(fileName).TrimStart('.');
        }
        catch
        {
            extensionName = "other";
        }

        return extensionName.ToLower();
    }

    public static void Delete(string path)
    {
        try
        {
            File.Delete(path);
        }
        catch
        {
            // ignored
        }
    }

    #endregion
}
