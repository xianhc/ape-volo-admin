<p align="center">
  <img src="frontend/public/logo.png" width="88" alt="Ape-Volo-Admin Logo" />
</p>

<h1 align="center">Ape-Volo-Admin</h1>

<p align="center">基于 .NET 10、Vue 3 与 TypeScript 的前后端分离后台管理系统</p>
<p align="center">集成认证授权、组织管理与常用业务模块，为中后台业务开发提供基础。</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-10-512BD4?style=flat-square" alt=".NET 10" />
  <img src="https://img.shields.io/badge/Vue-3.5-42b883?style=flat-square" alt="Vue 3.5" />
  <img src="https://img.shields.io/badge/TypeScript-5.9-3178C6?style=flat-square" alt="TypeScript 5.9" />
  <img src="https://img.shields.io/badge/Element_Plus-2-409EFF?style=flat-square" alt="Element Plus 2" />
  <a href="LICENSE"><img src="https://img.shields.io/badge/License-Apache--2.0-blue?style=flat-square" alt="Apache License 2.0" /></a>
</p>

<p align="center">
  <a href="#版权与授权">版权与授权</a> ·
  <a href="https://www.apevolo.com">官网与在线文档</a> ·
  <a href="https://vip.apevolo.com">在线体验</a> ·
  <a href="#快速开始">快速开始</a> ·
  <a href="docs/README.md">开发文档</a> ·
  <a href="https://gitee.com/xianhc/ape-volo-admin">Gitee</a>
</p>

## 项目文档

