# 开发文档

项目介绍、环境要求和启动步骤见根目录 [README.md](../README.md)，AI 助手工作约定见 [AGENTS.md](../AGENTS.md)。本目录面向所有贡献者，说明当前实现与开发方式。

通用修改范围与验证要求统一维护在 `AGENTS.md`；各专题文档说明具体技术约定和检查命令，`decisions.md` 说明架构约束，`examples/` 串联已有模块。发现文档与实现不一致时，先核对代码和配置，再同步修正文档。

## 编辑器工作区

使用 VS Code 打开根目录的 [ape-volo-admin-basic.code-workspace](../ape-volo-admin-basic.code-workspace)，资源管理器按三个分组显示：

- `前端`：`frontend/`。
- `后端`：`backend/`。
- `工程`：仓库根目录，包含 `docs/`、`AGENTS.md`、`README.md` 和其他项目配置。

工作区文件中的 `files.exclude` 隐藏“工程”分组下重复的 `frontend/` 和 `backend/` 目录；两个独立的前后端分组仍可正常展开。显示规则随工作区文件共享，无需单独提交 `.vscode/settings.json`。修改工作区后如未自动刷新，重新打开该工作区文件。

工作区的 `search.exclude` 排除所有层级的 `bin/`、`obj/` 和 `dist/` 目录，默认搜索跳过这些生成目录。

## 文档索引

| 文档 | 内容 |
| --- | --- |
| [backend.md](backend.md) | 后端分层、数据访问、事务与配置 |
| [frontend.md](frontend.md) | 前端请求、会话、组件与状态 |
| [contract.md](contract.md) | 动态路由、参数和响应格式 |
| [decisions.md](decisions.md) | 架构约定 |
| [examples/](examples/README.md) | 岗位模块的现有文件与调用关系 |

## 按任务查阅

| 任务 | 阅读顺序与核对重点 |
| --- | --- |
| 新增业务模块 | [后端开发](backend.md) → [接口契约](contract.md) → [前端开发](frontend.md) → [岗位示例](examples/README.md)，按现有模块核对契约、实现、映射、页面与权限 |
| 修改接口或字段 | [接口契约](contract.md)，同步检查后端契约、DTO、实现、映射与前端请求类型 |
| 调整权限或数据范围 | [后端开发](backend.md) 与 [架构约定](decisions.md)，核对服务权限声明、数据过滤、权限缓存和页面权限标识 |
| 修改登录、刷新或请求处理 | [前端开发](frontend.md) 的请求与会话部分，联查 [接口契约](contract.md) 并验证登录、刷新、退出和多标签页账号切换 |
| 调整页面布局与交互 | [前端开发](frontend.md) 与 [前端示例](examples/frontend-page-example.md)，复用已有组件并验证受影响页面 |
| 修改事务、缓存或表结构 | [后端开发](backend.md) 与 [架构约定](decisions.md)，核对事务边界、提交后动作和初始化开关 |

修改接口、配置或目录职责时，同步更新对应文档；示例中的文件链接也应随代码移动更新。

通用规则见 [修改范围](../AGENTS.md#修改范围) 和 [验证要求](../AGENTS.md#验证)；具体命令见 [后端检查](backend.md#检查) 与 [前端检查](frontend.md#检查)。仅修改文档时核对事实、命令、源码入口和文件链接，无需构建测试。
