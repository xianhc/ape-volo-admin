using System.Reflection;
using Ape.Volo.Application.Contracts.Abstractions.Caching;
using Ape.Volo.Application.Contracts.Identity.Models;
using Ape.Volo.Application.Identity;
using Ape.Volo.Common.Enums;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Ape.Volo.Entity.Base;
using Ape.Volo.Entity.Identity.Roles;
using Ape.Volo.Entity.Identity.Users;
using Ape.Volo.Entity.Organization.Departments;
using Ape.Volo.Shared.Enums;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SqlSugar;
using Xunit;
using MemoryCache = Ape.Volo.Core.Caches.Distributed.DistributedCache;

namespace Ape.Volo.Web.Tests;

public class DataScopeTests
{
    [Theory]
    [InlineData(DataScopeType.None, new long[] { })]
    [InlineData(DataScopeType.MySelf, new long[] { 1 })]
    [InlineData(DataScopeType.MyDept, new long[] { 1, 2 })]
    [InlineData(DataScopeType.MyDeptAndBelow, new long[] { 1, 2, 3 })]
    [InlineData(DataScopeType.Customize, new long[] { 3 })]
    public async Task RestrictedScopesPreserveRoleRulesAndSoftDeleteFiltering(DataScopeType type, long[] expected)
    {
        using var fixture = new ScopeFixture();
        fixture.AssignRoles(type);
        var result = await fixture.Service.GetDataScopeAsync(1);
        Assert.False(result.AllowAll);
        Assert.Equal(expected, result.UserIds.OrderBy(id => id));
        Assert.NotNull(await fixture.Cache.GetAsync<DataScopeSnapshot>("identity:user:dataScope:id:1"));
        // 同一缓存结果可再次读取，用户集合不会丢失类型。
        var hit = await fixture.Service.GetDataScopeAsync(1);
        Assert.Equal(expected, hit.UserIds.OrderBy(id => id));
    }

    [Fact]
    public async Task AllScopeTakesPrecedenceAndRestrictedRolesAreUnionedWithoutDuplicates()
    {
        using var fixture = new ScopeFixture();
        fixture.AssignRoles(DataScopeType.MySelf, DataScopeType.MyDept, DataScopeType.Customize);
        var restricted = await fixture.Service.GetDataScopeAsync(1);
        Assert.False(restricted.AllowAll);
        Assert.Equal(new long[] { 1, 2, 3 }, restricted.UserIds.OrderBy(id => id));

        fixture.AssignRoles(DataScopeType.All);
        await fixture.Permissions.UserChangedAsync(1);
        var all = await fixture.Service.GetDataScopeAsync(1);
        Assert.True(all.AllowAll);
        Assert.Empty(all.UserIds);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(5)]
    [InlineData(99)]
    public async Task MissingDeletedOrRolelessUsersHaveNoAccess(long userId)
    {
        using var fixture = new ScopeFixture();
        var result = await fixture.Service.GetDataScopeAsync(userId);
        Assert.False(result.AllowAll);
        Assert.Empty(result.UserIds);
    }

    [Fact]
    public async Task OrganizationAndRoleChangesInvalidateTheWholeSnapshot()
    {
        using var fixture = new ScopeFixture();
        fixture.AssignRoles(DataScopeType.MyDept);
        Assert.Equal(new long[] { 1, 2 }, (await fixture.Service.GetDataScopeAsync(1)).UserIds.OrderBy(id => id));

        await fixture.Db.Updateable<User>().SetColumns(user => user.DeptId == 20).Where(user => user.Id == 2).ExecuteCommandAsync();
        await fixture.Permissions.DataScopeChangedAsync();
        Assert.Equal(new long[] { 1 }, (await fixture.Service.GetDataScopeAsync(1)).UserIds);

        await fixture.Db.Updateable<Role>().SetColumns(role => role.DataScopeType == DataScopeType.All)
            .Where(role => role.Id == (long)DataScopeType.MyDept).ExecuteCommandAsync();
        await fixture.Permissions.RoleChangedAsync((long)DataScopeType.MyDept);
        var all = await fixture.Service.GetDataScopeAsync(1);
        Assert.True(all.AllowAll);
        Assert.Empty(all.UserIds);
    }

    [Fact]
    public async Task OrganizationInvalidationClearsAllUsersOnlyAfterCommit()
    {
        using var fixture = new ScopeFixture();
        fixture.AssignRoles(DataScopeType.MyDept);
        fixture.Db.Insertable(new UserRole { UserId = 2, RoleId = (long)DataScopeType.MyDept }).ExecuteCommand();
        await fixture.Service.GetDataScopeAsync(1);
        await fixture.Service.GetDataScopeAsync(2);

        await using (var transaction = fixture.UnitOfWork.BeginTransaction(Propagation.Required))
        {
            await fixture.Db.Updateable<User>().SetColumns(user => user.DeptId == 10)
                .Where(user => user.Id == 3).ExecuteCommandAsync();
            await fixture.Permissions.DataScopeChangedAsync();
            Assert.NotNull(await fixture.Cache.GetAsync<DataScopeSnapshot>("identity:user:dataScope:id:1"));
            Assert.NotNull(await fixture.Cache.GetAsync<DataScopeSnapshot>("identity:user:dataScope:id:2"));
            await transaction.CommitAsync();
        }

        Assert.Null(await fixture.Cache.GetAsync<DataScopeSnapshot>("identity:user:dataScope:id:1"));
        Assert.Null(await fixture.Cache.GetAsync<DataScopeSnapshot>("identity:user:dataScope:id:2"));
        Assert.Equal(new long[] { 1, 2, 3 }, (await fixture.Service.GetDataScopeAsync(1)).UserIds.OrderBy(id => id));
        Assert.Equal(new long[] { 1, 2, 3 }, (await fixture.Service.GetDataScopeAsync(2)).UserIds.OrderBy(id => id));
    }

