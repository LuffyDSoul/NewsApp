using NewsApp.Articulos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.ApiNews
{
    public interface IApiNewsService
    {
        Task<ICollection<ArticuloDto>> Search(string? Search);

    }
}
