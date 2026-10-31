import { createRequire } from 'node:module'

/** 版本号唯一来源是 package.json，避免横幅与包版本各写一处后不一致。 */
const { version } = createRequire(import.meta.url)('./package.json') as {
  version: string
}

const getWelcomeMessage = (): string => {
  const frontendPort = process.env.VITE_CLI_PORT || '9001'
  const backendPort = process.env.VITE_SERVER_PORT || '9002'
  const basePath = process.env.VITE_BASE_PATH || 'http://localhost'

  return `
欢迎使用《ape-volo-admin》中后台权限管理系统
当前版本：v${version}
加群方式：微信号：apevolo<备注'加群'>   QQ群：839263566
官方网站：https://www.apevolo.com
接口文档地址：${basePath}:${backendPort}/swagger/api/index.html
前端运行地址：http://localhost:${frontendPort}
--------------------------------------版权声明------------------------------
** 版权所有 © xianhc。保留所有权利。 **
** 移除版权标识需购买商用授权：https://www.apevolo.com/empower/index.html **
`
}

const greenText = (text: string): string => `\x1b[32m${text}\x1b[0m`

export const printWelcomeOnce = (): void => {
  if (!process.env.__APE_WELCOME_PRINTED) {
    setTimeout(() => {
      console.log(greenText(getWelcomeMessage()))
      process.env.__APE_WELCOME_PRINTED = '1'
    }, 100)
  }
}