    [Fact]
    public async Task TransactionReadsBypassSharedCacheAndRollbackDiscardsInvalidation()
    {
        using var fixture = new ScopeFixture();
        fixture.AssignRoles(DataScopeType.MySelf);
        await fixture.Service.GetDataScopeAsync(1);
        await using (fixture.UnitOfWork.BeginTransaction(Propagation.Required))
        {
            var temporary = await fixture.Permissions.GetOrCreateAsync(PermissionCacheItem.DataScope, 1,
                () => Task.FromResult(new DataScopeSnapshot { AllowAll = true }));
            Assert.True(temporary.AllowAll);
            await fixture.Permissions.UserChangedAsync(1);
        }
        var original = await fixture.Service.GetDataScopeAsync(1);
        Assert.False(original.AllowAll);
        Assert.Equal(new long[] { 1 }, original.UserIds);
    }

    private sealed class ScopeFixture : IDisposable
    {
        private readonly SqliteConnection _connection;
        private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memory;
        public ICache Cache { get; }
        public SqlSugarScope Db { get; }
        public UnitOfWork UnitOfWork { get; }
        public PermissionCacheService Permissions { get; }
        public DataScopeService Service { get; }

        public ScopeFixture()
        {
            _connection = new SqliteConnection($"Data Source=scope-{Guid.NewGuid():N};Mode=Memory;Cache=Shared");
            _connection.Open();
            Db = new SqlSugarScope(new ConnectionConfig
            {
                ConnectionString = _connection.ConnectionString, DbType = DbType.Sqlite,
                InitKeyType = InitKeyType.Attribute, IsAutoCloseConnection = true,
                ConfigureExternalServices = new ConfigureExternalServices
                {
                    EntityService = (property, column) =>
                    {
                        if (!column.IsPrimarykey && new NullabilityInfoContext().Create(property).WriteState == NullabilityState.Nullable)
                            column.IsNullable = true;
                    }
                }
            });
            Db.CodeFirst.InitTables(typeof(User), typeof(Role), typeof(UserRole), typeof(Department), typeof(RoleDepartment));
            Db.Insertable(new[]
            {
                new Department { Id = 10, Name = "parent", Enabled = true },
                new Department { Id = 20, Name = "child", ParentId = 10, Enabled = true },
                new Department { Id = 30, Name = "disabled", ParentId = 10, Enabled = false }
            }).ExecuteCommand();
            Db.Insertable(Enumerable.Range(1, 5).Select(id => new User
            {
                Id = id, UserName = "user-" + id, Email = $"user-{id}@test.invalid",
                DeptId = id == 3 ? 20 : id == 4 ? 30 : 10, IsDeleted = id == 5
            }).ToArray()).ExecuteCommand();
            Db.Insertable(Enum.GetValues<DataScopeType>().Select(type => new Role
            {
                Id = (long)type, Name = type.ToString(), AuthCode = type.ToString(), DataScopeType = type
            }).ToArray()).ExecuteCommand();
            Db.Insertable(new RoleDepartment { RoleId = (long)DataScopeType.Customize, DeptId = 20 }).ExecuteCommand();
            Db.QueryFilter.AddTableFilter<ISoftDeletedEntity>(entity => !entity.IsDeleted);
            // 模拟当前账号看不到权限元数据；实际服务必须通过独立客户端绕过该过滤器。
            Db.QueryFilter.AddTableFilter<IDataScopedEntity>(entity => entity.CreatorId == 999);

            var queries = new SugarQueryProvider(Db);
            UnitOfWork = new UnitOfWork(Db, NullLogger<UnitOfWork>.Instance);
            _memory = new Microsoft.Extensions.Caching.Memory.MemoryCache(Options.Create(new MemoryCacheOptions()));
            Cache = new MemoryCache(_memory);
            Permissions = new PermissionCacheService(Cache, queries, UnitOfWork,
                Options.Create(new AopOptions { Cache = true }));
            Service = new DataScopeService(queries, Permissions);
        }

        public void AssignRoles(params DataScopeType[] types) => Db.Insertable(types.Select(type => new UserRole
        {
            UserId = 1, RoleId = (long)type
        }).ToArray()).ExecuteCommand();

        public void Dispose() { _memory.Dispose(); Db.Dispose(); _connection.Dispose(); }
    }
}
