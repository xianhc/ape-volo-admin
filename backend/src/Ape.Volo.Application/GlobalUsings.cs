global using Ape.Volo.Application.Contracts.Abstractions.Caching;
// 应用层实现统一引入契约层命名空间，避免每个 Service 逐个补 using。
global using Ape.Volo.Application.Contracts;
global using Ape.Volo.Application.Contracts.Log;
global using Ape.Volo.Application.Contracts.Message.Email.Accounts;
global using Ape.Volo.Application.Contracts.Message.Email.Templates;
global using Ape.Volo.Application.Contracts.Operations;
global using Ape.Volo.Application.Contracts.Organization.Positions;
global using Ape.Volo.Application.Contracts.Identity;
global using Ape.Volo.Application.Contracts.Identity.Auth;
global using Ape.Volo.Application.Contracts.Organization.Departments;
global using Ape.Volo.Application.Contracts.Identity.Menus;
global using Ape.Volo.Application.Contracts.Identity.OnlineUsers;
global using Ape.Volo.Application.Contracts.Identity.Roles;
global using Ape.Volo.Application.Contracts.Identity.Users;
global using Ape.Volo.Application.Contracts.Message.Email.QueuedEmails;
global using Ape.Volo.Shared.Enums;
// 对象映射器、缓存、当前用户由构造函数注入，各 Service 无需逐个 using
global using Ape.Volo.Common.Global;
global using Ape.Volo.Application.Contracts.Abstractions;
global using Ape.Volo.Application.Contracts.Abstractions.Security;
global using Ape.Volo.Core.Caches;
global using Ape.Volo.Core.Extensions;
// 配置项通过 IOptions<T> 注入
global using Microsoft.Extensions.Options;
