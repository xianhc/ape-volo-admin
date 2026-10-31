# 架构约定

以下说明当前实现的约束与影响，具体开发方式见 [后端开发](backend.md)、[前端开发](frontend.md) 和 [接口契约](contract.md)。

## 契约生成动态接口

对外能力由 `Application.Contracts` 显式声明，宿主根据契约生成端点，因此新增常规业务方法无需重复编写转发控制器；同时，契约和方法的命名会影响公开路由，不能只把重命名当作内部重构。

登录、刷新和 multipart 上传保留显式控制器。生成规则与源码入口见 [接口契约](contract.md)。

## 通过接口代理执行切面

授权位于事务和缓存之前，保证缓存命中仍先检查权限，未通过授权时不进入业务事务。该行为依赖 Autofac 的接口代理；类内直接调用和手动创建实例不会经过这些切面，调用需要授权、事务或缓存的服务时应通过注入的接口。

顺序与配置开关见 [服务注册](../backend/src/Ape.Volo.Web/Extensions/AutofacExtensions.cs)。

## 同步开启事务

SqlSugar 的连接上下文依赖 `AsyncLocal`。如果在另一个 `async` 方法内开启事务，返回调用方后可能丢失该上下文，后续写入落到其他连接。因此事务在调用方同步开启，业务操作、提交和释放仍可异步执行。

约束见 [IUnitOfWork](../backend/src/Ape.Volo.Core/Repository/UnitOfWork/IUnitOfWork.cs)，调用方式与提交后动作语义见 [后端开发](backend.md#事务与缓存)。

## CodeFirst 管理表结构

实体是结构同步的输入，主库的结构同步和种子数据写入由 `System:IsInitDb` 控制；开关关闭时，修改主库实体不会触发启动时的结构同步。全局禁止 CodeFirst 删除已有列，避免实体差异直接造成删列；因此从实体移除字段也不代表数据库旧列已删除。

执行入口见 [SeedService](../backend/src/Ape.Volo.Core/SeedData/SeedService.cs)，删列限制见 [SqlSugar 配置](../backend/src/Ape.Volo.Web/Extensions/SqlSugarExtensions.cs)。结构变更仍须核对目标数据库结果。

## 数据范围由标记接口决定

软删除和数据范围使用不同的标记接口，实体可以保留软删除而不参与组织数据范围过滤。选择实体基类时，应根据数据的访问范围判断是否需要 `IDataScopedEntity`；日志等不需要该限制的实体使用不带此标记的基类。

跨数据范围的内部读取使用 [SugarQueryProvider](../backend/src/Ape.Volo.Core/Repository/SugarHandler/SugarQueryProvider.cs)。它在独立客户端上移除数据范围过滤，保留软删除过滤，避免改变普通业务查询的过滤器状态。

## Element Plus 全量注册

当前在 [插件入口](../frontend/src/plugins/index.ts) 全量注册组件，在 [main.ts](../frontend/src/main.ts) 加载完整样式。页面模板因此无需逐个导入 Element Plus 组件，函数式 API 仍需显式导入。

[Vite 分包配置](../frontend/vite.config.ts) 只改变产物组织，不代表已经改为按需注册。调整加载策略时，应一起核对组件注册、样式入口和构建配置。
