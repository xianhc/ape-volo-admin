using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;

namespace Ape.Volo.Core.ConfigOptions;

/// <summary>
/// 系统配置
/// </summary>
[OptionsSettings]
public class SystemOptions
{
    public RunMode RunMode { get; set; }

    /// <summary>
    /// 是否初始化数据库
    /// </summary>
    public bool IsInitDb { get; set; }

    /// <summary>
    /// 是否开启读写分离
    /// </summary>
    public bool IsCqrs { get; set; }


    /// <summary>
    /// 用户默认密码
    /// </summary>
    public string UserDefaultPassword { get; set; }

    /// <summary>
    /// 文件限制大小
    /// </summary>
    public int FileLimitSize { get; set; }

    /// <summary>
    /// Hmac签名密钥
    /// </summary>
    public string HmacSecret { get; set; }

    /// <summary>
    /// 主库
    /// </summary>
    public string MainDataBase { get; set; }

    /// <summary>
    /// 日志库
    /// </summary>
    public string LogDataBase { get; set; }

    /// <summary>
    /// 是否使用Redis缓存
    /// </summary>
    public bool UseRedisCache { get; set; }
}