using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.ObjectMapping;
using NewsAPI.Constants;
using NewsAPI.Models;
using static System.Runtime.InteropServices.JavaScript.JSType;
using AutoMapper.Internal.Mappers;
using NewsApp.ApiNews;
using NewsApp.Articulos;

namespace NewsApp.Busqueda
{
    public class SearchAppService : NewsAppAppService, ISearchAppService
    {
        private readonly IApiNewsService _IApiNewsAppService;
        public SearchAppService(IApiNewsService apiNewsAppService)
        {
            _IApiNewsAppService = apiNewsAppService;

        }

        public async Task<Search> SearchArticulos(string keyword)
        {
            var articulo = await _IApiNewsAppService.Search(keyword);

            return new Search
            {
                Keyword = keyword,
                Articulo = articulo
            };
        }
    }
}
