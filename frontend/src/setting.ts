/**
 * 网站配置文件
 */

/**
 * 网站配置接口
 */
export interface AppConfig {
  /**
   * 网站标题
   */
  title: string
  /**
   * 网站名称
   */
  appName: string
  /**
   * 网站Logo
   */
  appLogo: string
  /**
   * 默认路由
   */
  defaultRouter: string
  /**
   * 请求超时时间，毫秒（默认2分钟）
   */
  timeout: number
  /**
   * 版权所有
   */
  copyrightWwner: string
  /**
   * 项目地址
   */
  githubUrl: string
  /**
   * 在线文档地址
   */
  docsUrl: string
  /**
   * 项目账户
   */
  githubAccount: string
}

const config: AppConfig = {
  title: 'Ape-Volo-Admin',
  appName: 'Ape-Volo-Admin',
  appLogo: 'logo.png',
  defaultRouter: 'dashboard',
  timeout: 1200000,
  copyrightWwner: 'xianhc',
  githubUrl: 'https://github.com/xianhc/ape-volo-admin',
  githubAccount: 'https://github.com/xianhc',
  docsUrl: 'https://www.apevolo.com/'
}

export default config
