# 前端开发

## 目录与组件

代码位于 `frontend/src/`。`api/` 管理接口与传输类型，`views/` 放页面及私有组件，`components/` 放公共组件，`services/` 管理请求、会话、导航等能力，`pinia/` 管理全局状态。

用户、角色、菜单的请求和页面放在 `identity/`；部门与岗位的请求、类型和页面分别放在 `organization/department/`、`organization/position/`。菜单种子按身份与组织分组。

Vue 使用 `<script setup lang="ts">`。页面优先复用现有实现：

| 能力 | 入口 |
| --- | --- |
| CRUD | [UseCrud.ts](../frontend/src/components/crud/UseCrud.ts) |
| 列表、分页、排序 | [useTableController.ts](../frontend/src/composables/useTableController.ts) |
| 表单草稿 | [useEntityEditor.ts](../frontend/src/composables/useEntityEditor.ts) |
| 导出 | [useExportAction.ts](../frontend/src/composables/useExportAction.ts) |
| 单次操作 | [useMutationAction.ts](../frontend/src/composables/useMutationAction.ts) |
| 图标 | [AppIcon.vue](../frontend/src/components/AppIcon.vue) |
| 确认弹窗 | [confirmation.ts](../frontend/src/services/feedback/confirmation.ts) |

Element Plus 在插件入口全量注册，函数式 API 仍需导入。样式使用 UnoCSS 和 `style/` 的设计变量。

## 请求与会话

业务请求为 `api/` → [utils/request.ts](../frontend/src/utils/request.ts) → [http/client.ts](../frontend/src/services/http/client.ts)。认证使用独立的 [authClient.ts](../frontend/src/api/authClient.ts)。

- 基础地址为 `VITE_BASE_API`，模块路径不重复添加 `/api`；响应内容位于 `response.data`。
- 请求前检查凭证；401 时允许刷新并重放一次。同页刷新合并，跨标签页使用可用的 `navigator.locks` 协调。
- 刷新超时、429、5xx 保留凭证；刷新凭证失效或响应无效时结束会话。刷新不延长服务端会话绝对期限。
- 其他标签页切换会话或退出时清理状态并重挂应用；同一会话令牌轮换不重载。时间戳在认证适配层转为数字毫秒值。
- 列表传递 `AbortSignal`，新查询与卸载取消旧请求，代次检查阻止旧响应覆盖。合法越界页最多补查一次。
- HTTP 错误统一由 `httpFeedback.ts` 反馈，局部处理时设 `errorFeedback: false`；`appLogger.ts` 仅在开发环境输出控制台摘要。

密码操作通过 [rsaEncrypt.ts](../frontend/src/utils/rsaEncrypt.ts) 每次读取站点基础路径下的 `keys/password-public.pem`，禁用请求缓存，加载或导入失败时终止操作。公钥位于 [public/keys/password-public.pem](../frontend/public/keys/password-public.pem)，部署后可直接替换；代理和 CDN 不应缓存该路径。

静态头像、上传文件使用 `getStaticBaseUrl()`，接口使用 `getBaseUrl()`；开发代理分别转发 `/api` 与 `/uploads`。

## 页面与权限

- 按钮使用 `v-has-perm`、`v-has-role`；缓存页面通过 `defineOptions({ name })` 对应后端动态路由名。
- 部门接口返回平铺列表，由 `getDeptTree()` 组树。角色自定义范围独立勾选部门，父子层级不自动扩大授权。
- 部门管理采用与字典页一致的双栏布局：左侧完整部门树支持筛选和维护，右侧按选中部门查询直属用户。用户列表要求 `sys:user:list`，仅提供查询、排序和分页；切换部门重新挂载用户面板，重置筛选分页并取消旧请求。窄屏通过“部门列表 / 部门用户”切换，删除或筛选后选中部门不可见时清空用户面板。
- 用户、角色、菜单的敏感字段按管理员身份限制，最终由后端授权，详见 [接口契约](contract.md#用户与菜单约束)。
- 写入使用 `*WriteRequest` 和对应 Adapter 显式组装字段，不提交审计字段、树节点或其他展示数据。
- 内嵌与外链仅接受完整 HTTP/HTTPS 地址。iframe 保留沙箱、禁用同源权限且不发送 Referer。

完整调用关系见 [岗位示例](examples/frontend-page-example.md)。

## 检查

在 `frontend/` 执行：

```bash
pnpm type-check
pnpm lint
pnpm build
pnpm test
```

命令见 [package.json](../frontend/package.json)，`build` 已包含类型检查。基础测试覆盖树转换、链接与写入安全、会话变化和公钥加载；页面操作与权限显示需浏览器验证，会话改动需验证登录、刷新、退出和跨标签页切换。
