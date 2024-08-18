using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.ApiNews
{
    public interface IApiNewsService
    {
        Task<ICollection<NewsArticleDto>> Search(string? Search);
        Task<ICollection<NewsArticleDto>> SearchFromDate(string? Search, DateTime date);

    }
}
