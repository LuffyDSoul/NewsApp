using NewsApp.Samples;
using Xunit;

namespace NewsApp.EntityFrameworkCore.Domains;

[Collection(NewsAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleDomainTests : SampleDomainTests<NewsAppEntityFrameworkCoreTestModule>
{

}
