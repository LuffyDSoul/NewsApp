using NewsApp.Samples;
using Xunit;

namespace NewsApp.EntityFrameworkCore.Applications;

[Collection(NewsAppTestConsts.CollectionDefinitionName)]
public class EfCoreSampleAppServiceTests : SampleAppServiceTests<NewsAppEntityFrameworkCoreTestModule>
{

}
