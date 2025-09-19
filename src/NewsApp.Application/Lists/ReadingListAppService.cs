using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Users;
using Volo.Abp.Guids;
using NewsApp.Domain.Lists;
using NewsApp.Domain.Lists.Repositories;
using NewsApp.Domain.News.Repositories;
using NewsApp.Permissions;
using NewsApp.Application; // Add this for MappingHelper

namespace NewsApp.Lists
{
    /// <summary>
    /// Extension methods for ReadingList entities
    /// </summary>
    public static class ReadingListExtensions
    {
        /// <summary>
        /// Converts ReadingList entity to DTO
        /// </summary>
        public static ReadingListDto ToDto(this ReadingList list)
        {
            return new ReadingListDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                OwnerUserId = list.OwnerUserId,
                ParentId = list.ParentId,
                CreationTime = list.CreationTime,
                ItemCount = list.Items?.Count ?? 0,
                UnreadCount = list.GetUnreadCount(),
                ParentName = list.Parent?.Name,
                OwnerUserName = null // Will be filled by application service if needed
            };
        }

        /// <summary>
        /// Converts ReadingList entity to WithItemsDto
        /// </summary>
        public static ReadingListWithItemsDto ToWithItemsDto(this ReadingList list)
        {
            return new ReadingListWithItemsDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                OwnerUserId = list.OwnerUserId,
                ParentId = list.ParentId,
                CreationTime = list.CreationTime,
                ItemCount = list.Items?.Count ?? 0,
                UnreadCount = list.GetUnreadCount(),
                ParentName = list.Parent?.Name,
                OwnerUserName = null,
                Items = list.Items?.Select(i => i.ToDto()).ToArray() ?? Array.Empty<ReadingListItemDto>(),
                Children = list.Children?.Select(c => c.ToDto()).ToArray() ?? Array.Empty<ReadingListDto>()
            };
        }

        /// <summary>
        /// Converts ReadingList entity to HierarchyDto
        /// </summary>
        public static ReadingListHierarchyDto ToHierarchyDto(this ReadingList list)
        {
            return new ReadingListHierarchyDto
            {
                Id = list.Id,
                Name = list.Name,
                Description = list.Description,
                IsPublic = list.IsPublic,
                OwnerUserId = list.OwnerUserId,
                ParentId = list.ParentId,
                CreationTime = list.CreationTime,
                ItemCount = list.Items?.Count ?? 0,
                UnreadCount = list.GetUnreadCount(),
                ParentName = list.Parent?.Name,
                OwnerUserName = null,
                Level = 0, // Will be set by application service
                Path = Array.Empty<string>(), // Will be set by application service
                Children = Array.Empty<ReadingListHierarchyDto>() // Will be set by application service
            };
        }
    }

    /// <summary>
    /// Extension methods for ReadingListItem entities
    /// </summary>
    public static class ReadingListItemExtensions
    {
        /// <summary>
        /// Converts ReadingListItem entity to DTO
        /// </summary>
        public static ReadingListItemDto ToDto(this ReadingListItem item)
        {
            return new ReadingListItemDto
            {
                Id = item.Id,
                ReadingListId = item.ReadingListId,
                ArticleId = item.ArticleId,
                ArticleTitle = item.ArticleTitle,
                ArticleUrl = item.ArticleUrl,
                ArticleSource = item.ArticleSource,
                ArticlePublishedAt = item.ArticlePublishedAt,
                AddedAt = item.AddedAt,
                IsRead = item.IsRead,
                ReadAt = item.ReadAt,
                Notes = item.Notes,
                Rating = item.Rating
            };
        }
    }

    /// <summary>
    /// Application service for reading list operations
    /// </summary>
    [Authorize]
    public class ReadingListAppService : NewsAppAppService, IReadingListAppService
    {
        private readonly IReadingListRepository _readingListRepository;
        private readonly INewsArticleRepository _newsArticleRepository;
        private readonly ICurrentUser _currentUser;
        private readonly IGuidGenerator _guidGenerator;

        public ReadingListAppService(
            IReadingListRepository readingListRepository,
            INewsArticleRepository newsArticleRepository,
            ICurrentUser currentUser,
            IGuidGenerator guidGenerator)
        {
            _readingListRepository = readingListRepository;
            _newsArticleRepository = newsArticleRepository;
            _currentUser = currentUser;
            _guidGenerator = guidGenerator;
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<List<ReadingListDto>> GetMyListsAsync(bool includeHierarchy = false)
        {
            var userId = _currentUser.GetId();
            var lists = await _readingListRepository.GetByUserIdAsync(userId, includeHierarchy);
            
            return lists.ToDto(l => l.ToDto());
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<List<ReadingListHierarchyDto>> GetMyHierarchicalListsAsync()
        {
            var userId = _currentUser.GetId();
            var topLevelLists = await _readingListRepository.GetTopLevelByUserIdAsync(userId, true, false);
            
            // Convert to hierarchical structure
            var result = new List<ReadingListHierarchyDto>();
            foreach (var list in topLevelLists)
            {
                var hierarchyDto = BuildHierarchy(list, 0, new List<string>());
                result.Add(hierarchyDto);
            }
            
            return result;
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<ReadingListWithItemsDto> GetWithItemsAsync(Guid id, bool includeItems = true)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetWithItemsAsync(id, userId);
            
            if (list == null)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            return list.ToWithItemsDto();
        }

        [Authorize(NewsAppPermissions.Lists.Create)]
        public async Task<ReadingListDto> CreateAsync(CreateReadingListDto input)
        {
            var userId = _currentUser.GetId();
            
            // Validate parent if specified
            if (input.ParentId.HasValue)
            {
                var parent = await _readingListRepository.GetAsync(input.ParentId.Value);
                if (parent.OwnerUserId != userId)
                    throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot create list under a parent you don't own");
            }

            var list = new ReadingList(
                _guidGenerator.Create(),
                input.Name,
                userId,
                input.ParentId,
                input.Description,
                input.IsPublic);

            var createdList = await _readingListRepository.InsertAsync(list, autoSave: true);
            return createdList.ToDto();
        }

        [Authorize(NewsAppPermissions.Lists.Edit)]
        public async Task<ReadingListDto> UpdateAsync(Guid id, UpdateReadingListDto input)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetAsync(id);
            
            if (list.OwnerUserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot update a list you don't own");

            // Validate new parent if specified
            if (input.ParentId.HasValue && input.ParentId != list.ParentId)
            {
                var newParent = await _readingListRepository.GetAsync(input.ParentId.Value);
                if (newParent.OwnerUserId != userId)
                    throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot set parent to a list you don't own");
                
                if (!newParent.CanBeParentOf(list))
                    throw new InvalidOperationException("Cannot create circular reference in list hierarchy");
            }

            list.SetBasicInfo(input.Name, userId, input.ParentId, input.Description, input.IsPublic);
            
            var updatedList = await _readingListRepository.UpdateAsync(list, autoSave: true);
            return updatedList.ToDto();
        }

        [Authorize(NewsAppPermissions.Lists.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetAsync(id);
            
            if (list.OwnerUserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot delete a list you don't own");

            // Check if there are child lists
            var children = await _readingListRepository.GetChildrenAsync(id);
            if (children.Any())
                throw new InvalidOperationException("Cannot delete a list that has child lists. Delete or move child lists first.");

            await _readingListRepository.DeleteAsync(id);
        }

        [Authorize(NewsAppPermissions.Lists.Manage)]
        public async Task<ReadingListItemDto> AddArticleAsync(Guid listId, AddArticleToListDto input)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetWithItemsAsync(listId, userId);
            
            if (list == null)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            var article = await _newsArticleRepository.GetAsync(input.ArticleId);
            
            var item = list.AddArticle(article.Id, article.Title, article.Url, article.Source, article.PublishedAt);
            if (!string.IsNullOrEmpty(input.Notes))
            {
                item.SetNotes(input.Notes);
            }

            await _readingListRepository.UpdateAsync(list, autoSave: true);
            return item.ToDto();
        }

        [Authorize(NewsAppPermissions.Lists.Manage)]
        public async Task RemoveArticleAsync(Guid listId, Guid articleId)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetWithItemsAsync(listId, userId);
            
            if (list == null)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            list.RemoveArticle(articleId);
            await _readingListRepository.UpdateAsync(list, autoSave: true);
        }

        [Authorize(NewsAppPermissions.Lists.Manage)]
        public async Task MarkAsReadAsync(Guid listId, Guid articleId)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetWithItemsAsync(listId, userId);
            
            if (list == null)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            list.MarkArticleAsRead(articleId);
            await _readingListRepository.UpdateAsync(list, autoSave: true);
        }

        [Authorize(NewsAppPermissions.Lists.Manage)]
        public async Task MarkAsUnreadAsync(Guid listId, Guid articleId)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetWithItemsAsync(listId, userId);
            
            if (list == null)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            list.MarkArticleAsUnread(articleId);
            await _readingListRepository.UpdateAsync(list, autoSave: true);
        }

        [Authorize(NewsAppPermissions.Lists.Manage)]
        public async Task<ReadingListItemDto> UpdateItemAsync(Guid listId, Guid itemId, UpdateReadingListItemDto input)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetWithItemsAsync(listId, userId);
            
            if (list == null)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            var item = list.Items.FirstOrDefault(i => i.Id == itemId);
            if (item == null)
                throw new InvalidOperationException("Item not found in this list");

            if (input.IsRead)
                item.MarkAsRead();
            else
                item.MarkAsUnread();

            if (input.Notes != null)
                item.SetNotes(input.Notes);

            if (input.Rating.HasValue)
                item.SetRating(input.Rating.Value);

            await _readingListRepository.UpdateAsync(list, autoSave: true);
            return item.ToDto();
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<List<ReadingListDto>> GetChildrenAsync(Guid parentId)
        {
            var userId = _currentUser.GetId();
            
            // Verify user has access to parent
            var parent = await _readingListRepository.GetAsync(parentId);
            if (parent.OwnerUserId != userId && !parent.IsPublic)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            var children = await _readingListRepository.GetChildrenAsync(parentId);
            return children.ToDto(c => c.ToDto());
        }

        [Authorize(NewsAppPermissions.Lists.Manage)]
        public async Task MoveListAsync(Guid listId, Guid? newParentId)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetAsync(listId);
            
            if (list.OwnerUserId != userId)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot move a list you don't own");

            if (newParentId.HasValue)
            {
                var newParent = await _readingListRepository.GetAsync(newParentId.Value);
                if (newParent.OwnerUserId != userId)
                    throw new Volo.Abp.Authorization.AbpAuthorizationException("Cannot move to a parent you don't own");
                
                if (!newParent.CanBeParentOf(list))
                    throw new InvalidOperationException("Cannot create circular reference in list hierarchy");
            }

            list.ParentId = newParentId;
            await _readingListRepository.UpdateAsync(list, autoSave: true);
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<List<ReadingListDto>> GetListsContainingArticleAsync(Guid articleId)
        {
            var userId = _currentUser.GetId();
            var lists = await _readingListRepository.FindListsContainingArticleAsync(articleId, userId);
            return lists.ToDto(l => l.ToDto());
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<List<ReadingListDto>> SearchByNameAsync(string namePattern)
        {
            var userId = _currentUser.GetId();
            var lists = await _readingListRepository.GetByNamePatternAsync(namePattern, userId);
            return lists.ToDto(l => l.ToDto());
        }

        [Authorize(NewsAppPermissions.Lists.ViewPublic)]
        public async Task<PagedResultDto<ReadingListDto>> GetPublicListsAsync(int skipCount = 0, int maxResultCount = 10)
        {
            var lists = await _readingListRepository.GetPublicListsAsync(skipCount, maxResultCount);
            var totalCount = await _readingListRepository.GetCountAsync();
            
            return new PagedResultDto<ReadingListDto>(
                totalCount,
                lists.ToDto(l => l.ToDto()));
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<List<ReadingListDto>> GetRecentlyUpdatedAsync(int count = 10)
        {
            var userId = _currentUser.GetId();
            var lists = await _readingListRepository.GetRecentlyUpdatedAsync(userId, count);
            return lists.ToDto(l => l.ToDto());
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<List<ReadingListDto>> GetHierarchyPathAsync(Guid listId)
        {
            var userId = _currentUser.GetId();
            
            // Verify user has access
            var list = await _readingListRepository.GetAsync(listId);
            if (list.OwnerUserId != userId && !list.IsPublic)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            var path = await _readingListRepository.GetHierarchyPathAsync(listId);
            return path.ToDto(p => p.ToDto());
        }

        [Authorize(NewsAppPermissions.Lists.Manage)]
        public async Task<int> ImportArticlesAsync(Guid listId, string importData)
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetWithItemsAsync(listId, userId);
            
            if (list == null)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            // Simple implementation - assume importData contains URLs separated by newlines
            var urls = importData.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                                 .Select(url => url.Trim())
                                 .Where(url => Uri.IsWellFormedUriString(url, UriKind.Absolute))
                                 .ToList();

            var importedCount = 0;
            foreach (var url in urls)
            {
                try
                {
                    // Try to find existing article by URL
                    var existingArticles = await _newsArticleRepository.GetByUrlsAsync(new List<string> { url });
                    var article = existingArticles.FirstOrDefault();
                    
                    if (article != null)
                    {
                        // Check if not already in list
                        if (!list.Items.Any(i => i.ArticleUrl == url))
                        {
                            list.AddArticle(article.Id, article.Title, article.Url, article.Source, article.PublishedAt);
                            importedCount++;
                        }
                    }
                    // If article doesn't exist, we could create a placeholder or skip
                }
                catch
                {
                    // Skip invalid URLs or articles
                }
            }

            if (importedCount > 0)
            {
                await _readingListRepository.UpdateAsync(list, autoSave: true);
            }

            return importedCount;
        }

        [Authorize(NewsAppPermissions.Lists.Default)]
        public async Task<string> ExportListAsync(Guid listId, string format = "json")
        {
            var userId = _currentUser.GetId();
            var list = await _readingListRepository.GetWithItemsAsync(listId, userId);
            
            if (list == null)
                throw new Volo.Abp.Authorization.AbpAuthorizationException("Access denied to this reading list");

            return format.ToLower() switch
            {
                "json" => System.Text.Json.JsonSerializer.Serialize(list.ToWithItemsDto()),
                "csv" => ExportToCsv(list),
                "opml" => ExportToOpml(list),
                _ => throw new ArgumentException("Unsupported export format")
            };
        }

        private ReadingListHierarchyDto BuildHierarchy(ReadingList list, int level, List<string> path)
        {
            var newPath = new List<string>(path) { list.Name };
            var dto = list.ToHierarchyDto();
            dto.Level = level;
            dto.Path = newPath.ToArray();
            
            if (list.Children?.Any() == true)
            {
                dto.Children = list.Children.Select(child => BuildHierarchy(child, level + 1, newPath)).ToArray();
            }
            
            return dto;
        }

        private static string ExportToCsv(ReadingList list)
        {
            var csv = new System.Text.StringBuilder();
            csv.AppendLine("Title,URL,Source,Published At,Added At,Is Read,Notes,Rating");
            
            foreach (var item in list.Items)
            {
                csv.AppendLine($"\"{item.ArticleTitle}\",\"{item.ArticleUrl}\",\"{item.ArticleSource}\",\"{item.ArticlePublishedAt:yyyy-MM-dd}\",\"{item.AddedAt:yyyy-MM-dd}\",{item.IsRead},\"{item.Notes}\",{item.Rating}");
            }
            
            return csv.ToString();
        }

        private static string ExportToOpml(ReadingList list)
        {
            // Simplified OPML export
            var opml = new System.Text.StringBuilder();
            opml.AppendLine("<?xml version=\"1.0\" encoding=\"UTF-8\"?>");
            opml.AppendLine("<opml version=\"2.0\">");
            opml.AppendLine("<head>");
            opml.AppendLine($"<title>{list.Name}</title>");
            opml.AppendLine("</head>");
            opml.AppendLine("<body>");
            
            foreach (var item in list.Items)
            {
                opml.AppendLine($"<outline text=\"{item.ArticleTitle}\" xmlUrl=\"{item.ArticleUrl}\" />");
            }
            
            opml.AppendLine("</body>");
            opml.AppendLine("</opml>");
            
            return opml.ToString();
        }
    }
}
