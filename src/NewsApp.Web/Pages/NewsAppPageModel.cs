using NewsApp.Localization;
using Volo.Abp.AspNetCore.Mvc.UI.RazorPages;

namespace NewsApp.Web.Pages;

/* Inherit your PageModel classes from this class.
 */
public abstract class NewsAppPageModel : AbpPageModel
{
    protected NewsAppPageModel()
    {
        LocalizationResourceType = typeof(NewsAppResource);
    }
}
