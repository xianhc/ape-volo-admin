namespace Ape.Volo.Application.Contracts.Authorization;

/// <summary>
/// 系统权限点，权限值与 sys_menu.json 及前端按钮权限保持一致。
/// </summary>
public static class SystemPermissions
{
    /// <summary>
    /// 用户管理权限。
    /// </summary>
    public static class Users
    {
        /// <summary>
        /// 用户管理。
        /// </summary>
        public const string List = "sys:user:list";

        /// <summary>
        /// 用户新增。
        /// </summary>
        public const string Add = "sys:user:add";

        /// <summary>
        /// 用户编辑。
        /// </summary>
        public const string Edit = "sys:user:edit";

        /// <summary>
        /// 用户删除。
        /// </summary>
        public const string Delete = "sys:user:del";

        /// <summary>
        /// 用户导出。
        /// </summary>
        public const string Download = "sys:user:download";

    }

    /// <summary>
    /// 角色管理权限。
    /// </summary>
    public static class Roles
    {
        /// <summary>
        /// 角色管理。
        /// </summary>
        public const string List = "sys:role:list";

        /// <summary>
        /// 角色创建。
        /// </summary>
        public const string Add = "sys:role:add";

        /// <summary>
        /// 角色修改。
        /// </summary>
        public const string Edit = "sys:role:edit";

        /// <summary>
        /// 角色删除。
        /// </summary>
        public const string Delete = "sys:role:del";

        /// <summary>
        /// 角色导出。
        /// </summary>
        public const string Download = "sys:role:download";

    }

    /// <summary>
    /// 菜单管理权限。
    /// </summary>
    public static class Menus
    {
        /// <summary>
        /// 菜单管理。
        /// </summary>
        public const string List = "sys:menu:list";

        /// <summary>
        /// 菜单新增。
        /// </summary>
        public const string Add = "sys:menu:add";

        /// <summary>
        /// 菜单编辑。
        /// </summary>
        public const string Edit = "sys:menu:edit";

        /// <summary>
        /// 菜单删除。
        /// </summary>
        public const string Delete = "sys:menu:del";

        /// <summary>
        /// 菜单导出。
        /// </summary>
        public const string Download = "sys:menu:download";

    }

    /// <summary>
    /// 部门管理权限。
    /// </summary>
    public static class Departments
    {
        /// <summary>
        /// 部门管理。
        /// </summary>
        public const string List = "sys:dept:list";

        /// <summary>
        /// 部门新增。
        /// </summary>
        public const string Add = "sys:dept:add";

        /// <summary>
        /// 部门编辑。
        /// </summary>
        public const string Edit = "sys:dept:edit";

        /// <summary>
        /// 部门删除。
        /// </summary>
        public const string Delete = "sys:dept:del";

        /// <summary>
        /// 部门导出。
        /// </summary>
        public const string Download = "sys:dept:download";

    }

    /// <summary>
    /// 岗位管理权限。
    /// </summary>
    public static class Positions
    {
        /// <summary>
        /// 岗位管理。
        /// </summary>
        public const string List = "sys:position:list";

        /// <summary>
        /// 岗位新增。
        /// </summary>
        public const string Add = "sys:position:add";

        /// <summary>
        /// 岗位编辑。
        /// </summary>
        public const string Edit = "sys:position:edit";

        /// <summary>
        /// 岗位删除。
        /// </summary>
        public const string Delete = "sys:position:del";

        /// <summary>
        /// 岗位导出。
        /// </summary>
        public const string Download = "sys:position:download";

    }

    /// <summary>
    /// 字典管理权限。
    /// </summary>
    public static class Dictionaries
    {
        /// <summary>
        /// 字典管理。
        /// </summary>
        public const string List = "sys:dict:list";

        /// <summary>
        /// 字典新增。
        /// </summary>
        public const string Add = "sys:dict:add";

        /// <summary>
        /// 字典编辑。
        /// </summary>
        public const string Edit = "sys:dict:edit";

        /// <summary>
        /// 字典删除。
        /// </summary>
        public const string Delete = "sys:dict:del";

        /// <summary>
        /// 字典导出。
        /// </summary>
        public const string Download = "sys:dict:download";

    }

    /// <summary>
    /// 作业调度权限。
    /// </summary>
    public static class ScheduledJobs
    {
        /// <summary>
        /// 作业调度。
        /// </summary>
        public const string List = "sys:scheduledJob:list";

        /// <summary>
        /// 作业新增。
        /// </summary>
        public const string Add = "sys:scheduledJob:add";

        /// <summary>
        /// 作业编辑。
        /// </summary>
        public const string Edit = "sys:scheduledJob:edit";

        /// <summary>
        /// 作业删除。
        /// </summary>
        public const string Delete = "sys:scheduledJob:del";

        /// <summary>
        /// 作业导出。
        /// </summary>
        public const string Download = "sys:scheduledJob:download";

    }

    /// <summary>
    /// 系统配置权限。
    /// </summary>
    public static class Settings
    {
        /// <summary>
        /// 系统配置。
        /// </summary>
        public const string List = "sys:setting:list";

