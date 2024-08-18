using System.Collections.Generic;
using System.Threading.Tasks;
using NewsApp.Articulos;

namespace NewsApp.ApiNews
{
    public interface IApiNews
    {
        Task<ICollection<NewsArticleDto>> GetArticulosAsync(string? Search);
    }
}
