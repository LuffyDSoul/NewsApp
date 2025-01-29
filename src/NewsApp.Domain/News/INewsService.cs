using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.News
{
    public interface INewsService
    {
        Task<ICollection<ArticleDto>> GetNewsAsync(string query);
    }
}
