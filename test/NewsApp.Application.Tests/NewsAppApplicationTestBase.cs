using Volo.Abp.Modularity;

namespace NewsApp;

public abstract class NewsAppApplicationTestBase<TStartupModule> : NewsAppTestBase<TStartupModule>
    where TStartupModule : IAbpModule
{

}
