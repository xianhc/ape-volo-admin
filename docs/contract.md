# 接口契约

## 动态路由

继承 `IRemoteService` 的契约接口参与动态端点生成。`IApplicationService` 继承该接口。

路由格式为 `{前缀}/{模块}/{动作}`，前缀由 `DynamicApi:RoutePrefix` 配置，当前默认 `api`。接口名去掉开头的 `I` 和结尾的 `Service`，方法名去掉结尾的 `Async`，名称转为小写短横线格式。`Manager` 后缀保留。

HTTP 动词按方法名前缀确定：

| 方法名前缀 | 动词 |
| --- | --- |
| `GetList`、`GetAll`、`Get` | GET |
| `Put`、`Update` | PUT |
| `Delete`、`Remove` | DELETE |
| `Create`、`Add`、`Insert`、`Post` | POST |
| `Patch` | PATCH |
| 其他前缀，包括 `Find`、`Query`、`Download` | POST |

现有岗位接口（完整调用关系见 [岗位示例](examples/README.md)）：

```text
GET    /api/position/get-list
GET    /api/position/get-all
POST   /api/position/create
PUT    /api/position/update
DELETE /api/position/delete
POST   /api/position/download
```

接口或方法上的 `RemoteService(false)` 禁止自动暴露。登录、刷新和 multipart 上传使用显式控制器。

源码入口：[端点扫描与命名](../backend/src/Ape.Volo.Web/DynamicApi/DynamicApiRegistry.cs)、[MVC 端点与默认认证](../backend/src/Ape.Volo.Web/DynamicApi/DynamicApiApplicationModelProvider.cs)。动态端点默认校验登录，具体权限由服务授权检查。

## 请求

- 前端 Axios 基础地址由 `VITE_BASE_API` 配置，当前默认 `/api`；模块封装只写 `/position/get-list` 等相对接口路径。调整后端路由前缀时，同步核对该配置和代理规则。
- 分页字段为 `pageIndex`、`pageSize`、`sorting`。
- 通用数据库分页要求 `pageIndex >= 1`、`1 <= pageSize <= 100`。合法页码超出总页数时返回空 `items` 和真实 `totalCount`；前端最多补查一次最后一页，空表回到第 1 页。
- 可空查询字段以 `null` 表示不添加该条件；根节点筛选使用 `IsRoot`。
- 正文长整型集合支持裸数组，也支持按参数名包装的对象；删除接口的参数名为 `ids`。
- DTO 校验由入口模型验证处理；取消令牌来自当前请求。

源码入口：[正文集合绑定](../backend/src/Ape.Volo.Web/DynamicApi/DynamicApiBodyCollectionModelBinder.cs)、[DTO 验证](../backend/src/Ape.Volo.Web/DynamicApi/DynamicApiValidationFilter.cs)。

### 用户与菜单约束

- 新增用户除 `Users.Add` 权限外还要求 `admin` 角色；普通编辑仍要求 `Users.Edit`，其中角色集合发生变化时额外要求 `admin`，与单独分配角色入口一致。
- 内嵌与外链菜单的 `component` 必须是完整的 `http://` 或 `https://` 地址；校验实际目标字段，不以 `path` 代替。
- 创建请求中的 `creatorId`、`creationTime` 不用于设置当前请求的创建审计信息。

## 响应

| 场景 | 格式 |
| --- | --- |
| 分页 | `{ items, totalCount }` |
| 操作成功 | `{ isSuccess: true, message }` |
| 业务操作失败 | HTTP 400，包含 `status`、`message`、`actionError`、`timestamp`、`path` |
| 参数校验失败 | HTTP 400，校验信息位于 `actionError` |
| 未认证或会话失效 | HTTP 401 |
| 权限不足 | HTTP 403 |

动态接口的 `OperateResult.IsSuccess = false` 由结果过滤器转为 400，不按 HTTP 200 处理。导出请求接收文件流。

源码入口：[业务失败响应](../backend/src/Ape.Volo.Web/DynamicApi/DynamicApiOperateResultFilter.cs)、[导出响应](../backend/src/Ape.Volo.Web/DynamicApi/DynamicApiExportFilter.cs)。

## 客户端诊断

响应头 `X-Request-ID` 用于关联后端请求日志，跨域策略允许前端读取该头。

`POST /api/client-errors` 要求登录，接收 `code`、`errorName`、`routeName`、`build`、`requestId`，成功返回 204；请求体上限 4 KiB，字段有长度和字符集校验。该入口写入现有日志通道，不接收异常正文、堆栈、URL、请求正文或凭证。

## 字段类型

- 后端 `long` 序列化为字符串，包含主键和凭证过期时间戳。
- 前端主键保留为字符串，凭证时间戳在认证适配层转为数字。
- `totalCount` 为 `int`。
- 日期时间使用 `yyyy-MM-dd HH:mm:ss`；凭证过期时刻为 Unix 毫秒时间戳。

## 修改接口

同步检查契约、DTO、服务实现、映射和前端 `api/` 类型。权限码使用 `SystemPermissions`，页面权限指令与菜单权限标识保持一致。

具体实现约定见 [后端开发](backend.md) 与 [前端开发](frontend.md)，完整调用关系见 [岗位示例](examples/README.md)。按 [验证要求](../AGENTS.md#验证) 执行受影响范围的 [后端检查](backend.md#检查) 和 [前端检查](frontend.md#检查)，并实际核对请求路径、参数、响应及权限。
