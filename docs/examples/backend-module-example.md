# 岗位模块后端

文件路径相对于 `backend/src/`。

| 职责 | 文件 |
| --- | --- |
| 实体 | [Position.cs](../../backend/src/Ape.Volo.Entity/Organization/Position.cs) |
| 用户岗位关联 | [UserPosition.cs](../../backend/src/Ape.Volo.Entity/Organization/UserPosition.cs) |
| 服务契约 | [IPositionService.cs](../../backend/src/Ape.Volo.Application.Contracts/Organization/IPositionService.cs) |
| 创建与编辑参数 | [CreateUpdatePositionDto.cs](../../backend/src/Ape.Volo.Application.Contracts/Organization/Models/CreateUpdatePositionDto.cs) |
| 查询参数 | [PositionQueryCriteria.cs](../../backend/src/Ape.Volo.Application.Contracts/Organization/Models/PositionQueryCriteria.cs) |
| 返回模型 | [PositionVo.cs](../../backend/src/Ape.Volo.Application.Contracts/Organization/Models/PositionVo.cs) |
| 导出模型 | [PositionExport.cs](../../backend/src/Ape.Volo.Application.Contracts/Organization/Models/PositionExport.cs) |
| 服务实现 | [PositionService.cs](../../backend/src/Ape.Volo.Application/Organization/PositionService.cs) |
| 映射声明 | [OrganizationMapping.cs](../../backend/src/Ape.Volo.Application/Mapping/Organization/OrganizationMapping.cs) |
| 权限码 | [SystemPermissions.cs](../../backend/src/Ape.Volo.Application.Contracts/Authorization/SystemPermissions.cs) 中的 `Positions` |

## 字段与服务行为

`Position` 继承 `DataScopedEntity`，参与软删除与数据范围过滤。创建与编辑 DTO 包含 `id`、`name`、`sort` 和 `enabled`；名称必填，排序范围为 1–999。查询参数按名称模糊匹配、按启用状态筛选，并继承分页、排序与日期范围字段。

`PositionService` 继承 `BaseService<Position>`，通过构造函数取得仓储和映射器：

- 新增检查名称重复；编辑先检查记录存在，名称变化时再检查重复。
- 删除先读取当前查询范围内的岗位；没有匹配记录时返回业务错误，存在 `UserPosition` 关联时拒绝删除，否则执行逻辑删除。
- 分页使用 `PageAsync<PositionVo>`；导出按查询条件读取数据并转换为 `PositionExport`。
- `GetAllAsync` 只返回启用岗位，按 `Sort` 升序排列，供用户管理等场景选择岗位；返回类型为 `List<PositionVo>`。

## 接口与权限

接口由 `IPositionService` 按 [动态路由约定](../contract.md#动态路由) 生成，以下路径采用默认 `/api` 前缀。

| 动词 | 路径 | 用途 | 实现方法权限 |
| --- | --- | --- | --- |
| GET | `/api/position/get-list` | 分页 | `Positions.List` |
| POST | `/api/position/create` | 新增 | `Positions.Add` |
| PUT | `/api/position/update` | 编辑及启用状态更新 | `Positions.Edit` |
| DELETE | `/api/position/delete` | 删除 | `Positions.Delete` |
| POST | `/api/position/download` | 导出 | `Positions.Download` |
| GET | `/api/position/get-all` | 启用岗位选项 | `Positions.List` 或 `Users.List`、`Users.Add`、`Users.Edit` 中任一权限 |

表中权限常量均来自 `SystemPermissions`。岗位管理权限值使用 `sys:position:*`，需与前端按钮和菜单权限配置一致；前端列表中的状态切换也调用编辑接口。

## 修改时联查

- 新增或调整字段：依次核对实体、创建与编辑 DTO、查询参数、返回模型和映射，再同步 [前端类型与表单](frontend-page-example.md)。实体变动涉及表结构时，还需按 [初始化约定](../backend.md#初始化与配置) 检查数据库结果。
- 调整增删改查：核对实现方法权限与 `SystemPermissions.Positions`，检查名称重复、排序校验、关联删除限制和分页行为。
- 修改启用状态或岗位选项：同时核对 `GetAllAsync` 的筛选、排序和权限，以及 [用户管理页面](../../frontend/src/views/permission/user/index.vue) 对 `getAll` 的调用。
- 修改用户岗位关系：联查 `UserPosition` 和 [UserService](../../backend/src/Ape.Volo.Application/Permission/Users/UserService.cs)，避免只更新岗位页面而遗漏用户分配入口。

后端检查命令见 [后端开发](../backend.md#检查)；接口改动还需实际核对上述动词、路径与响应。
