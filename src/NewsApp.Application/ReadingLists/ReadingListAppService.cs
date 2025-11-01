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
    public class ReadingListAppService : ApplicationService, IReadingListAppService
    {
        private readonly IReadingListRepository _readingListRepository;
        private readonly ISavedArticleRepository _savedArticleRepository;

        public ReadingListAppService(
            IReadingListRepository readingListRepository,
            ISavedArticleRepository savedArticleRepository)
        {
            _readingListRepository = readingListRepository;
            _savedArticleRepository = savedArticleRepository;
        }

        public async Task<List<ReadingListDto>> GetMyReadingListsAsync()
        {
            var currentUserId = CurrentUser.GetId();
            var readingLists = await _readingListRepository.GetUserReadingListsAsync(currentUserId, includeArticles: true);
            
            return readingLists.Select(rl => new ReadingListDto
            {
                Id = rl.Id,
                Name = rl.Name,
                Description = rl.Description,
                IsPublic = rl.IsPublic,
                Color = rl.Color,
                SortOrder = rl.SortOrder,
                CreatedAt = rl.CreatedAt,
                UpdatedAt = rl.UpdatedAt,
                ArticleCount = rl.SavedArticles?.Count ?? 0,
                UnreadCount = rl.SavedArticles?.Count(a => !a.IsRead) ?? 0
            }).ToList();
        }

        public async Task<ReadingListWithArticlesDto> GetReadingListWithArticlesAsync(Guid id)
        {
            var currentUserId = CurrentUser.GetId();
            var readingList = await _readingListRepository.GetAsync(id);

            if (readingList.UserId != currentUserId && !readingList.IsPublic)
            {
                throw new UserFriendlyException("You don't have permission to access this reading list.");
            }

            var articles = await _savedArticleRepository.GetArticlesByReadingListAsync(id);

            return new ReadingListWithArticlesDto
            {
                Id = readingList.Id,
                Name = readingList.Name,
                Description = readingList.Description,
                IsPublic = readingList.IsPublic,
                Color = readingList.Color,
                SortOrder = readingList.SortOrder,
                CreatedAt = readingList.CreatedAt,
                UpdatedAt = readingList.UpdatedAt,
                ArticleCount = articles.Count,
                UnreadCount = articles.Count(a => !a.IsRead),
                Articles = articles.Select(a => new SavedArticleDto
                {
                    Id = a.Id,
                    ReadingListId = a.ReadingListId,
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
                }).ToList()
            };
        }

        public async Task<List<ReadingListDto>> GetPublicReadingListsAsync(int maxCount = 50)
        {
            var publicLists = await _readingListRepository.GetPublicReadingListsAsync(maxCount);
            
            return publicLists.Select(rl => new ReadingListDto
            {
                Id = rl.Id,
                Name = rl.Name,
                Description = rl.Description,
                IsPublic = rl.IsPublic,
                Color = rl.Color,
                SortOrder = rl.SortOrder,
                CreatedAt = rl.CreatedAt,
                UpdatedAt = rl.UpdatedAt,
                ArticleCount = rl.SavedArticles?.Count ?? 0,
                UnreadCount = rl.SavedArticles?.Count(a => !a.IsRead) ?? 0,
                OwnerName = rl.User?.UserName
            }).ToList();
        }

        public async Task<ReadingListDto> CreateReadingListAsync(CreateReadingListDto input)
        {
            var currentUserId = CurrentUser.GetId();
            
            // Check if user already has a list with this name
            var existingList = await _readingListRepository.GetUserReadingListByNameAsync(currentUserId, input.Name);
            if (existingList != null)
            {
                throw new UserFriendlyException($"You already have a reading list named '{input.Name}'.");
            }

            var readingList = new ReadingList(
                currentUserId,
                input.Name,
                input.Description,
                input.IsPublic,
                input.Color)
            {
                SortOrder = input.SortOrder
            };

            await _readingListRepository.InsertAsync(readingList, autoSave: true);

            return new ReadingListDto
            {
                Id = readingList.Id,
                Name = readingList.Name,
                Description = readingList.Description,
                IsPublic = readingList.IsPublic,
                Color = readingList.Color,
                SortOrder = readingList.SortOrder,
                CreatedAt = readingList.CreatedAt,
                UpdatedAt = readingList.UpdatedAt,
                ArticleCount = 0,
                UnreadCount = 0
            };
        }

        public async Task<ReadingListDto> UpdateReadingListAsync(Guid id, UpdateReadingListDto input)
        {
            var currentUserId = CurrentUser.GetId();
            var readingList = await _readingListRepository.GetAsync(id);

            if (readingList.UserId != currentUserId)
            {
                throw new UserFriendlyException("You can only edit your own reading lists.");
            }

            // Check if another list with the same name exists
            var existingList = await _readingListRepository.GetUserReadingListByNameAsync(currentUserId, input.Name);
            if (existingList != null && existingList.Id != id)
            {
                throw new UserFriendlyException($"You already have a reading list named '{input.Name}'.");
            }

            readingList.Name = input.Name;
            readingList.Description = input.Description;
            readingList.IsPublic = input.IsPublic;
            readingList.Color = input.Color;
            readingList.SortOrder = input.SortOrder;
            readingList.UpdatedAt = DateTime.UtcNow;

            await _readingListRepository.UpdateAsync(readingList, autoSave: true);

            return new ReadingListDto
            {
                Id = readingList.Id,
                Name = readingList.Name,
                Description = readingList.Description,
                IsPublic = readingList.IsPublic,
                Color = readingList.Color,
                SortOrder = readingList.SortOrder,
                CreatedAt = readingList.CreatedAt,
                UpdatedAt = readingList.UpdatedAt,
                ArticleCount = readingList.SavedArticles?.Count ?? 0,
                UnreadCount = readingList.SavedArticles?.Count(a => !a.IsRead) ?? 0
            };
        }

        public async Task DeleteReadingListAsync(Guid id)
        {
            var currentUserId = CurrentUser.GetId();
            var readingList = await _readingListRepository.GetAsync(id);

            if (readingList.UserId != currentUserId)
            {
                throw new UserFriendlyException("You can only delete your own reading lists.");
            }

            // Move any saved articles referencing this list to "Uncategorized" (null ReadingListId)
            // to avoid foreign key constraint violations when deleting the list.
            var articles = await _savedArticleRepository.GetArticlesByReadingListAsync(id);
            if (articles != null && articles.Count > 0)
            {
                // Update articles in-memory and defer saving for a single SaveChanges call to
                // avoid repeated DB round-trips and ensure atomicity within the current unit of work.
                foreach (var article in articles)
                {
                    article.ReadingListId = null;
                    await _savedArticleRepository.UpdateAsync(article, autoSave: false);
                }

                var uow = CurrentUnitOfWork;
                if (uow != null)
                {
                    await uow.SaveChangesAsync();
                }
            }

            // Delete the reading list (changes already flushed above)
            await _readingListRepository.DeleteAsync(readingList, autoSave: true);
        }

        public async Task ReorderReadingListsAsync(Dictionary<Guid, int> listOrders)
        {
            var currentUserId = CurrentUser.GetId();
            var readingLists = await _readingListRepository.GetUserReadingListsAsync(currentUserId);

            foreach (var readingList in readingLists)
            {
                if (listOrders.ContainsKey(readingList.Id))
                {
                    readingList.SortOrder = listOrders[readingList.Id];
                    readingList.UpdatedAt = DateTime.UtcNow;
                    await _readingListRepository.UpdateAsync(readingList);
                }
            }

            var uow2 = CurrentUnitOfWork;
            if (uow2 != null)
            {
                await uow2.SaveChangesAsync();
            }
        }
    }
}