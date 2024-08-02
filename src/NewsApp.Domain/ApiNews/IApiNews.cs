using System.Collections.Generic;
using System.Threading.Tasks;


namespace NewsApp.ApiNews;

public interface IApiNews
{
    Task<ICollection<ArticuloDto>> GetArticulosAsync(string? Search);
}
