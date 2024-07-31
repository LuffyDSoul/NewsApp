using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Volo.Abp.Domain.Repositories;


namespace NewsApp.ApiNews
{
    public class ApiNewsAppService : NewsAppAppService, IApiNewsAppService
    {
        private readonly IApiNews _apiNews;
        public ApiNewsAppService(IApiNews apiNewsAppService)
        {
            _apiNews = apiNewsAppService;
        }
        public async Task<ICollection<ArticuloDto>> Search(string? Search)
        {
            var articulos = await _apiNews.GetArticulosAsync(Search);

            return ManualMapper.MapArticlesToNews(articulos);
        }
    }
}