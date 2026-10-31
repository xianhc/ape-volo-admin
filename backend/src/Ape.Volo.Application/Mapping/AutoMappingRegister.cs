using System.Reflection;
using Ape.Volo.Common.Attributes;
using Mapster;

namespace Ape.Volo.Application.Mapping;

/// <summary>
/// 对象映射
/// </summary>
public class AutoMappingRegister : IRegister
{
    /// <summary>
    /// 注册
    /// </summary>
    /// <param name="config"></param>
    public void Register(TypeAdapterConfig config)
    {
        // AutoMapping 声明在应用层程序集上，契约层不引用实体
        var maps = typeof(AutoMappingRegister).Assembly
            .GetCustomAttributes<AutoMappingAttribute>()
            .Select(x => x.ToEntity
                //Dto：模型 -> 实体
                ? (Source: x.TargetType, Target: x.SourceType)
                //Vo：实体 -> 模型
                : (Source: x.SourceType, Target: x.TargetType))
            .ToList();

        if (maps == null)
        {
            return;
        }

        //根据AutoMappingAttribute特性自动映射
        maps.ForEach(aMap => config.NewConfig(aMap.Source, aMap.Target));
    }
}
