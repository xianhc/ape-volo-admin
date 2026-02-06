## 📚 项目简介

Ape-Volo-Admin 是一套面向企业场景的 **权限管理 / 后台管理系统解决方案**，帮助你快速搭建：用户、角色、菜单、接口权限、数据权限等通用后台能力，并在此基础上进行二次开发。

基于 **.NET 8 + SqlSugar + Vue 3 + Element Plus + Vite** 构建，默认开箱即用，同时保留足够的扩展空间，适合绝大多数 **.NET/C#** 项目落地。

- 通用能力完备：认证授权、审计日志、缓存、任务调度、限流、数据权限、国际化等
- 架构清晰可维护：分层模块化（接口层 / 业务层 / 基础设施层…）便于扩展与替换
- 低侵入易集成：核心能力以通用方式提供，尽量减少对业务代码的“硬绑定”

## 🌐 在线体验

- 系统官网：<a href="https://www.apevolo.com" target="_blank" rel="noopener noreferrer">https://www.apevolo.com</a>
- 系统预览：<a href="https://vip.apevolo.com" target="_blank" rel="noopener noreferrer">https://vip.apevolo.com</a>
- 体验账号：`apevolo / 123456`

## 💒 代码仓库

### 后端 / API

- GitHub：https://www.github.com/xianhc/ape-volo-admin
- Gitee：https://www.gitee.com/xianhc/ape-volo-admin

### 前端 / Web

- GitHub：https://www.github.com/xianhc/ape-volo-web
- Gitee：https://www.gitee.com/xianhc/ape-volo-web

## ⚙️ 模块说明

| #   | 模块功能 | 项目文件                | 说明                                      |
| --- | -------- | ----------------------- | ----------------------------------------- |
| 1   | 接口层   | Web API / 控制器层      | 对外提供 REST API、鉴权、限流、审计等入口 |
| 2   | 业务层   | 业务服务实现层          | 业务编排、领域逻辑、应用服务              |
| 3   | 通用基础 | 工具与扩展库            | 通用工具类、扩展方法、文件/图像处理等     |
| 4   | 核心能力 | 核心组件层              | AOP 拦截、系统配置、App 服务等            |
| 5   | 数据实体 | 实体映射层              | 数据库实体映射、基础实体定义              |
| 6   | 事件总线 | EventBus 组件           | 事件发布/订阅、集成消息中间件             |
| 7   | 业务契约 | 业务接口定义层          | 业务服务接口、对外契约定义                |
| 8   | 基础设施 | Infrastructure 扩展层   | 依赖注入、认证/授权扩展、中间件等         |
| 9   | 仓储层   | Repository / UoW 层     | 数据访问封装、事务、仓储扩展              |
| 10  | 共享模型 | DTO / Query / Validator | 请求 DTO、查询参数、校验器等              |
| 11  | 作业调度 | Job / Scheduler 层      | 定时任务、任务服务实现                    |
| 12  | 视图模型 | ViewModel 层            | UI/展示层对象、前后端交互模型             |

## 🚀 系统特性

- 全面采用 `async/await` 异步编程
- 基于「仓储 + 服务 + 接口」组织方式，构建 RESTful 风格 API
- 使用 SqlSugar ORM（CodeFirst），封装 `BaseService` 数据库基础操作
- 同时支持 Redis 与 `DistributedCache`，并扩展 SqlSugar 二级缓存
- 使用 Autofac IOC 容器，实现服务批量自动注入
- 集成 Swagger UI 生成 WebAPI 文档
- 集成 Serilog 日志（数据库 / 控制台 / 文件 / Elasticsearch）
- 封装 Quartz.NET 调度中心，支持定时任务
- 统一异常过滤、审计过滤：集中记录系统异常与接口请求
- 缓存拦截器：对业务方法结果进行缓存处理
- 事务拦截器：对业务方法数据库操作自动事务处理
- 配置集中化：封装 `appsettings.json` 读取与管理
- 自定义鉴权：重写 ASP.NET Core `AuthorizationHandler` 实现自定义规则
- 多数据库支持：MySql、SqlServer、Sqlite、Oracle、PostgreSQL、达梦、神通、GaussDB 等
- 消息队列支持：RabbitMQ、RedisMQ
- CORS 跨域、接口限流
- 数据能力：读写分离、多库、分表
- 多租户：ID 隔离、库隔离
- 数据权限：全部 / 本人 / 本部门 / 本部门及以下 / 自定义
- 数据字典、自定义设置、国际化

## ⚡ 快速开始

### 环境准备

- .NET SDK：`8.x`
- 推荐 IDE：JetBrains Rider / Visual Studio / VS Code

### 运行（默认 Sqlite + DistributedCache）

1. 编译通过后启动后端 API 服务（解决方案中的 WebAPI 项目）
2. 首次运行会自动创建数据库表，并初始化基础数据

可选命令行方式启动（在仓库根目录执行）：

```bash
dotnet restore
dotnet run --project .\Ape.Volo.Api\Ape.Volo.Api.csproj
```

## ⭐️ 支持作者

如果这个项目对你有帮助，欢迎在 GitHub 或 Gitee 点个 Star：

- GitHub：https://www.github.com/xianhc/ape-volo-admin
- Gitee：https://www.gitee.com/xianhc/ape-volo-admin

## 🙋 反馈交流

### QQ群：839263566

| QQ 群 |
| :---: |

| <img width="150" src="https://www.apevolo.com/contact/wechat/20230723172503.jpg">

### 微信群

| 微信 |
| :--: |

| <img width="150" src="https://www.apevolo.com/contact/wechat/20230723172451.jpg">

添加微信，备注"加群"

## 🤟 捐赠

如果你觉得这个项目对你有帮助，你可以请作者喝饮料 🍹 [点我](https://www.apevolo.com/donate/)

## 💡 其他说明

### Go 版本（基于 .NET 版本的复刻实现）

Go 版本属于“无心插柳”的产物，更多是出于个人对编程语言的兴趣（Go 语言爱好/练手）而做的复刻实现。

它是基于本仓库 **.NET 版本的功能与接口设计** 进行复刻的，整体以 **.NET 版本为主线**；如两者出现差异，请以 **.NET 版本** 为准。

- GitHub：https://www.github.com/xianhc/ape-volo-admin-go
- Gitee：https://www.gitee.com/xianhc/ape-volo-admin-go


## 📄 许可证

本项目采用 Apache-2.0 license 许可证 - 查看 [LICENSE](LICENSE) 文件了解更多详情。
