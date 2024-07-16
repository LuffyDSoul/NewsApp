using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace NewsApp.Pages;

public class Index_Tests : NewsAppWebTestBase
{
    [Fact]
    public async Task Welcome_Page()
    {
        var response = await GetResponseAsStringAsync("/");
        response.ShouldNotBeNull();
    }
}
