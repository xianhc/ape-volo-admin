using SqlSugar;

namespace Ape.Volo.Repository.UnitOfWork;

/// <summary>
/// 工作单元接口
/// </summary>
public interface IUnitOfWork
{
    SqlSugarScope GetDbClient();
    void BeginTran();
    void CommitTran();
    void RollbackTran();
}