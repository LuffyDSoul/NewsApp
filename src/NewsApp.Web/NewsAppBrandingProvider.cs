using Volo.Abp.Ui.Branding;
using Volo.Abp.DependencyInjection;

namespace NewsApp.Web;

[Dependency(ReplaceServices = true)]
public class NewsAppBrandingProvider : DefaultBrandingProvider
{
    public override string AppName => "NewsApp";
}