- **系统官网 / 在线文档**：[https://www.apevolo.com](https://www.apevolo.com)
- **仓库开发文档**：[开发指南](docs/README.md) · [岗位模块示例](docs/examples/README.md)
- **软件著作权**：[查看证书](https://www.apevolo.com/copyright.pdf)

## 在线演示

- **系统预览**：[https://vip.apevolo.com](https://vip.apevolo.com)
- **体验账号**：`apevolo`
- **体验密码**：`123456`

## 项目介绍

Ape-Volo-Admin 是面向管理后台与企业内部业务系统的开源项目系列，覆盖 **C#、Go、Java 三大主流后端语言**，并提供基于 **Vue 3 + Element Plus** 的 **TypeScript 与 JavaScript 双前端版本**。团队可以结合现有技术栈与开发习惯，选择适合自己的版本。

项目内置用户、角色、菜单、部门和岗位管理，集成认证授权、数据权限、文件管理、任务调度、邮件与日志等常用能力。开发者可以复用已有业务模块及 CRUD、表格、表单组件，把精力集中在业务功能开发上。

**本仓库为主仓库**，包含 .NET 10 后端与 TypeScript 前端；Go、Java 后端及 JavaScript 前端的独立仓库见[项目系列](#项目系列)。

### 项目特点

- **三种后端选择**：C# / .NET、Go / Gin、Java / Spring Boot，适应不同团队的技术积累。
- **双前端版本**：TypeScript 与 JavaScript 两种选择，均基于 Vue 3 + Element Plus。
- **权限控制**：角色、菜单、按钮权限与五种数据范围。
- **契约驱动**：服务契约生成 HTTP 接口与 Swagger 文档。
- **会话认证**：JWT、令牌自动刷新、验证码与登录失败锁定。
- **界面配置**：7 种导航布局、主题切换、多标签页与移动端适配。
- **业务复用**：CRUD、分页、表单与导出组件，附岗位模块开发示例。

## 功能模块

| 模块       | 已实现功能                                           |
| ---------- | ---------------------------------------------------- |
| 权限与组织 | 用户、角色、菜单、部门、岗位、菜单授权、数据范围配置 |
| 系统管理   | 字典与字典项、系统参数、应用密钥、文件上传与文件记录 |
| 运维管理   | 定时任务配置、启停与执行、服务器监控                 |
| 日志管理   | 操作日志、异常日志、任务执行日志                     |
| 邮件管理   | 邮件账户、邮件模板、发送队列                         |
| 会话管理   | 在线用户查询、导出与强制下线，需要启用 Redis         |
| 个人中心   | 个人资料、头像、密码、邮箱与界面偏好                 |

## 技术栈

| 层次       | 主要技术                                                    |
| ---------- | ----------------------------------------------------------- |
| 后端框架   | .NET 10、ASP.NET Core、Autofac                              |
| 数据与缓存 | SqlSugar、SQLite / MySQL / SQL Server / PostgreSQL、进程内缓存 / Redis   |
| 认证与安全 | JWT、BCrypt、RSA、AspNetCoreRateLimit                       |
| 日志与调度 | Serilog、Quartz.NET，可选 Elasticsearch 日志输出            |
| 接口与工具 | Swashbuckle、Newtonsoft.Json、Mapster、ClosedXML、SkiaSharp |
| 前端框架   | Vue 3.5、TypeScript 5.9、Vite 6                             |
| 界面与样式 | Element Plus、UnoCSS、Sass                                  |
| 状态与请求 | Pinia、Vue Router、Axios                                    |
| 图表       | ECharts                                                     |

后端依赖版本集中维护在 [Directory.Packages.props](backend/Directory.Packages.props)，前端依赖与脚本维护在 [package.json](frontend/package.json)。

## 快速开始

### 1. 环境要求

| 工具     | 项目要求                     |
| -------- | ---------------------------- |
| .NET SDK | 10，后端目标框架为 `net10.0` |
| Node.js  | `^24.0.0` 或 `^26.0.0`       |
| pnpm     | 12，项目指定 `12.15.0`       |

默认使用 SQLite 和进程内缓存，首次启动自动初始化数据库。切换数据库等配置见[后端开发](docs/backend.md#初始化与配置)。

### 2. 启动后端

在仓库根目录执行：

```bash
dotnet restore backend/Ape.Volo.Admin.slnx
dotnet run --project backend/src/Ape.Volo.Web/Ape.Volo.Web.csproj
```

### 3. 启动前端

另开终端，在仓库根目录执行：

```bash
cd frontend
pnpm install
pnpm dev
```

### 4. 访问系统

| 服务       | 默认本地地址                                                                                 |
| ---------- | -------------------------------------------------------------------------------------------- |
| 管理界面   | [http://localhost:9001](http://localhost:9001)                                               |
| 后端服务   | [http://localhost:9002](http://localhost:9002)                                               |
| Swagger UI | [http://localhost:9002/swagger/api/index.html](http://localhost:9002/swagger/api/index.html) |

本地种子数据的初始账号为 `apevolo`，密码为 `123456`。前端端口与后端代理配置见 [frontend/.env.development](frontend/.env.development)，后端监听地址见 [launchSettings.json](backend/src/Ape.Volo.Web/Properties/launchSettings.json)；修改后端端口时需同步调整前端代理并重启前端服务。

### VS Code 工作区

打开根目录的 [ape-volo-admin-basic.code-workspace](ape-volo-admin-basic.code-workspace)，即可按 **前端 / 后端 / 工程** 分组浏览项目。

## 目录结构

```text
ape-volo-admin/
├── backend/
│   ├── src/
│   │   ├── Ape.Volo.Web/                    HTTP 宿主与基础设施
│   │   │   ├── Controllers/                 登录、刷新、文件上传等显式接口
│   │   │   ├── DynamicApi/                  契约扫描、动态路由与参数绑定
│   │   │   ├── Extensions/                  服务注册、认证与中间件配置
│   │   │   ├── resources/db/                初始化种子数据，不对外公开
│   │   │   ├── resources/ip/                IP 归属地离线库，不对外公开
│   │   │   └── wwwroot/                     静态资源与上传文件
│   │   ├── Ape.Volo.Application.Contracts/  服务接口、DTO 与业务端口
│   │   ├── Ape.Volo.Application/            业务服务与对象映射
│   │   ├── Ape.Volo.Core/                   仓储、事务、缓存、切面与初始化
│   │   ├── Ape.Volo.Entity/                 实体、审计字段与数据范围标记
│   │   ├── Ape.Volo.Common/                 特性、转换器与工具
│   │   └── Ape.Volo.Shared/                 枚举、异常与通用模型
│   ├── tests/
│   │   ├── Ape.Volo.Web.Tests/              控制器路由与 JSON 契约基础测试
│   │   ├── Ape.Volo.Application.Tests/      服务接口基础测试
│   │   ├── Ape.Volo.Core.Tests/             对象映射基础测试
│   │   └── Ape.Volo.Common.Tests/           序列化基础测试
│   ├── Ape.Volo.Admin.slnx                  后端解决方案
│   ├── Directory.Build.props               目标框架与构建设置
│   └── Directory.Packages.props            NuGet 版本管理
├── frontend/
│   ├── src/
│   │   ├── api/                            接口封装与传输类型
│   │   ├── views/                          业务页面与页面私有组件
│   │   ├── components/                     公共组件与 CRUD
│   │   ├── composables/                    表格、表单与导出逻辑
│   │   ├── services/
│   │   │   ├── http/                       请求处理与错误反馈
│   │   │   ├── session/                    登录、令牌刷新与会话管理
│   │   │   ├── appearance/                 主题、布局与界面偏好
│   │   │   ├── navigation/                 导航与标签页逻辑
│   │   │   └── logging/                    客户端诊断与日志上报
│   │   ├── pinia/                          全局状态
│   │   ├── router/                         路由配置
│   │   ├── directive/                      权限等自定义指令
│   │   ├── plugins/                        插件与应用能力注册
│   │   ├── assets/                         图片、图标与前端公钥
│   │   ├── style/                          全局样式与设计变量
│   │   ├── utils/                          通用工具
│   │   └── main.ts                         应用入口
│   ├── public/                             公共静态资源
│   ├── tests/                              部门树与菜单链接基础测试
│   ├── .env.development                    开发端口与代理配置
│   ├── package.json                        依赖与开发脚本
│   └── vite.config.ts                      开发服务与构建配置
├── docs/
│   ├── backend.md                          后端开发
│   ├── frontend.md                         前端开发
│   ├── contract.md                         接口契约
│   ├── decisions.md                        架构约定
│   └── examples/                           岗位模块示例
├── AGENTS.md                               Agent 工作约定
├── ape-volo-admin-basic.code-workspace      VS Code 工作区
├── README.md                               项目说明
└── LICENSE                                 许可证
```

## 开发文档

| 文档                                | 内容                                   |
| ----------------------------------- | -------------------------------------- |
| [开发入口](docs/README.md)          | 工作区配置、文档索引与任务查阅顺序     |
| [后端开发](docs/backend.md)         | 分层职责、数据访问、事务、缓存与配置   |
| [前端开发](docs/frontend.md)        | 请求、会话、组件与状态管理             |
| [接口契约](docs/contract.md)        | 动态路由、请求参数、响应格式与字段类型 |
| [架构约定](docs/decisions.md)       | 接口代理、事务、数据范围等实现约定     |
| [岗位示例](docs/examples/README.md) | 已有业务模块的前后端调用关系           |

## 项目系列

本仓库为主仓库，提供 .NET 后端与 TypeScript 前端；其他版本可按团队技术栈选择。

| 项目 | 定位 | 核心技术 |
| --- | --- | --- |
| **[ape-volo-admin](https://github.com/xianhc/ape-volo-admin)** | **主仓库 · 完整前后端** | .NET 10 + SqlSugar / Vue 3 + TypeScript + Element Plus |
| [ape-volo-admin-go](https://github.com/xianhc/ape-volo-admin-go) | Go 后端 | Go + Gin + GORM |
| [ape-volo-admin-java](https://github.com/xianhc/ape-volo-admin-java) | Java 后端 | Java 25 + Spring Boot 4 + MyBatis-Plus |
| [ape-volo-web-js](https://github.com/xianhc/ape-volo-web-js) | JavaScript 前端 | Vue 3 + JavaScript + Vite + Element Plus |

## 贡献指南

欢迎提交问题、功能建议、文档改进和代码修复。

### Issue 规范

- 提交前先搜索已有 Issue，避免重复反馈。
- Bug 请提供运行环境、复现步骤、预期与实际结果，以及必要的错误信息。
- 功能建议请说明使用场景、要解决的问题和预期行为。
- 日志、截图和示例配置请移除密码、令牌、密钥等敏感信息。

### Pull Request 规范

- Fork 仓库后在自己的分支修改，一个 PR 聚焦一个问题或功能。
- 提交信息简明说明改动；PR 描述包含改动目的、影响范围和验证结果，关联已有 Issue。
- 遵循现有目录结构与代码风格；修改接口、配置或目录职责时，同步更新文档。
- 按改动执行[后端检查](docs/backend.md#检查)或[前端检查](docs/frontend.md#检查)。界面改动补充截图与操作验证；未验证的范围请明确说明。

## 交流与反馈

[官方网站](https://www.apevolo.com) · [GitHub](https://github.com/xianhc/ape-volo-admin) · [Gitee](https://gitee.com/xianhc/ape-volo-admin)

QQ 群：`839263566` · 微信：`apevolo`（备注“加群”）

## 版权与授权

**版权所有 © 2022–2026 xianhc。**

本项目采用 [Apache License 2.0](LICENSE)，使用、修改与分发请遵循许可证要求。

Apache License 2.0 允许在遵守许可证条款的前提下使用、修改、商业使用与再分发本项目。

再分发时须提供许可证副本；以源码形式分发衍生作品时，须保留与作品相关的原始版权及署名通知，修改过的文件须显著注明已作修改。完整义务以 [LICENSE](LICENSE) 为准。

商业合作咨询：[apevolo@gmail.com](mailto:apevolo@gmail.com)。
