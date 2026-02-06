namespace Ape.Volo.Common.Global;

public static class GlobalConstants
{
    /// <summary>
    /// 缓存Key常量
    /// </summary>
    public static class CachePrefix
    {
        /// <summary>
        /// 在线
        /// </summary>
        public const string OnlineKey = "online:";

        /// <summary>
        /// 验证码
        /// </summary>
        public const string CaptchaId = "login:captcha:";

        /// <summary>
        /// 登录失败阈值
        /// </summary>
        public const string Threshold = "login:threshold:";

        /// <summary>
        /// 登录失败次数
        /// </summary>
        public const string Attempts = "login:attempts:";

        /// <summary>
        /// 邮箱验证码
        /// </summary>
        public const string EmailCaptcha = "email:captcha:";

        /// <summary>
        /// 用户信息
        /// </summary>
        public const string UserInfoById = "user:info:id:";

        /// <summary>
        /// 用户菜单
        /// </summary>
        public const string UserMenuById = "user:menu:id:";

        /// <summary>
        /// 用户权限标识
        /// </summary>
        public const string UserAuthCodes = "user:authCode:id:";

        /// <summary>
        /// 用户权限Url
        /// </summary>
        public const string UserAuthUrls = "user:authUrl:id:";

        /// <summary>
        /// 加载菜单根据PID
        /// </summary>
        public const string LoadMenusByPId = "menu:pid:";

        /// <summary>
        /// 加载菜单根据ID
        /// </summary>
        public const string LoadMenusById = "menu:id:";

        /// <summary>
        /// 全部菜单
        /// </summary>
        public const string LoadAllMenu = "menu:LoadAllMenu";

        /// <summary>
        /// 加载设置信息
        /// </summary>
        public const string LoadSettingByName = "setting:name:";

        /// <summary>
        /// 用户数据权限
        /// </summary>
        public const string UserDataScopeById = "user:dataScope:id:";


        /// <summary>
        /// 加载字典详情信息
        /// </summary>
        public const string LoadDictDetailByDictId = "dict:detail:dictid:";

        /// <summary>
        /// 加载字典详情信息
        /// </summary>
        public const string LoadDictByName = "dict:name:";
    }
}
