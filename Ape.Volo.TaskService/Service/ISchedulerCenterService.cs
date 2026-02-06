using System.Threading.Tasks;
using Ape.Volo.Entity.Core.System;
using Quartz;

namespace Ape.Volo.TaskService.service;

/// <summary>
/// 作业调度接口
/// </summary>
public interface ISchedulerCenterService
{
    /// <summary>
    /// 开启任务
    /// </summary>
    /// <returns></returns>
    Task<bool> StartScheduleAsync();

    /// <summary>
    /// 停止任务
    /// </summary>
    /// <returns></returns>
    Task<bool> ShutdownScheduleAsync();

    /// <summary>
    /// 添加任务
    /// </summary>
    /// <param name="taskQuartz"></param>
    /// <returns></returns>
    Task<bool> AddScheduleJobAsync(QuartzNet taskQuartz);

    /// <summary>
    /// 删除任务
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    Task<bool> DeleteScheduleJobAsync(string name, string group);

    /// <summary>
    /// 暂停任务
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    Task<bool> PauseJob(string name, string group);

    /// <summary>
    /// 恢复任务
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    Task<bool> ResumeJob(string name, string group);

    /// <summary>
    /// 检测任务是否存在
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    Task<bool> IsExistScheduleJobAsync(string name, string group);


    /// <summary>
    /// 获取任务触发器状态
    /// </summary>
    /// <param name="name"></param>
    /// <param name="group"></param>
    /// <returns></returns>
    Task<TriggerState> GetTriggerStatus(string name, string group);
}
