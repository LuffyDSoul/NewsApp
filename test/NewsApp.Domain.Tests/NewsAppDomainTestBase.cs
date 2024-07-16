using Volo.Abp.Modularity;

namespace NewsApp;

/* Inherit from this class for your domain layer tests. */
public abstract class NewsAppDomainTestBase<TStartupModule> : NewsAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
