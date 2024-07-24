using System.Collections.Generic;
using System.Threading.Tasks;
using NewsApp.Articulos;

namespace NewsApp.ApiNews
{
    public interface IApiNewsService
    {
        Task<List<ArticuloDto>> GetNewsAsync(string query);
    }
}
