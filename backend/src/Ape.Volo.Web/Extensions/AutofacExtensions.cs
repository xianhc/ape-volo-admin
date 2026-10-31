using Ape.Volo.Core.Aop;
using Ape.Volo.Core.ConfigOptions;
using Ape.Volo.Core.Repository.SugarHandler;
using Ape.Volo.Core.Repository.UnitOfWork;
using Autofac;
using Autofac.Extras.DynamicProxy;
using Castle.DynamicProxy;
using Module = Autofac.Module;

namespace Ape.Volo.Web.Extensions;

/// <summary>
/// autofac注册
/// </summary>
public class AutofacExtensions(IConfiguration configuration) : Module
{
    /// <summary>
    /// Autofac 容器注册入口：批量注册服务、仓储并挂载 AOP 拦截器
    /// </summary>
    /// <param name="builder">容器构建器</param>
    protected override void Load(ContainerBuilder builder)
    {
        var options = configuration.GetConfigurableOptions<AopOptions>();

        // 授权不可随事务/缓存配置关闭，且必须在缓存命中或开启事务之前执行。
        builder.RegisterType<AuthorizationAop>();
        var registerType = new List<Type> { typeof(AuthorizationAop) };
        if (options.Transactions)
        {
            // Autofac.Extras.DynamicProxy 只识别 IInterceptor，异步拦截器要用 Castle 的适配器包一层
            builder.RegisterType<TransactionAop>().As<IAsyncInterceptor>();
            builder.RegisterType<AsyncDeterminationInterceptor>().As<IInterceptor>().AsSelf();
            registerType.Add(typeof(AsyncDeterminationInterceptor));
        }

        if (options.Cache)
        {
            builder.RegisterType<CacheAop>();
            registerType.Add(typeof(CacheAop));
        }

        builder.RegisterGeneric(typeof(SugarRepository<>)).As(typeof(ISugarRepository<>))
            .InstancePerDependency();

        // BaseService 是纯实现基类，不实现任何契约，因此不做泛型注册。
        // 对外能力完全由各业务接口自己声明的方法决定，Autofac 只按业务接口注册。

        //注册业务层（全部走构造函数注入，不再使用属性注入）
        var contracts = typeof(IScheduledJobService).Assembly;
        var applicationTypes = typeof(Ape.Volo.Application.BaseService<>).Assembly.GetExportedTypes()
            .Concat(typeof(Ape.Volo.Core.SystemManagement.ServerInfo.ServerResourcesService).Assembly.GetExportedTypes()
                .Where(type => typeof(IRemoteService).IsAssignableFrom(type)))
            .Where(type => type.IsClass && !type.IsAbstract && type.GetInterfaces().Any(contract => contract.Assembly == contracts))
            .ToArray();
        builder
            .RegisterTypes(applicationTypes)
            .As(type => type.GetInterfaces().Where(contract =>
                contract != typeof(IRemoteService) && contract != typeof(IApplicationService)))
            .InstancePerDependency()
            .EnableInterfaceInterceptors()
            .InterceptedBy(registerType.ToArray());

        //注册工作单元
        builder.RegisterType<UnitOfWork>().As<IUnitOfWork>()
            .AsImplementedInterfaces()
            .InstancePerLifetimeScope();

    }
}
