using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NewsApp.News;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;

namespace NewsApp.Controllers
{
    [Area(NewsAppRemoteServiceConsts.ModuleName)]
    [RemoteService(Name = NewsAppRemoteServiceConsts.RemoteServiceName)]
    [Route("api/app/news")]
    public class NewsController : AbpController, INewsAppService
    {
        private readonly INewsAppService _newsAppService;

        public NewsController(INewsAppService newsAppService)
        {
            _newsAppService = newsAppService;
        }

        [HttpPost("search")]
        public virtual Task<PagedResultDto<NewsArticleDto>> SearchAsync(NewsSearchDto searchDto)
        {
            return _newsAppService.SearchAsync(searchDto);
        }

        [HttpGet("top-headlines")]
        [AllowAnonymous] // Temporal para pruebas
        public virtual Task<PagedResultDto<NewsArticleDto>> GetTopHeadlinesAsync(
            string? category = null,
            string? country = null,
            string language = "en",
            int page = 1,
            int pageSize = 20)
        {
            return _newsAppService.GetTopHeadlinesAsync(category, country, language, page, pageSize);
        }

        [HttpGet("{id}")]
        public virtual Task<NewsArticleDto> GetAsync(Guid id)
        {
            return _newsAppService.GetAsync(id);
        }

        [HttpGet("from-sources")]
        [AllowAnonymous] // Temporal para pruebas
        public virtual Task<PagedResultDto<NewsArticleDto>> GetFromSourcesAsync(
            string sources,
            string language = "en",
            int page = 1,
            int pageSize = 20)
        {
            return _newsAppService.GetFromSourcesAsync(sources, language, page, pageSize);
        }

        [HttpGet("sources")]
        [AllowAnonymous] // Temporal para pruebas
        public virtual Task<List<NewsSourceDto>> GetSourcesAsync(string? language = null, string? country = null)
        {
            return _newsAppService.GetSourcesAsync(language, country);
        }

        [HttpGet("latest")]
        [AllowAnonymous] // Temporal para pruebas
        public virtual Task<List<NewsArticleDto>> GetLatestAsync(int count = 10, string? languageCode = null)
        {
            return _newsAppService.GetLatestAsync(count, languageCode);
        }

        [HttpPost]
        public virtual Task<NewsArticleDto> CreateAsync(CreateNewsArticleDto input)
        {
            return _newsAppService.CreateAsync(input);
        }

        [HttpGet("by-source")]
        [AllowAnonymous] // Temporal para pruebas
        public virtual Task<PagedResultDto<NewsArticleDto>> GetBySourceAsync(string source, int skipCount = 0, int maxResultCount = 10)
        {
            return _newsAppService.GetBySourceAsync(source, skipCount, maxResultCount);
        }

        [HttpGet("search-local")]
        [AllowAnonymous] // Temporal para pruebas
        public virtual Task<PagedResultDto<NewsArticleDto>> SearchLocalAsync(
            string searchText,
            string? languageCode = null,
            int skipCount = 0,
            int maxResultCount = 10)
        {
            return _newsAppService.SearchLocalAsync(searchText, languageCode, skipCount, maxResultCount);
        }

        [HttpGet("test-connection")]
        [AllowAnonymous] // Temporal para pruebas
        public virtual Task<bool> TestConnectionAsync()
        {
            return _newsAppService.TestConnectionAsync();
        }

        [HttpGet("search-legacy")]
        [AllowAnonymous] // Temporal para pruebas
        [Obsolete("Use SearchAsync with NewsSearchDto instead")]
        public virtual Task<ICollection<NewsDto>> Search(string query)
        {
            return _newsAppService.Search(query);
        }
    }
}