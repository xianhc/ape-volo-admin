# 前端开发

## 目录

代码位于 `frontend/src/`。

请求路径与传输类型联查 [接口契约](contract.md)，组件注册等架构约定见 [架构约定](decisions.md)；通用修改范围和验证要求见 [Agent 工作约定](../AGENTS.md)。

| 目录 | 职责 |
| --- | --- |
| `api/` | 接口封装和传输类型 |
| `views/` | 业务页面及页面私有组件 |
| `components/` | 公共 UI 和 CRUD 组件 |
| `composables/` | 通用交互逻辑 |
| `services/` | HTTP、会话、导航、外观、存储与日志 |
| `pinia/` | 全局状态 |
| `router/` | 路由配置 |
| `plugins/` | 应用能力注册 |
| `utils/` | 工具函数 |

## 请求与会话

业务请求链路为 `api/` → [utils/request.ts](../frontend/src/utils/request.ts) → [services/http/client.ts](../frontend/src/services/http/client.ts)。认证接口使用独立的 [api/authClient.ts](../frontend/src/api/authClient.ts)，避免刷新请求再次触发刷新。

- Axios 基础地址为 `VITE_BASE_API`，接口封装不重复添加 `/api`。
- 页面收到 `AxiosResponse`，响应内容在 `response.data`。
- 请求前检查访问凭证；服务端返回 401 时允许刷新并重放一次。
- 同页刷新合并，跨标签页使用可用的 `navigator.locks` 协调。
- 登录和刷新响应的时间戳在接口适配层转为数字毫秒值。
- 刷新超时、429、5xx 保留凭证；刷新凭证失效或返回无效凭证时结束会话。
- 刷新不延长服务端会话的绝对截止时间。
- 局部处理错误的请求使用 `errorFeedback: false`。
- 列表请求传递 `AbortSignal`，新查询和页面卸载取消旧请求；代次检查继续阻止旧响应覆盖当前数据。合法越界页根据真实总数最多补查一次。

刷新合并与凭证失效处理见 [tokenRefresh.ts](../frontend/src/services/session/tokenRefresh.ts)，时间戳适配见 [auth.types.ts](../frontend/src/api/auth.types.ts)。

头像、上传文件使用 `getStaticBaseUrl()`，接口地址使用 `getBaseUrl()`。开发代理分别转发 `/api` 和 `/uploads`，静态文件路径不能添加接口前缀。

## 页面与组件

使用 `<script setup lang="ts">`。公共组件放 `components/`，页面私有组件放业务模块目录。页面优先复用以下组件与组合式函数。

| 能力 | 现有实现 |
| --- | --- |
| CRUD 协调 | [UseCrud.ts](../frontend/src/components/crud/UseCrud.ts) |
| 表格查询、分页、排序 | [useTableController.ts](../frontend/src/composables/useTableController.ts) |
| 表单草稿 | [useEntityEditor.ts](../frontend/src/composables/useEntityEditor.ts) |
| 导出 | [useExportAction.ts](../frontend/src/composables/useExportAction.ts) |
| 单次操作 | [useMutationAction.ts](../frontend/src/composables/useMutationAction.ts) |
| 图标 | [AppIcon.vue](../frontend/src/components/AppIcon.vue) |
| 确认弹窗 | [confirmation.ts](../frontend/src/services/feedback/confirmation.ts) |
| 本地存储 | [services/storage/](../frontend/src/services/storage/) |

按钮权限使用 `v-has-perm`、`v-has-role`。动态路由名称来自后端菜单，缓存页面通过 `defineOptions({ name })` 声明对应组件名。

全量部门接口返回平铺列表，`getDeptTree()` 在前端按 `parentId` 组树，供角色自定义数据权限和用户部门选择器使用。角色自定义范围按部门独立勾选，展示父子层级不代表自动授予下属部门。

用户新增入口和角色选择仅对 `admin` 开放；非管理员编辑已有用户时保留原角色，最终授权由后端校验。内嵌与外链导航仅允许完整的 HTTP/HTTPS 地址。内嵌页面使用 iframe 沙箱，不授予同源权限，不发送 Referer；需要同源存储能力的网站可能无法正常内嵌。

权限指令见 [directive/permission/index.ts](../frontend/src/directive/permission/index.ts)，完整页面调用关系见 [岗位示例](examples/frontend-page-example.md)。

Element Plus 在插件入口全量注册，函数式 API 仍需导入。样式使用 UnoCSS 和 `style/` 中的设计变量。工作台和监控页异步加载图表，并保留占位高度。

## 错误诊断

[诊断插件](../frontend/src/plugins/diagnostics.ts) 收集 Vue、浏览器未处理异常和请求层的 5xx 错误。局部错误关闭弹窗不会关闭诊断。仅在访问令牌有效时发送到本项目后端 `/api/client-errors`，不会为诊断请求刷新令牌。

摘要仅包含固定事件码、错误类别、路由名称、构建标识和后端请求 ID；不发送原始错误消息、堆栈、URL、请求正文或凭证。每分钟最多十条，同类错误每分钟一次；诊断失败不影响业务，也不递归报告。构建标识可通过 `VITE_BUILD_TAG` 指定，未指定时使用构建时间戳。

## 检查

修改前端代码后，在 `frontend/` 执行：

```bash
pnpm type-check
pnpm lint
pnpm build
pnpm test
```

命令定义见 [package.json](../frontend/package.json)。`build` 已包含类型检查，`type-check` 可用于提前定位类型错误。

前端保留 [basic.test.ts](../frontend/tests/basic.test.ts) 中的 2 个基础用例，检查部门树转换和菜单链接安全校验，使用英文名称。页面布局、操作流程和权限显示需在浏览器验证；修改会话链路时，还需验证登录、令牌刷新、退出和多标签页切换账号。
