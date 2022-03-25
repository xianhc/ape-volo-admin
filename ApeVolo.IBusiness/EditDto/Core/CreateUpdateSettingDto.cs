using ApeVolo.Common.AttributeExt;
using ApeVolo.Entity.Do.Core;

namespace ApeVolo.IBusiness.EditDto.Core;

[AutoMapping(typeof(Setting), typeof(CreateUpdateSettingDto))]
public class CreateUpdateSettingDto : BaseCreateUpdateEntityDto
{
    [ApeVoloRequired(Message = "设置键不能为空！")]
    public string Name { get; set; }

    [ApeVoloRequired(Message = "设置值不能为空！")]
    public string Value { get; set; }

    public bool Enabled { get; set; }

    public string Description { get; set; }
}