        /// <summary>
        /// 新增配置。
        /// </summary>
        public const string Add = "sys:setting:add";

        /// <summary>
        /// 编辑配置。
        /// </summary>
        public const string Edit = "sys:setting:edit";

        /// <summary>
        /// 删除配置。
        /// </summary>
        public const string Delete = "sys:setting:del";

        /// <summary>
        /// 导出配置。
        /// </summary>
        public const string Download = "sys:setting:download";

    }

    /// <summary>
    /// 应用密钥权限。
    /// </summary>
    public static class AppSecrets
    {
        /// <summary>
        /// 应用密钥。
        /// </summary>
        public const string List = "sys:appSecret:list";

        /// <summary>
        /// 密钥新增。
        /// </summary>
        public const string Add = "sys:appSecret:add";

        /// <summary>
        /// 密钥编辑。
        /// </summary>
        public const string Edit = "sys:appSecret:edit";

        /// <summary>
        /// 密钥删除。
        /// </summary>
        public const string Delete = "sys:appSecret:del";

        /// <summary>
        /// 密钥导出。
        /// </summary>
        public const string Download = "sys:appSecret:download";

    }

    /// <summary>
    /// 操作日志权限。
    /// </summary>
    public static class OperateLogs
    {
        /// <summary>
        /// 操作日志。
        /// </summary>
        public const string List = "log:operate:list";

    }

    /// <summary>
    /// 异常日志权限。
    /// </summary>
    public static class ExceptionLogs
    {
        /// <summary>
        /// 异常日志。
        /// </summary>
        public const string List = "log:exception:list";

    }

    /// <summary>
    /// 作业日志权限。
    /// </summary>
    public static class ScheduledJobLogs
    {
        /// <summary>
        /// 作业日志。
        /// </summary>
        public const string List = "log:scheduledJob:list";

    }

    /// <summary>
    /// 邮箱账户权限。
    /// </summary>
    public static class EmailAccounts
    {
        /// <summary>
        /// 邮箱账户。
        /// </summary>
        public const string List = "sys:emailAccount:list";

        /// <summary>
        /// 邮箱账户新增。
        /// </summary>
        public const string Add = "sys:emailAccount:add";

        /// <summary>
        /// 邮箱账户编辑。
        /// </summary>
        public const string Edit = "sys:emailAccount:edit";

        /// <summary>
        /// 邮箱账户删除。
        /// </summary>
        public const string Delete = "sys:emailAccount:del";

        /// <summary>
        /// 邮箱账户导出。
        /// </summary>
        public const string Download = "sys:emailAccount:download";

    }

    /// <summary>
    /// 邮件模板权限。
    /// </summary>
    public static class EmailTemplates
    {
        /// <summary>
        /// 邮件模板。
        /// </summary>
        public const string List = "sys:emailTemplate:list";

        /// <summary>
        /// 邮件模板新增。
        /// </summary>
        public const string Add = "sys:emailTemplate:add";

        /// <summary>
        /// 邮件模板编辑。
        /// </summary>
        public const string Edit = "sys:emailTemplate:edit";

        /// <summary>
        /// 邮件模板删除。
        /// </summary>
        public const string Delete = "sys:emailTemplate:del";

    }

    /// <summary>
    /// 邮件队列权限。
    /// </summary>
    public static class QueuedEmails
    {
        /// <summary>
        /// 邮件队列。
        /// </summary>
        public const string List = "sys:queuedEmail:list";

        /// <summary>
        /// 邮件队列添加。
        /// </summary>
        public const string Add = "sys:queuedEmail:add";

        /// <summary>
        /// 邮件队列编辑。
        /// </summary>
        public const string Edit = "sys:queuedEmail:edit";

        /// <summary>
        /// 邮件队列删除。
        /// </summary>
        public const string Delete = "sys:queuedEmail:del";

    }

    /// <summary>
    /// 文件管理权限。
    /// </summary>
    public static class Files
    {
        /// <summary>
        /// 文件管理。
        /// </summary>
        public const string List = "sys:storage:list";

        /// <summary>
        /// 文件新增。
        /// </summary>
        public const string Add = "sys:storage:add";

        /// <summary>
        /// 文件编辑。
        /// </summary>
        public const string Edit = "sys:storage:edit";

        /// <summary>
        /// 文件删除。
        /// </summary>
        public const string Delete = "sys:storage:del";

        /// <summary>
        /// 文件导出。
        /// </summary>
        public const string Download = "sys:storage:download";

    }

    /// <summary>
    /// 在线用户权限。
    /// </summary>
    public static class OnlineUsers
    {
        /// <summary>
        /// 在线用户。
        /// </summary>
        public const string List = "sys:online:list";

    }

    /// <summary>
    /// 服务监控权限。
    /// </summary>
    public static class Server
    {
        /// <summary>
        /// 服务监控。
        /// </summary>
        public const string List = "sys:server:list";

    }

}
