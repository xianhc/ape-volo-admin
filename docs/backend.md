# 后端开发

## 分层

项目代码位于 `backend/src/`。

| 项目                            | 直接项目依赖                                         |
| ------------------------------- | ---------------------------------------------------- |
| `Web`                           | Application、Contracts、Core、Common、Entity、Shared |
| `Application`                   | Contracts、Core、Common、Entity                      |
| `Core`                          | Contracts、Common、Entity                            |
| `Contracts`、`Common`、`Entity` | Shared                                               |
| `Shared`                        | 无                                                   |

表中的 `Contracts` 为 `Ape.Volo.Application.Contracts`，其余项目使用 `Ape.Volo.` 前缀。

`Web` 处理 HTTP、认证、上传、文件存储和调度宿主。`Application` 实现业务服务，不使用 `HttpContext`、`IFormFile`、`ActionResult`；服务授权特性可使用 `Authorize`。

新增模块时联查 [接口契约](contract.md) 与 [岗位模块示例](examples/backend-module-example.md)；通用修改范围和验证要求见 [Agent 工作约定](../AGENTS.md)。

上传契约使用 `UploadFile`，HTTP 文件类型由宿主适配。

## 契约与实现

- 对外服务接口继承 `IApplicationService`，接口和 DTO 放在 `Application.Contracts`。
- 实现类按模块放在 `Application`，映射声明放在 `Application/Mapping/`。
- 契约不出现实体类型；DTO、VO 和查询参数放在模块的 `Models/`。
- 业务校验失败返回 `OperateResult.Error(...)`，不要依靠异常表达普通操作失败。
- 方法传递 `CancellationToken`，不在内部丢弃请求取消信号；提交后的必要清理不随客户端断开取消。
- 实现成员使用 `inheritdoc`；契约、DTO 和控制器补齐 XML 注释。

`HasPerm`、`HasRole` 写在实现方法上，权限码由 `SystemPermissions` 定义。服务通过 Autofac 接口代理调用；授权拦截器始终注册，事务和缓存切面分别受 `Aop:Transactions`、`Aop:Cache` 控制。注册顺序为授权、事务、缓存。

未声明具体权限码不表示可匿名访问；动态端点默认校验登录，接口生成与请求约定见 [接口契约](contract.md)。

源码入口：[服务注册与拦截顺序](../backend/src/Ape.Volo.Web/Extensions/AutofacExtensions.cs)、[服务授权](../backend/src/Ape.Volo.Core/Aop/AuthorizationAop.cs)、[权限码](../backend/src/Ape.Volo.Application.Contracts/Authorization/SystemPermissions.cs)。

## 数据访问

- 实体主键为 `long`，新增时由雪花 ID 生成；部署实例使用不同的 `IdGenerator:WorkerId`。
- `AuditedEntity` 包含审计与软删除字段，`DataScopedEntity` 额外声明数据范围标记。
- 请求新增数据时，创建人和创建时间由服务端覆盖；无请求身份的初始化保留种子数据审计值。
- 全局过滤器处理软删除；实现 `IDataScopedEntity` 的实体参与数据范围过滤。
- 跨数据范围的内部查询使用 `ISugarQueryProvider.QueryWithoutDataScopeAsync`，该入口保留软删除过滤。
- 查询条件使用 `QueryCondition`；分页使用 `PageAsync`，返回 `PagedResultDto<T>`。
- `QueryOptions` 支持投影 `SelectExpression`、锁提示 `LockString`（默认 `SqlWith.NoLock`）和 SqlSugar 查询缓存 `CacheDurationInSeconds`（秒，默认 0，不启用）；同时保留条件、导航加载及分表配置。
- 导航优先使用 `IncludePropertyNameList = [nameof(实体.导航)]` 白名单，按 SqlSugar 的 `IncludesByNameString` 加载指定一级导航。非空白名单优先于 `IsIncludes` 和排除名单；未提供或为空时，仍由 `IsIncludes` 开启 `IncludesAllFirstLayer` 并应用 `IgnorePropertyNameList`。用户、角色和字典列表已显式声明所需导航，避免新增实体导航自动扩大查询范围。
- 树节点父 ID 使用 `long?`，顶级为 `null`；根查询使用 `IsRoot`。

源码入口：[全局过滤器](../backend/src/Ape.Volo.Web/Extensions/SqlSugarExtensions.cs)、[跨数据范围查询](../backend/src/Ape.Volo.Core/Repository/SugarHandler/SugarQueryProvider.cs)。

## 事务与缓存

显式事务在调用方同步开启：

```csharp
await using var transaction = unitOfWork.BeginTransaction(Propagation.Required);
// 数据写入
await transaction.CommitAsync(ct);
```

