using Xunit;

namespace NewsApp.EntityFrameworkCore;

[CollectionDefinition(NewsAppTestConsts.CollectionDefinitionName)]
public class NewsAppEntityFrameworkCoreCollection : ICollectionFixture<NewsAppEntityFrameworkCoreFixture>
{

}
