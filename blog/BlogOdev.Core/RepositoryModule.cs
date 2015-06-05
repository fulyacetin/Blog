
#region Usings
using FluentNHibernate.Cfg;
using FluentNHibernate.Cfg.Db;
using BlogOdev.Core.Objects;
using NHibernate;
using NHibernate.Cache;
using NHibernate.Tool.hbm2ddl;
using Ninject;
using Ninject.Modules;
using Ninject.Web.Common;
#endregion

namespace BlogOdev.Core
{
  /// <summary>
  /// Contains the bindings for db components.
  /// </summary>
  public class RepositoryModule: NinjectModule
  {
    public override void Load()
    {
        var rr = MsSqlConfiguration.MsSql2008.ConnectionString(c => c.FromConnectionStringWithKey("DefaultConnection"));

      Bind<ISessionFactory>()
        .ToMethod(e => Fluently.Configure()
        .Database(rr)
        .Cache(c => c.UseQueryCache().ProviderClass<HashtableCacheProvider>())
        .Mappings(m => m.FluentMappings.AddFromAssemblyOf<Post>())
        //.ExposeConfiguration(cfg => new SchemaExport(cfg).Execute(true, true, false))
        .BuildConfiguration()
        .BuildSessionFactory())
        .InSingletonScope();

      Bind<ISession>()
        .ToMethod((ctx) => ctx.Kernel.Get<ISessionFactory>().OpenSession())
        .InRequestScope();
    }
  }
}