不要把事务开启封装到另一个异步方法中，原因见 [同步开启事务](decisions.md#同步开启事务)。`UseTran` 仅用于返回 `Task` 或 `Task<T>` 的业务方法。

`IUnitOfWork.AfterCommitAsync` 登记提交后动作：没有事务时立即执行，有事务时等待根事务提交。保存点回滚丢弃该范围内的动作，根事务回滚丢弃全部动作。动作失败会继续执行其他已登记动作并返回异常，不回滚已提交的数据。该机制不持久化动作，进程中断后不会自动补偿。

`UseCache.Expiration` 单位为分钟。通用缓存键由前缀和方法参数生成，不包含 `CancellationToken`；依赖当前用户、但没有用户参数的方法必须显式构造用户缓存键。

用户资料、角色代码、权限代码、数据范围和用户菜单缓存统一由 `PermissionCacheService` 管理，保留逻辑键前缀、用户 ID 后缀和有效期。用户、角色、菜单变更通过该入口登记提交后失效。`UseCache` 和权限缓存在事务内直接查询，不读取或回填共享结果。原 `Aop:Cache` 开关继续控制此前由切面缓存的内容。

缓存存储契约为 `Application.Contracts/Abstractions/Caching/ICache`，仅由 Core 提供具体实现。可选键枚举使用 `ICacheKeyCatalog`，仅 Redis 注册此能力；Redis 原生连接只供基础设施使用。

[CacheEntries](../backend/src/Ape.Volo.Core/Caches/CacheEntries.cs) 使用原子比较交换协调加载与失效。加载开始时观察缓存修订，回填失败则重新读取，防止旧查询覆盖失效后的修订。物理键使用 `cache:entries:` 前缀。登录会话使用独立的会话键。

字典及明细变更失效名称和所属字典明细缓存；系统参数更新或改名失效新旧名称缓存。这些清理均通过 `AfterCommitAsync` 登记，回滚不清理。

菜单管理缓存和用户会话撤销也通过 `AfterCommitAsync` 登记，等待最外层事务提交；回滚不触发清理或撤销。菜单树与权限码查询排除禁用菜单。

菜单管理仅缓存全量快照；祖先和直属子节点查询直接读取数据库，避免移动、删除节点后维护多份依赖树的失效关系。全量快照同样使用原子回填协议。

源码入口：[工作单元契约](../backend/src/Ape.Volo.Core/Repository/UnitOfWork/IUnitOfWork.cs)、[事务与提交后动作](../backend/src/Ape.Volo.Core/Repository/UnitOfWork/UnitOfWork.cs)、[权限缓存](../backend/src/Ape.Volo.Application/Permission/PermissionCacheService.cs)。

## 初始化与配置

- 配置顺序：`appsettings.json` → 环境文件 → `IpRateLimit.json` → 环境变量 → 命令行。
- 当前配置链没有加载 User Secrets。
- `System:IsInitDb` 控制启动时的数据库初始化，先日志库、后主库；主库同步结构并写入种子数据，实体通过 `CodeFirst.InitTables` 同步，禁止删除已有列。已有日志表仍受实体上的 `IsDisabledUpdateAll` 限制。
- 表和字段注释由 SqlSugar 从实体的 XML `summary` 读取；Entity 项目在 Debug/Release 均生成 `Ape.Volo.Entity.xml`，部署时须与实体程序集放在同一目录。已有主库表补齐注释需开启 `System:IsInitDb` 后重启；声明 `IsDisabledUpdateAll = true` 的已有日志分表不会更新，新建分表会读取注释。
- 种子 JSON 位于 Web 内容根目录的 `resources/db/`，随构建和发布复制，不由静态文件中间件公开。
- IP 归属地离线库位于 Web 内容根目录的 `resources/ip/ip2region.xdb`，随构建和发布复制，仅供后端读取。
- `System:UseRedisCache` 选择 Redis 或内存分布式缓存；内存缓存不跨进程共享。
- 在线用户枚举要求 Redis。
- 静态上传文件由 `wwwroot` 下的 `/uploads/file/` 提供，不带 `/api` 前缀。
- 限流使用连接 IP，不接受 `X-ClientId` 白名单或客户端自报的 `X-Real-IP`。转发头由 ASP.NET Core 处理，默认仅信任回环代理；代理位于其他主机时，用 `ForwardedHeaders:KnownProxies` 配置代理 IP 数组。显式配置后替换默认信任范围，只处理一层转发。
- MiniProfiler 仅在 `Development` 且 `Middleware:MiniProfiler` 开启时挂载；`/profiler` 的结果和列表仅已登录的 `admin` 角色可访问。

源码入口：[配置与宿主启动](../backend/src/Ape.Volo.Web/Program.cs)、[初始化开关与顺序](../backend/src/Ape.Volo.Web/Extensions/DataSeederExtensions.cs)、[表结构和种子数据初始化](../backend/src/Ape.Volo.Core/SeedData/SeedService.cs)。

## 任务调度

配置实体为 `ScheduledJob`，表名为 `sys_scheduled_job`。查询接口为 `scheduled-job`，增删改及运行控制由 `scheduled-job-manager` 提供。数据库配置和调度器状态由 `ScheduledJobManager` 协调。

## Elasticsearch 日志

使用官方维护的 `Elastic.Serilog.Sinks` 9，要求 Elasticsearch 8.15 及以上，日志分别写入 `logs-ape-volo-application` 和 `logs-ape-volo-sql` 数据流。仅同时启用 `Serilog:ToElasticsearch` 与 `Middleware:Elasticsearch` 时连接服务器。

`Serilog:ElasticsearchEndpoint` 默认 `http://127.0.0.1:9200`；认证 API Key 通过环境变量 `Serilog__ElasticsearchApiKey` 提供。启动时安装 ECS 模板失败会阻止启动，避免日志输出静默失效。数据库日志批处理使用 Serilog 内置批处理，每批最多 500 条，缓冲上限 10000 条。

## 检查

修改后端代码后，在 `backend/` 执行：

```bash
dotnet build Ape.Volo.Admin.slnx
dotnet test Ape.Volo.Admin.slnx
```

基础测试位于 [tests/](../backend/tests/)，保留 Web、Application、Core、Common 四个项目，共 9 个用例，分别检查控制器路由与 JSON 契约、服务接口约定、对象映射和序列化，测试方法使用英文命名。修改业务行为、HTTP 契约或表结构时，仍需验证对应流程、实际请求响应和目标数据库初始化结果；基础测试不覆盖完整业务功能。
