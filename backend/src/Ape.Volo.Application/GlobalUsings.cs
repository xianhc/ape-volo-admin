global using Ape.Volo.Application.Contracts.Abstractions.Caching;
// 应用层实现统一引入契约层命名空间，避免每个 Service 逐个补 using。
global using Ape.Volo.Application.Contracts;
global using Ape.Volo.Application.Contracts.Log;
global using Ape.Volo.Application.Contracts.Message.Email.Accounts;
global using Ape.Volo.Application.Contracts.Message.Email.Templates;
global using Ape.Volo.Application.Contracts.Operations;
global using Ape.Volo.Application.Contracts.Organization;
global using Ape.Volo.Application.Contracts.Permission;
global using Ape.Volo.Application.Contracts.Permission.Auth;
global using Ape.Volo.Application.Contracts.Permission.Departments;
global using Ape.Volo.Application.Contracts.Permission.Menus;
global using Ape.Volo.Application.Contracts.Permission.OnlineUsers;
global using Ape.Volo.Application.Contracts.Permission.Roles;
global using Ape.Volo.Application.Contracts.Permission.Users;
global using Ape.Volo.Application.Contracts.Message.Email.QueuedEmails;
global using Ape.Volo.Application.Contracts.SystemManagement.AppSecrets;
global using Ape.Volo.Application.Contracts.SystemManagement.Dictionaries;
global using Ape.Volo.Application.Contracts.SystemManagement.FileRecords;
global using Ape.Volo.Application.Contracts.SystemManagement.ServerInfo;
global using Ape.Volo.Application.Contracts.SystemManagement.Settings;
global using Ape.Volo.Shared.Enums;
// 对象映射器、缓存、当前用户由构造函数注入，各 Service 无需逐个 using
global using Ape.Volo.Common.Global;
global using Ape.Volo.Application.Contracts.Abstractions;
global using Ape.Volo.Application.Contracts.Abstractions.Security;
global using Ape.Volo.Core.Caches;
global using Ape.Volo.Core.Extensions;
// 配置项通过 IOptions<T> 注入
global using Microsoft.Extensions.Options;
