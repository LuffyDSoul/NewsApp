using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Users;

namespace NewsApp.ReadingLists
{
    [Authorize]
    public class SavedArticleAppService : ApplicationService, ISavedArticleAppService
    {
        private readonly ISavedArticleRepository _savedArticleRepository;
        private readonly IReadingListRepository _readingListRepository;

        public SavedArticleAppService(
            ISavedArticleRepository savedArticleRepository,
            IReadingListRepository readingListRepository)
        {
            _savedArticleRepository = savedArticleRepository;
            _readingListRepository = readingListRepository;
        }

        public async Task<List<SavedArticleDto>> GetMySavedArticlesAsync(
            Guid? readingListId = null, 
            bool onlyUnread = false, 
            int maxCount = 100)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticles = await _savedArticleRepository.GetUserSavedArticlesAsync(
                currentUserId, readingListId, onlyUnread, maxCount);

            return savedArticles.Select(a => new SavedArticleDto
            {
                Id = a.Id,
                ReadingListId = a.ReadingListId,
                ReadingListName = a.ReadingList?.Name,
                Source = a.Source,
                Title = a.Title,
                Description = a.Description,
                Url = a.Url,
                UrlToImage = a.UrlToImage,
                PublishedAt = a.PublishedAt,
                Content = a.Content,
                LanguageCode = a.LanguageCode,
                Author = a.Author,
                SavedAt = a.SavedAt,
                IsRead = a.IsRead,
                Notes = a.Notes,
                Tags = a.Tags
            }).ToList();
        }

        public async Task<SavedArticleDto> GetSavedArticleAsync(Guid id)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticle = await _savedArticleRepository.GetAsync(id);

            if (savedArticle.UserId != currentUserId)
            {
                throw new UserFriendlyException("You can only access your own saved articles.");
            }

