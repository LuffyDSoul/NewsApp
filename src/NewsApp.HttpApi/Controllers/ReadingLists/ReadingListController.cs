using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using NewsApp.ReadingLists;
using Volo.Abp.AspNetCore.Mvc;

namespace NewsApp.Controllers.ReadingLists
{
    [ApiController]
    [Route("api/reading-lists")]
    public class ReadingListController : AbpControllerBase
    {
        private readonly IReadingListAppService _readingListAppService;

        public ReadingListController(IReadingListAppService readingListAppService)
        {
            _readingListAppService = readingListAppService;
        }

        /// <summary>
        /// Gets all reading lists for the current user
        /// </summary>
        [HttpGet("my-lists")]
        public async Task<List<ReadingListDto>> GetMyReadingListsAsync()
        {
            return await _readingListAppService.GetMyReadingListsAsync();
        }

        /// <summary>
        /// Gets a specific reading list with articles
        /// </summary>
        [HttpGet("{id}/with-articles")]
        public async Task<ReadingListWithArticlesDto> GetReadingListWithArticlesAsync(Guid id)
        {
            return await _readingListAppService.GetReadingListWithArticlesAsync(id);
        }

        /// <summary>
        /// Gets public reading lists
        /// </summary>
        [HttpGet("public")]
        public async Task<List<ReadingListDto>> GetPublicReadingListsAsync([FromQuery] int maxCount = 50)
        {
            return await _readingListAppService.GetPublicReadingListsAsync(maxCount);
        }

        /// <summary>
        /// Creates a new reading list
        /// </summary>
        [HttpPost]
        public async Task<ReadingListDto> CreateReadingListAsync([FromBody] CreateReadingListDto input)
        {
            return await _readingListAppService.CreateReadingListAsync(input);
        }

        /// <summary>
        /// Updates an existing reading list
        /// </summary>
        [HttpPut("{id}")]
        public async Task<ReadingListDto> UpdateReadingListAsync(Guid id, [FromBody] UpdateReadingListDto input)
        {
            return await _readingListAppService.UpdateReadingListAsync(id, input);
        }

        /// <summary>
        /// Deletes a reading list
        /// </summary>
        [HttpDelete("{id}")]
        public async Task DeleteReadingListAsync(Guid id)
        {
            await _readingListAppService.DeleteReadingListAsync(id);
        }

        /// <summary>
        /// Reorders reading lists
        /// </summary>
        [HttpPost("reorder")]
        public async Task ReorderReadingListsAsync([FromBody] Dictionary<Guid, int> listOrders)
        {
            await _readingListAppService.ReorderReadingListsAsync(listOrders);
        }
    }
}