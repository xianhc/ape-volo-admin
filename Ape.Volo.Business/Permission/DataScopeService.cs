using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Ape.Volo.Common.Attributes;
using Ape.Volo.Common.Enums;
using Ape.Volo.Common.Extensions;
using Ape.Volo.Common.Global;
using Ape.Volo.Entity.Core.Permission;
using Ape.Volo.Entity.Core.Permission.Role;
using Ape.Volo.Entity.Core.Permission.User;
using Ape.Volo.IBusiness.Permission;
using SqlSugar;
using Enumerable = System.Linq.Enumerable;

namespace Ape.Volo.Business.Permission;

/// <summary>
/// 数据权限服务
/// </summary>
public class DataScopeService : IDataScopeService
{
    #region 字段

    private readonly ISqlSugarClient _db;

    #endregion

    #region 构造函数

    /// <summary>
    /// 
    /// </summary>
    /// <param name="db"></param>
    public DataScopeService(ISqlSugarClient db)
    {
        _db = db;
    }

    #endregion

    #region 基础方法

    /// <summary>
    /// 获取指定用户在数据权限范围内可访问的所有用户账号（用户名）
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    [UseCache(Expiration = 60, KeyPrefix = GlobalConstants.CachePrefix.UserDataScopeById)]
    public async Task<List<string>> GetDataScopeAccountsAsync(long userId)
    {
        List<string> accountList = new List<string>();
        var user = await _db.Queryable<User>().Includes(x => x.Roles).FirstAsync(x => x.Id == userId);
        if (user.IsNull())
        {
            return accountList;
        }

        var isAll = Enumerable.Any(user.Roles, x => x.DataScopeType == DataScopeType.All);

        if (isAll)
        {
            accountList.Add("All");
            return accountList;
        }

        foreach (var role in user.Roles)
        {
            accountList.AddRange(await GetAccounts(role.DataScopeType, role.Id, user.DeptId, user.UserName));
        }

        return Enumerable.ToList(Enumerable.Distinct(accountList));
    }


    /// <summary>
    /// 获取用户账号
    /// </summary>
    /// <param name="dataScopeType"></param>
    /// <param name="roleId"></param>
    /// <param name="deptId"></param>
    /// <param name="account"></param>
    /// <returns></returns>
    private async Task<List<string>> GetAccounts(DataScopeType dataScopeType, long roleId, long deptId,
        string account)
    {
        List<string> accountList = new List<string>();
        switch (dataScopeType)
        {
            case DataScopeType.MySelf:
                accountList.Add(account);
                break;
            case DataScopeType.MyDept:
                {
                    var userList = await _db.Queryable<User>().Where(x => x.DeptId == deptId).ToListAsync();
                    accountList.AddRange(Enumerable.Select(userList, x => x.UserName));
                    break;
                }
            case DataScopeType.MyDeptAndBelow:
                {
                    var deptIds = await GetChildIds([deptId], null);
                    var userList = await _db.Queryable<User>().Where(x => deptIds.Contains(x.DeptId)).ToListAsync();
                    accountList.AddRange(Enumerable.Select(userList, x => x.UserName));
                    break;
                }
            case DataScopeType.Customize:
                {
                    var role = await _db.Queryable<Role>().Includes(x => x.DepartmentList).Where(x => x.Id == roleId)
                        .FirstAsync();
                    if (role.IsNotNull())
                    {
                        var deptIds = role.DepartmentList.Select(x => x.Id).ToList();
                        var userList = await _db.Queryable<User>().Where(x => deptIds.Contains(x.DeptId)).ToListAsync();
                        accountList.AddRange(userList.Select(x => x.UserName));
                    }

                    break;
                }
        }

        return accountList;
    }

    private async Task<List<long>> GetChildIds(List<long> ids, List<long> allIds)
    {
        allIds ??= new List<long>();

        foreach (var id in ids.Where(id => !allIds.Contains(id)))
        {
            allIds.Add(id);
            var list = await _db.Queryable<Department>().Where(x => x.ParentId == id && x.Enabled).ToListAsync();
            if (list.Any())
            {
                await GetChildIds(list.Select(x => x.Id).ToList(), allIds);
            }
        }

        return allIds;
    }

    #endregion
}