            return new SavedArticleDto
            {
                Id = savedArticle.Id,
                ReadingListId = savedArticle.ReadingListId,
                ReadingListName = savedArticle.ReadingList?.Name,
                Source = savedArticle.Source,
                Title = savedArticle.Title,
                Description = savedArticle.Description,
                Url = savedArticle.Url,
                UrlToImage = savedArticle.UrlToImage,
                PublishedAt = savedArticle.PublishedAt,
                Content = savedArticle.Content,
                LanguageCode = savedArticle.LanguageCode,
                Author = savedArticle.Author,
                SavedAt = savedArticle.SavedAt,
                IsRead = savedArticle.IsRead,
                Notes = savedArticle.Notes,
                Tags = savedArticle.Tags
            };
        }

        public async Task<bool> IsArticleSavedAsync(string url)
        {
            var currentUserId = CurrentUser.GetId();
            return await _savedArticleRepository.IsArticleSavedByUserAsync(currentUserId, url);
        }

        public async Task<SavedArticleDto> SaveArticleAsync(SaveArticleDto input)
        {
            var currentUserId = CurrentUser.GetId();

            // If saving to a specific reading list, check if article is already in THAT list
            if (input.ReadingListId.HasValue)
            {
                // Check if article already exists in this specific list using FindAsync (returns null if not found)
                var existingInList = await _savedArticleRepository.FindAsync(
                    a => a.UserId == currentUserId && 
                         a.Url == input.Url && 
                         a.ReadingListId == input.ReadingListId.Value);
                
                if (existingInList != null)
                {
                    throw new UserFriendlyException("This article is already in the selected reading list.");
                }

                // Validate reading list ownership
                var ownsReadingList = await _readingListRepository.UserOwnsListAsync(currentUserId, input.ReadingListId.Value);
                if (!ownsReadingList)
                {
                    throw new UserFriendlyException("You can only save articles to your own reading lists.");
                }
            }
            else
            {
                // If not saving to a specific list, check if article exists without a list (uncategorized)
                var existingArticle = await _savedArticleRepository.FindAsync(
                    a => a.UserId == currentUserId && 
                         a.Url == input.Url && 
                         a.ReadingListId == null);
                
                if (existingArticle != null)
                {
                    throw new UserFriendlyException("This article is already saved in your uncategorized articles.");
                }
            }

            var savedArticle = new SavedArticle(
                currentUserId,
                input.Title,
                input.Url,
                input.Source,
                input.Description,
                input.UrlToImage,
                input.PublishedAt,
                input.Content,
                input.Author,
                input.ReadingListId)
            {
                LanguageCode = input.LanguageCode,
                Notes = input.Notes,
                Tags = input.Tags
            };

            await _savedArticleRepository.InsertAsync(savedArticle, autoSave: true);

            return new SavedArticleDto
            {
                Id = savedArticle.Id,
                ReadingListId = savedArticle.ReadingListId,
                Source = savedArticle.Source,
                Title = savedArticle.Title,
                Description = savedArticle.Description,
                Url = savedArticle.Url,
                UrlToImage = savedArticle.UrlToImage,
                PublishedAt = savedArticle.PublishedAt,
                Content = savedArticle.Content,
                LanguageCode = savedArticle.LanguageCode,
                Author = savedArticle.Author,
                SavedAt = savedArticle.SavedAt,
                IsRead = savedArticle.IsRead,
                Notes = savedArticle.Notes,
                Tags = savedArticle.Tags
            };
        }

        public async Task<SavedArticleDto> UpdateSavedArticleAsync(Guid id, UpdateSavedArticleDto input)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticle = await _savedArticleRepository.GetAsync(id);

            if (savedArticle.UserId != currentUserId)
            {
                throw new UserFriendlyException("You can only update your own saved articles.");
            }

            // Validate reading list if provided
            if (input.ReadingListId.HasValue)
            {
                var ownsReadingList = await _readingListRepository.UserOwnsListAsync(currentUserId, input.ReadingListId.Value);
                if (!ownsReadingList)
                {
                    throw new UserFriendlyException("You can only move articles to your own reading lists.");
                }
            }

            savedArticle.ReadingListId = input.ReadingListId;
            savedArticle.IsRead = input.IsRead;
            savedArticle.Notes = input.Notes;
            savedArticle.Tags = input.Tags;

            await _savedArticleRepository.UpdateAsync(savedArticle, autoSave: true);

            return new SavedArticleDto
            {
                Id = savedArticle.Id,
                ReadingListId = savedArticle.ReadingListId,
                ReadingListName = savedArticle.ReadingList?.Name,
                Source = savedArticle.Source,
                Title = savedArticle.Title,
                Description = savedArticle.Description,
                Url = savedArticle.Url,
                UrlToImage = savedArticle.UrlToImage,
                PublishedAt = savedArticle.PublishedAt,
                Content = savedArticle.Content,
                LanguageCode = savedArticle.LanguageCode,
                Author = savedArticle.Author,
                SavedAt = savedArticle.SavedAt,
                IsRead = savedArticle.IsRead,
                Notes = savedArticle.Notes,
                Tags = savedArticle.Tags
            };
        }

        public async Task UnsaveArticleAsync(Guid id)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticle = await _savedArticleRepository.GetAsync(id);

            if (savedArticle.UserId != currentUserId)
            {
                throw new UserFriendlyException("You can only delete your own saved articles.");
            }

            await _savedArticleRepository.DeleteAsync(savedArticle, autoSave: true);
        }

        public async Task UnsaveArticleByUrlAsync(string url)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticle = await _savedArticleRepository.GetUserSavedArticleByUrlAsync(currentUserId, url);

            if (savedArticle != null)
            {
                await _savedArticleRepository.DeleteAsync(savedArticle, autoSave: true);
            }
        }

        public async Task MarkAsReadAsync(Guid id)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticle = await _savedArticleRepository.GetAsync(id);

            if (savedArticle.UserId != currentUserId)
            {
                throw new UserFriendlyException("You can only update your own saved articles.");
            }

            savedArticle.IsRead = true;
            await _savedArticleRepository.UpdateAsync(savedArticle, autoSave: true);
        }

        public async Task MarkAsUnreadAsync(Guid id)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticle = await _savedArticleRepository.GetAsync(id);

            if (savedArticle.UserId != currentUserId)
            {
                throw new UserFriendlyException("You can only update your own saved articles.");
            }

            savedArticle.IsRead = false;
            await _savedArticleRepository.UpdateAsync(savedArticle, autoSave: true);
        }

        public async Task MoveToReadingListAsync(Guid articleId, Guid? readingListId)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticle = await _savedArticleRepository.GetAsync(articleId);

            if (savedArticle.UserId != currentUserId)
            {
                throw new UserFriendlyException("You can only move your own saved articles.");
            }

            // Validate reading list if provided
            if (readingListId.HasValue)
            {
                var ownsReadingList = await _readingListRepository.UserOwnsListAsync(currentUserId, readingListId.Value);
                if (!ownsReadingList)
                {
                    throw new UserFriendlyException("You can only move articles to your own reading lists.");
                }
            }

            savedArticle.ReadingListId = readingListId;
            await _savedArticleRepository.UpdateAsync(savedArticle, autoSave: true);
        }

        public async Task BulkUpdateSavedArticlesAsync(BulkUpdateSavedArticlesDto input)
        {
            var currentUserId = CurrentUser.GetId();

            foreach (var articleId in input.ArticleIds)
            {
                var savedArticle = await _savedArticleRepository.GetAsync(articleId);

                if (savedArticle.UserId != currentUserId)
                {
                    continue; // Skip articles that don't belong to the current user
                }

                var updated = false;

                if (input.MarkAsRead.HasValue)
                {
                    savedArticle.IsRead = input.MarkAsRead.Value;
                    updated = true;
                }

                if (input.MoveToReadingListId.HasValue)
                {
                    // Validate reading list
                    var ownsReadingList = await _readingListRepository.UserOwnsListAsync(currentUserId, input.MoveToReadingListId.Value);
                    if (ownsReadingList)
                    {
                        savedArticle.ReadingListId = input.MoveToReadingListId.Value;
                        updated = true;
                    }
                }

                if (!string.IsNullOrWhiteSpace(input.AddTags))
                {
                    var currentTags = string.IsNullOrWhiteSpace(savedArticle.Tags) 
                        ? new List<string>() 
                        : savedArticle.Tags.Split(',').Select(t => t.Trim()).ToList();
                    
                    var newTags = input.AddTags.Split(',').Select(t => t.Trim()).Where(t => !string.IsNullOrWhiteSpace(t));
                    currentTags.AddRange(newTags.Where(t => !currentTags.Contains(t)));
                    
                    savedArticle.Tags = string.Join(", ", currentTags);
                    updated = true;
                }

                if (!string.IsNullOrWhiteSpace(input.RemoveTags))
                {
                    if (!string.IsNullOrWhiteSpace(savedArticle.Tags))
                    {
                        var currentTags = savedArticle.Tags.Split(',').Select(t => t.Trim()).ToList();
                        var tagsToRemove = input.RemoveTags.Split(',').Select(t => t.Trim());
                        
                        foreach (var tagToRemove in tagsToRemove)
                        {
                            currentTags.Remove(tagToRemove);
                        }
                        
                        savedArticle.Tags = string.Join(", ", currentTags);
                        updated = true;
                    }
                }

                if (updated)
                {
                    await _savedArticleRepository.UpdateAsync(savedArticle);
                }
            }

            await CurrentUnitOfWork.SaveChangesAsync();
        }

        public async Task<SavedArticleStatsDto> GetSavedArticleStatsAsync()
        {
            var currentUserId = CurrentUser.GetId();
            var totalCount = await _savedArticleRepository.GetUserSavedArticlesCountAsync(currentUserId);
            var unreadCount = await _savedArticleRepository.GetUserSavedArticlesCountAsync(currentUserId, onlyUnread: true);

            var oneWeekAgo = DateTime.UtcNow.AddDays(-7);
            var allArticles = await _savedArticleRepository.GetUserSavedArticlesAsync(currentUserId, maxCount: int.MaxValue);
            
            var savedThisWeekCount = allArticles.Count(a => a.SavedAt >= oneWeekAgo);
            var readThisWeekCount = allArticles.Count(a => a.IsRead && a.SavedAt >= oneWeekAgo);

            return new SavedArticleStatsDto
            {
                TotalCount = totalCount,
                UnreadCount = unreadCount,
                SavedThisWeekCount = savedThisWeekCount,
                ReadThisWeekCount = readThisWeekCount
            };
        }

        public async Task<List<Guid>> GetReadingListIdsForArticleAsync(string url)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticles = await _savedArticleRepository.GetListAsync(
                x => x.UserId == currentUserId && x.Url == url
            );
            
            return savedArticles
                .Where(a => a.ReadingListId.HasValue)
                .Select(a => a.ReadingListId.Value)
                .ToList();
        }

        public async Task UnsaveArticleByUrlAndListAsync(string url, Guid readingListId)
        {
            var currentUserId = CurrentUser.GetId();
            var savedArticle = await _savedArticleRepository.FindAsync(
                x => x.UserId == currentUserId && x.Url == url && x.ReadingListId == readingListId
            );

            if (savedArticle != null)
            {
                await _savedArticleRepository.DeleteAsync(savedArticle, autoSave: true);
            }
        }
    }
}