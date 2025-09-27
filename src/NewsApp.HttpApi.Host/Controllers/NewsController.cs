using Microsoft.AspNetCore.Mvc;
using NewsApp.News;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace NewsApp.Controllers
{
    [ApiController]
    [Route("api/news")]
    [Authorize] // Ahora requiere autenticación
    public class NewsController : AbpController
    {
        private readonly INewsAppService _newsAppService;

        public NewsController(INewsAppService newsAppService)
        {
            _newsAppService = newsAppService;
        }

        [HttpPost("search")]
        public async Task<PagedResultDto<NewsArticleDto>> SearchAsync([FromBody] NewsSearchDto searchDto)
        {
            return await _newsAppService.SearchAsync(searchDto);
        }

        [HttpGet("get-top-headlines")]
        public async Task<PagedResultDto<NewsArticleDto>> GetTopHeadlinesAsync(
            [FromQuery] string? category = null,
            [FromQuery] string? country = null,
            [FromQuery] string language = "en",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            return await _newsAppService.GetTopHeadlinesAsync(category, country, language, page, pageSize);
        }

        [HttpGet("{id}")]
        public async Task<NewsArticleDto> GetAsync(Guid id)
        {
            return await _newsAppService.GetAsync(id);
        }

        [HttpGet("get-from-sources")]
        public async Task<PagedResultDto<NewsArticleDto>> GetFromSourcesAsync(
            [FromQuery] string sources,
            [FromQuery] string language = "en",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            return await _newsAppService.GetFromSourcesAsync(sources, language, page, pageSize);
        }

        [HttpGet("get-sources")]
        public async Task<List<NewsSourceDto>> GetSourcesAsync(
            [FromQuery] string? language = null,
            [FromQuery] string? country = null)
        {
            return await _newsAppService.GetSourcesAsync(language, country);
        }

        [HttpGet("get-latest")]
        public async Task<List<NewsArticleDto>> GetLatestAsync(
            [FromQuery] int count = 10,
            [FromQuery] string? languageCode = null)
        {
            return await _newsAppService.GetLatestAsync(count, languageCode);
        }

        [HttpPost("create")]
        public async Task<NewsArticleDto> CreateAsync([FromBody] CreateNewsArticleDto input)
        {
            return await _newsAppService.CreateAsync(input);
        }

        [HttpGet("get-by-source")]
        public async Task<PagedResultDto<NewsArticleDto>> GetBySourceAsync(
            [FromQuery] string source,
            [FromQuery] int skipCount = 0,
            [FromQuery] int maxResultCount = 10)
        {
            return await _newsAppService.GetBySourceAsync(source, skipCount, maxResultCount);
        }

        [HttpGet("search-local")]
        public async Task<PagedResultDto<NewsArticleDto>> SearchLocalAsync(
            [FromQuery] string searchText,
            [FromQuery] string? languageCode = null,
            [FromQuery] int skipCount = 0,
            [FromQuery] int maxResultCount = 10)
        {
            return await _newsAppService.SearchLocalAsync(searchText, languageCode, skipCount, maxResultCount);
        }

        [HttpGet("test-connection")]
        [AllowAnonymous] // Este endpoint sigue siendo público para testing
        public async Task<bool> TestConnectionAsync()
        {
            return await _newsAppService.TestConnectionAsync();
        }
    }
}