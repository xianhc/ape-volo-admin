using Ape.Volo.Common.IdGenerator.Interfaces;

namespace Ape.Volo.Common.IdGenerator;

/// <summary>
/// Id生成
/// </summary>
public class IdHelper
{
    private static IIdGenerator _idGenInstance = default!;

    /// <summary>
    /// 设置参数，程序初始化时执行
    /// </summary>
    /// <param name="options"></param>
    public static void SetIdGeneratorOptions(IdGeneratorOptions options)
    {
        _idGenInstance = new DefaultIdGenerator(options);
    }

    /// <summary>
    /// 生成新的Id
    /// </summary>
    /// <returns></returns>
    public static long NextId()
    {
        if (_idGenInstance == null)
        {
            throw new ArgumentException("Please initialize the IdGeneratorOptions parameter configuration first.");
        }

        return _idGenInstance.NewId();
    }
}
