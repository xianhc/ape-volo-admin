# 开发文档

环境与启动见 [README.md](../README.md)，修改范围和验证要求见 [AGENTS.md](../AGENTS.md)。按任务查阅对应文档，并以代码和配置核对事实。

| 文档 | 内容 |
| --- | --- |
| [backend.md](backend.md) | 后端分层、授权、数据、事务和配置 |
| [contract.md](contract.md) | 路由、请求、响应和字段类型 |
| [frontend.md](frontend.md) | 前端请求、会话、页面和组件 |
| [decisions.md](decisions.md) | 架构约束及原因 |
| [examples/](examples/README.md) | 岗位模块的现有调用关系 |

接口变更联查前后端契约；权限变更联查数据过滤、缓存和页面权限；会话变更联查登录、刷新、退出和跨标签页切换。

VS Code 使用 [ape-volo-admin-basic.code-workspace](../ape-volo-admin-basic.code-workspace)，包含前端、后端和工程分组；搜索默认排除生成目录。
