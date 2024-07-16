using Volo.Abp.Modularity;

namespace NewsApp;

[DependsOn(
    typeof(NewsAppDomainModule),
    typeof(NewsAppTestBaseModule)
)]
public class NewsAppDomainTestModule : AbpModule
{

}
