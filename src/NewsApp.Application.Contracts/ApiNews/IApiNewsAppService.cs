using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.ApiNews
{
    public interface IApiNewsAppService
    {
        Task<ICollection<ArticuloDto>> Search(string? Search);

    }
}
