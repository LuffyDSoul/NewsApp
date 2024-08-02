using NewsApp.ApiNews;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace NewsApp.Busqueda
{
    public class SearchAppService : NewsAppAppService, ISearchAppService
    {
        private readonly IApiNewsAppService _IApiNewsAppService;
        public SearchAppService(IApiNewsAppService apiNewsAppService)
        {
            _IApiNewsAppService = apiNewsAppService;
        }
        public async Task<Search> SearchArticulo(string keyword)
        {
            var news = await _IApiNewsAppService.Search(keyword);
            return new Search
            {
                Keyword = keyword,
                Articulos = news
            };


        }
    }
}
