using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Domain.Repositories;
using NewsApp.Domain.News;
using NewsApp.Domain.News.Services;
using NewsApp.Domain.News.Repositories;
using NewsApp.Permissions;
using NewsApp.Application; // Add this for MappingHelper

namespace NewsApp.News
{
    /// <summary>
    /// Application service for news-related operations
    /// </summary>
    [Authorize]
    public class NewsAppService : NewsAppAppService, INewsAppService
    {
        private readonly INewsProvider _newsProvider;
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly INewsService _legacyNewsService; // Keep for backward compatibility

        public NewsAppService(
            INewsProvider newsProvider,
            INewsArticleRepository newsArticleRepository,
            INewsService legacyNewsService)
        {
            _newsProvider = newsProvider;
            _newsArticleRepository = newsArticleRepository;
            _legacyNewsService = legacyNewsService;
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<PagedResultDto<NewsArticleDto>> SearchAsync(NewsSearchDto searchDto)
        {
            var articles = await _newsProvider.SearchAsync(
                searchDto.Query,
                searchDto.LanguageCode,
                searchDto.FromDate,
                searchDto.Page,
                searchDto.PageSize);

            var totalCount = articles.Count; // NewsAPI doesn't provide total count in free tier
            
            return new PagedResultDto<NewsArticleDto>(
                totalCount,
                ObjectMapper.Map<IList<NewsArticle>, List<NewsArticleDto>>(articles));
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<PagedResultDto<NewsArticleDto>> GetTopHeadlinesAsync(
            string? category = null,
            string? country = null,
            string language = "en",
            int page = 1,
            int pageSize = 20)
        {
            var articles = await _newsProvider.GetTopHeadlinesAsync(category, country, language, page, pageSize);
            var totalCount = articles.Count;

            return new PagedResultDto<NewsArticleDto>(
                totalCount,
                ObjectMapper.Map<IList<NewsArticle>, List<NewsArticleDto>>(articles));
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<NewsArticleDto> GetAsync(Guid id)
        {
            var article = await _newsArticleRepository.GetAsync(id);
            return ObjectMapper.Map<NewsArticle, NewsArticleDto>(article);
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<PagedResultDto<NewsArticleDto>> GetFromSourcesAsync(
            string sources,
            string language = "en",
            int page = 1,
            int pageSize = 20)
        {
            var articles = await _newsProvider.GetFromSourcesAsync(sources, language, page, pageSize);
            var totalCount = articles.Count;

            return new PagedResultDto<NewsArticleDto>(
                totalCount,
                ObjectMapper.Map<IList<NewsArticle>, List<NewsArticleDto>>(articles));
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<List<NewsSourceDto>> GetSourcesAsync(string? language = null, string? country = null)
        {
            var sources = await _newsProvider.GetSourcesAsync(language, country);
            return ObjectMapper.Map<IList<NewsSource>, List<NewsSourceDto>>(sources);
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<List<NewsArticleDto>> GetLatestAsync(int count = 10, string? languageCode = null)
        {
            var articles = await _newsArticleRepository.GetLatestAsync(count, languageCode);
            return ObjectMapper.Map<List<NewsArticle>, List<NewsArticleDto>>(articles);
        }

        [Authorize(NewsAppPermissions.News.Create)]
        public async Task<NewsArticleDto> CreateAsync(CreateNewsArticleDto input)
        {
            // Check if article already exists by URL
            var urlHash = ComputeUrlHash(input.Url);
            var existingArticle = await _newsArticleRepository.FindByUrlHashAsync(urlHash);
            
            if (existingArticle != null)
            {
                return ObjectMapper.Map<NewsArticle, NewsArticleDto>(existingArticle);
            }

            var article = new NewsArticle(
                GuidGenerator.Create(),
                input.Source,
                input.Title,
                input.Url,
                input.PublishedAt,
                input.LanguageCode,
                input.Description,
                input.UrlToImage,
                input.Content,
                input.Author);

            var createdArticle = await _newsArticleRepository.InsertAsync(article, autoSave: true);
            return ObjectMapper.Map<NewsArticle, NewsArticleDto>(createdArticle);
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<PagedResultDto<NewsArticleDto>> GetBySourceAsync(string source, int skipCount = 0, int maxResultCount = 10)
        {
            var articles = await _newsArticleRepository.GetBySourceAsync(source, skipCount, maxResultCount);
            var totalCount = await _newsArticleRepository.CountAsync(x => x.Source == source);

            return new PagedResultDto<NewsArticleDto>(
                totalCount,
                ObjectMapper.Map<List<NewsArticle>, List<NewsArticleDto>>(articles));
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<PagedResultDto<NewsArticleDto>> SearchLocalAsync(
            string searchText,
            string? languageCode = null,
            int skipCount = 0,
            int maxResultCount = 10)
        {
            var articles = await _newsArticleRepository.SearchAsync(searchText, languageCode, skipCount, maxResultCount);
            
            // For count, we'd need to implement a count method in repository
            var totalCount = articles.Count; // Simplified for now

            return new PagedResultDto<NewsArticleDto>(
                totalCount,
                ObjectMapper.Map<List<NewsArticle>, List<NewsArticleDto>>(articles));
        }

        [Authorize(NewsAppPermissions.News.Default)]
        public async Task<bool> TestConnectionAsync()
        {
            return await _newsProvider.TestConnectionAsync();
        }

        // Legacy method for backward compatibility
        [Obsolete("Use SearchAsync with NewsSearchDto instead")]
        public async Task<ICollection<NewsDto>> Search(string query)
        {
            var legacyResult = await _legacyNewsService.GetNewsAsync(query);
            return ObjectMapper.Map<ICollection<ArticleDto>, ICollection<NewsDto>>(legacyResult);
        }

        private static string ComputeUrlHash(string url)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            var bytes = System.Text.Encoding.UTF8.GetBytes(url.ToLowerInvariant());
            var hash = sha256.ComputeHash(bytes);
            return Convert.ToHexString(hash);
        }
    }
}
