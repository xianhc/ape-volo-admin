using Ape.Volo.Common.Attributes;
using Ape.Volo.Entity.Core.System;

namespace Ape.Volo.ViewModel.Core.System;

/// <summary>
/// 任务调度Vo
/// </summary>
[AutoMapping(typeof(QuartzNet), typeof(QuartzNetSmallVo))]
public class QuartzNetSmallVo
{
    /// <summary>
    /// 任务ID
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// 任务名称
    /// </summary>
    public string TaskName { get; set; }
}
