# 后端开发

## 分层

代码位于 `backend/src/`，项目均使用 `Ape.Volo.` 前缀。

| 项目 | 职责与直接依赖 |
| --- | --- |
| Web | HTTP、认证、上传、调度和日志宿主；依赖 Application、Contracts、Core、Common、Entity、Shared |
| Application | 业务服务与映射；依赖 Contracts、Core、Common、Entity |
| Core | 数据访问、事务、缓存等基础设施；依赖 Contracts、Common、Entity |
| Contracts、Common、Entity | 分别承载服务契约、通用工具、实体；依赖 Shared |
| Shared | 跨层常量和基础类型，无项目依赖 |

`Contracts` 指 `Application.Contracts`。Application 不使用 `HttpContext`、`IFormFile` 或 `ActionResult`，上传使用 `UploadFile` 契约，由 Web 适配。新增模块参考 [岗位示例](examples/backend-module-example.md)。

`Identity` 管理用户、角色、认证、会话和权限；`Organization` 在契约、实体、服务和映射各层统一使用 `Departments/`、`Positions/` 子目录。

## 契约与授权

- 对外接口继承 `IApplicationService`，与 DTO 一起放在 Contracts；契约不引用实体。实现和映射放在 Application。
- 创建、修改 DTO 使用带 `init` 属性的 `sealed record`，只声明业务字段及所需的 `Id`，不继承审计基类。
- 普通校验失败返回 `OperateResult.Error(...)`。常见错误使用 `BaseService` 的 `NotFound`、`AlreadyExists`、`Required`；字段说明来自实体或 DTO 的 XML 注释，缺失时回退到代码名称。发布时保留 Entity、Contracts 的 XML 文档。
- 传递 `CancellationToken`；提交后的必要清理不随客户端断开取消。
- 契约、DTO 和控制器保留必要 XML 注释，实现成员使用 `inheritdoc`。

`HasPerm`、`HasRole` 写在实现方法上，权限码使用 `SystemPermissions`。通过注入的接口调用服务，切面顺序为授权、事务、缓存；授权始终启用，后两项分别由 `Aop:Transactions`、`Aop:Cache` 控制。角色和权限满足任意一项即通过，额外的管理员要求在方法内调用 `IApplicationAuthorizationService.CheckAsync`。

动态端点默认要求登录，未声明具体权限不代表匿名访问。路由、请求与敏感字段授权见 [接口契约](contract.md)。

## 数据访问

- 主键为 `long`，通过注入的单例 `IIdGenerator.NewId()` 生成。
- `AuditedEntity` 含审计和软删除字段；`DataScopedEntity` 额外参与数据范围过滤。请求新增时，创建人和创建时间由服务端覆盖。
- 普通查询和写入保留软删除、数据范围过滤，空范围不授予访问权。内部跨范围读取使用 `ISugarQueryProvider.QueryWithoutDataScopeAsync`，仍保留软删除过滤。
- 分页使用 `PageAsync` 和 `PagedResultDto<T>`。`QueryOptions` 包含分页、条件、导航白名单和分表配置；不使用 `NoLock` 或 ORM 查询缓存。
- 导航通过 `IncludePropertyNameList` 显式加载；未指定时不加载。树顶级 `ParentId` 为 `null`，根查询使用 `IsRoot`。

入口：[BaseService](../backend/src/Ape.Volo.Application/BaseService.cs)、[全局过滤器](../backend/src/Ape.Volo.Web/Extensions/SqlSugarExtensions.cs)、[跨范围查询](../backend/src/Ape.Volo.Core/Repository/SugarHandler/SugarQueryProvider.cs)。

## 事务与缓存

