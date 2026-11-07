/******************************************************************************
 * 项目名称：APE-VOLO-ADMIN
 * 开源地址：https://github.com/xianhc/ape-volo-admin
 * 系统官网：https://www.apevolo.com
 * 联系邮箱：apevolo@gmail.com
 *
 * Copyright © 2022 xianhc（冼浩春）。保留所有权利。
 * SPDX-License-Identifier: Apache-2.0
 *
 * 本项目采用 Apache License 2.0，完整许可条款详见根目录 LICENSE 文件。
 *
 * 版权与署名保留要求（Apache License 2.0 第 4 条）：
 * 再分发本项目或其衍生作品时，须向接收者提供许可证副本。
 * 以源码形式分发衍生作品时，须保留与该作品相关的原始版权及署名通知，
 * 不得删除原始通知或以他人署名替换；修改过的文件须显著注明已作修改。
 * 上述说明为许可证要求的提示，完整义务以 LICENSE 条款为准。
 *
 * 作者保留依法维护自身合法权益的权利。授权与合作事宜请通过上述邮箱联系。
 ******************************************************************************/

import { defineConfig, loadEnv, ConfigEnv, UserConfig } from 'vite'
import bannerPlugin from 'vite-plugin-banner'
import * as path from 'node:path'
import { fileURLToPath } from 'node:url'
import vuePlugin from '@vitejs/plugin-vue'
import vueDevTools from 'vite-plugin-vue-devtools'
import iconAssetsPlugin from './iconAssets'
import vueFilePathPlugin from './componentNameMap'
import { printWelcomeOnce } from './welcome'
const __dirname = path.dirname(fileURLToPath(import.meta.url))

const toNumber = (value: string, fallback: number): number => {
  const numberValue = Number(value)
  return Number.isFinite(numberValue) ? numberValue : fallback
}

export default defineConfig(
  async ({ mode }: ConfigEnv): Promise<UserConfig> => {
    const { default: UnoCSS } = await import('@unocss/vite')
    const env = loadEnv(mode || 'development', process.cwd(), '')

    // 把 .env.* 写入 process.env，便于插件/脚本复用
    for (const [key, value] of Object.entries(env)) {
      process.env[key] = value
    }

    const timestamp = Date.now()

    const alias = {
      '@': path.resolve(__dirname, 'src'),
      vue$: 'vue/dist/vue.runtime.esm-bundler.js'
    }

    const rollupOptions = {
      output: {
        entryFileNames: 'assets/APEVOLO_7F3C9A21-[name]-[hash].js',
        chunkFileNames: 'assets/APEVOLO_7F3C9A21-[name]-[hash].js',
        assetFileNames: 'assets/APEVOLO_7F3C9A21-[name]-[hash].[ext]',
        // 按模块路径分组，而不是把包名当 chunk 入口：
        // 数组写法（['element-plus']）会强制引入整包，令按需引入失效。
        manualChunks(id: string) {
          if (!id.includes('node_modules')) return
          if (
            /[\\/]node_modules[\\/](element-plus|@element-plus)[\\/]/.test(id)
          )
            return 'element-plus'
          if (/[\\/]node_modules[\\/](vue|vue-router|pinia|@vue)[\\/]/.test(id))
            return 'vue'
        }
      }
    }

    const baseApi = env.VITE_BASE_API || '/api'
    const cliPort = toNumber(env.VITE_CLI_PORT, 9001)
    const serverPort = env.VITE_SERVER_PORT
    const basePath = env.VITE_BASE_PATH
    const proxyTarget = serverPort
      ? `${basePath}:${serverPort}/`
      : `${basePath}/`

    const config: UserConfig = {
      base: '/', // 编译后js导入的资源路径
      root: './', // index.html文件所在位置
      publicDir: 'public', // 静态资源文件夹
      resolve: {
        alias
      },
      css: {
        preprocessorOptions: {
          scss: {
            api: 'modern-compiler' // or "modern"
          }
        }
      },
      server: {
        open: true,
        port: cliPort,
        proxy: {
          // 把key的路径代理到target位置
          // 动态接口的后端路由本身就带 /api 前缀，这里不再重写路径
          [baseApi]: {
            // 需要代理的路径   例如 '/api'
            target: proxyTarget,
            changeOrigin: true
          },
          // 后端静态文件（头像、上传附件）由 wwwroot 以根路径提供，不带 /api 前缀
          '/uploads': {
            target: proxyTarget,
            changeOrigin: true
          }
        }
      },
      build: {
        minify: 'terser', // 是否进行压缩,boolean | 'terser' | 'esbuild',默认使用terser
        manifest: false, // 是否产出manifest.json
        sourcemap: false, // 是否产出sourcemap.json
        outDir: 'dist', // 产出目录
        terserOptions: {
          compress: {
            //生产环境时移除console
            drop_console: true,
            drop_debugger: true
          }
        },
        rollupOptions
      },
      plugins: [
        env.VITE_POSITION === 'open' &&
          vueDevTools({ launchEditor: env.VITE_EDITOR }),
        vuePlugin(),
        iconAssetsPlugin(),
        bannerPlugin(`\n ape-volo-admin build \n Time : ${timestamp}`),
        vueFilePathPlugin(),
        {
          name: 'ape-volo-welcome-message',
          configureServer(server: any) {
            server.httpServer?.once('listening', printWelcomeOnce)
          }
        },
        UnoCSS()
      ].filter(Boolean)
    }
    return config
  }
)
