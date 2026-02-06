using System;

namespace Ape.Volo.Common.Attributes;

[AttributeUsage(AttributeTargets.Method)]
public class HasPermAttribute : Attribute
{
    public HasPermAttribute(string[] authCodes)
    {
        AuthCodes = authCodes;
    }

    /// <summary>
    /// 权限代码
    /// </summary>
    public string[] AuthCodes { get; }
}