显式事务在调用方同步开启，原因见 [架构约定](decisions.md#同步开启事务)：

```csharp
await using var transaction = unitOfWork.BeginTransaction(Propagation.Required);
// 数据写入
await transaction.CommitAsync(ct);
```

`AfterCommitAsync` 登记提交后动作：无事务时立即执行，有事务时等待根事务提交，回滚丢弃对应动作。动作失败不回滚已提交的数据，也不持久化重试。`UseTran` 仅用于返回 `Task` 或 `Task<T>` 的方法。

- `UseCache.Expiration` 单位为分钟，缓存键不包含取消令牌。ID、名称等简单值直接拼接，仅 AOP 的复杂查询参数序列化后计算 MD5。依赖当前用户的结果必须使用用户缓存键；事务内直接查询，不读写共享结果。
- 缓存键直接按 `auth:`、`identity:`、`setting:`、`dict:` 等业务前缀分组，不添加公共目录前缀。
- 权限缓存统一由 `PermissionCacheService` 管理，键使用 `identity:` 前缀，不添加版本号。用户、角色、菜单变更提交后失效关联缓存；组织成员与部门树变更同时推进数据范围修订。会话撤销也在提交后执行。
- 菜单管理查询直接读取数据库并保留数据范围，用户路由菜单按用户缓存。
- 字典变更失效名称与明细缓存；设置改名失效新旧名称缓存。内部 `GetSettingValue<T>` 跨数据范围读取全局参数，保留软删除过滤。
- 缓存契约为 `ICache`，可选键枚举能力为 `ICacheKeyCatalog`，目前仅 Redis 支持；在线用户枚举要求 Redis。

入口：[工作单元](../backend/src/Ape.Volo.Core/Repository/UnitOfWork/UnitOfWork.cs)、[权限缓存](../backend/src/Ape.Volo.Application/Identity/PermissionCacheService.cs)。

内置设置名称统一使用 [SettingNames](../backend/src/Ape.Volo.Shared/Settings/SettingNames.cs)。已有环境升级需保留原值并改名，种子不会更新已有记录：

| 旧名称 | 新名称 |
| --- | --- |
| `DefaultEmailAccountId` | `App.Email.DefaultAccountId` |
| `IsExceptionLogSaveDB` | `App.Logging.ExceptionLog.SaveToDatabase` |
| `IsOperateLogSaveDB` | `App.Logging.OperateLog.SaveToDatabase` |

## 初始化与配置

配置按 `appsettings.json` → 环境文件 → `ipratelimit.json` → 环境变量 → 命令行覆盖，当前不加载 User Secrets。入口为 [Program.cs](../backend/src/Ape.Volo.Web/Program.cs) 与 [环境配置](../backend/src/Ape.Volo.Web/appsettings.Development.json)。

VS Code 的 [.vscode/settings.json](../.vscode/settings.json) 和[工作区](../ape-volo-admin-basic.code-workspace)均为 `appsettings*.json` 内联配置通用对象 Schema，直接打开仓库根目录或使用工作区时均生效，覆盖 C# 扩展的公共 Schema 关联，避免在线加载失败及自定义 `SerilogOptions` 字段误报。JSON 语法检查保留，不提供字段级类型校验。

- SQLite 路径相对于 Web 内容根目录，默认主库、日志库在 `data/`，不对外公开。旧根目录数据库不会自动迁移；保留数据时停机移动数据库及其 `-wal`、`-shm` 文件。
- `System:IsInitDb` 控制启动初始化，先日志库后主库；主库同步表结构并写入种子，禁止自动删列。已有日志表受 `IsDisabledUpdateAll` 限制。
- Oracle 需预先准备服务/PDB、Schema 和建表权限，程序只检查连接并初始化表结构。
- 种子位于 `resources/db/`，IP 离线库位于 `resources/ip/ip2region_v4.xdb`、`ip2region_v6.xdb`，随发布复制。归属地通过 `IIpLocationService` 查询，更新离线库后重启。
- `System:UseRedisCache` 选择 Redis 或进程内缓存；后者不跨进程共享。
- `SnowflakeId:WorkerId` 默认 1（0–63），`DataCenterId` 默认 0（0–31）。部署节点组合必须唯一；旧 `IdGenerator:WorkerId` 不再读取。新 ID 已加入数据中心位，时间戳左移 17 位，依赖旧位布局的解析代码需调整。
- Swagger 联系人使用 `Swagger:ContactName`、`ContactEmail`、`ContactUrl`。
- MiniProfiler 仅在 Development 且 `Middleware:MiniProfiler` 开启时挂载，结果仅已登录的 `admin` 可访问。

### PostgreSQL

两份环境配置均提供默认关闭的 `Ape.Volo.Admin.PostgreSQL` 连接（`DBType: 4`）。填写连接字符串、启用连接，并将 `System:MainDatabase` 设为该 ID。首次自动建库需 `CREATEDB` 权限，也可预建数据库。日志库通过 `LogDataConnection` 独立配置，可与其他数据库类型组合。

PostgreSQL 查询及 CodeFirst 使用小写命名，大文本使用 `CodeFirst_BigString`。已有环境如需数据库类型字典项，通过字典管理补充 `PostgreSQL / 4`。

## IP 限流

规则见 [IpRateLimit.json](../backend/src/Ape.Volo.Web/IpRateLimit.json)，由 `Middleware:IpLimit` 启用，修改后重启。按连接 IP、动词和实际路径计数，查询参数不拆分额度，超限返回 429 和 `Retry-After`。同一出口 IP 共享额度，部署时按并发量调整。

转发头默认仅信任回环代理，其他代理通过 `ForwardedHeaders:KnownProxies` 配置，仅处理一层转发。修改 API 前缀时同步修改限流路径。

## 上传与日志

上传由 [FileUploadPolicy](../backend/src/Ape.Volo.Application/System/FileRecords/FileUploadPolicy.cs) 判断允许规则；Web 的 `FileTypeResolver` 按扩展名解析 MIME，应用层 `FileCategory` 决定显示分类和目录。未知类型为 `application/octet-stream`，不信任客户端 MIME，也不验证实际内容。文件由 `/uploads/file/` 提供，不带 `/api`。

异常、操作和作业日志由 Web 构建实体，经独立内存队列写入日志库月分表；Application 日志服务仅查询、统计或导出。队列满、写入失败或进程退出可能丢日志。日志失败不改变作业执行结果。

文件日志由 `Serilog:ToFile` 控制，按本地日期写入 `logs/yyyy-MM-dd/`，各级别独立文件，SQL 为 `AopSql.log`，每类最多保留 31 个日期文件。兜底为 `Fallback.log`，清理 30 天前同类文件；旧目录不自动迁移。

Elasticsearch 输出需同时启用 `Serilog:ToElasticsearch` 与 `Middleware:Elasticsearch`，使用 `Elastic.Serilog.Sinks` 9，要求 Elasticsearch 8.15+。地址由 `Serilog:ElasticsearchEndpoint` 指定，API Key 使用环境变量 `Serilog__ElasticsearchApiKey`。

## VS Code 调试

安装 .NET 10 SDK 和 Microsoft C# 扩展（`ms-dotnettools.csharp`），使用 VS Code 打开仓库根目录或现有工作区。调试入口位于 [.vscode/launch.json](../.vscode/launch.json)，构建任务位于 [.vscode/tasks.json](../.vscode/tasks.json)。在“运行和调试”中选择“后端：Ape.Volo.Web”，按 F5 自动构建 Debug 版本并启动，F9 设置断点，Shift+F5 停止调试。

调试复用 [launchSettings.json](../backend/src/Ape.Volo.Web/Properties/launchSettings.json) 的 `Ape.Volo.Web` 配置：环境为 Development，地址为 `http://localhost:9002`，Swagger 入口为 `http://localhost:9002/swagger/api`。工作目录设为 Web 项目目录，确保相对路径配置、私钥和资源可正确加载；启动前按 [快速开始](../README.md#快速开始) 核对数据库与密钥。

## 检查

在 `backend/` 执行：

```bash
dotnet build Ape.Volo.Admin.slnx
dotnet test Ape.Volo.Admin.slnx
```

[tests/](../backend/tests/) 保留契约、安全、事务和关键行为回归。PostgreSQL 集成测试默认跳过；设置 `APEVOLO_TEST_POSTGRES` 指向一次性 PostgreSQL 13+ 实例后运行，会创建并删除独立测试库。业务流程、实际 HTTP 响应和目标数据库初始化仍需按改动验证。
