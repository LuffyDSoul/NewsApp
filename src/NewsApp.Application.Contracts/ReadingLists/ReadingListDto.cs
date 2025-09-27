using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace NewsApp.ReadingLists
{
    /// <summary>
    /// DTO for reading list information
    /// </summary>
    public class ReadingListDto : EntityDto<Guid>
    {
        /// <summary>
        /// Name of the reading list/topic
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of what this list is about
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether this list is public or private
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Color or icon identifier for UI purposes
        /// </summary>
        public string? Color { get; set; }

        /// <summary>
        /// Order for displaying lists
        /// </summary>
        public int SortOrder { get; set; }

        /// <summary>
        /// When this list was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When this list was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Number of articles in this list
        /// </summary>
        public int ArticleCount { get; set; }

        /// <summary>
        /// Number of unread articles in this list
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// Owner's name (for public lists)
        /// </summary>
        public string? OwnerName { get; set; }
    }

    /// <summary>
    /// DTO for creating a new reading list
    /// </summary>
    public class CreateReadingListDto
    {
        /// <summary>
        /// Name of the reading list/topic
        /// </summary>
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of what this list is about
        /// </summary>
        [StringLength(1024)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this list is public or private
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Color or icon identifier for UI purposes
        /// </summary>
        [StringLength(50)]
        public string? Color { get; set; }

        /// <summary>
        /// Order for displaying lists
        /// </summary>
        public int SortOrder { get; set; } = 0;
    }

    /// <summary>
    /// DTO for updating a reading list
    /// </summary>
    public class UpdateReadingListDto
    {
        /// <summary>
        /// Name of the reading list/topic
        /// </summary>
        [Required]
        [StringLength(256)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of what this list is about
        /// </summary>
        [StringLength(1024)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this list is public or private
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Color or icon identifier for UI purposes
        /// </summary>
        [StringLength(50)]
        public string? Color { get; set; }

        /// <summary>
        /// Order for displaying lists
        /// </summary>
        public int SortOrder { get; set; }
    }

    /// <summary>
    /// DTO for reading list with articles included
    /// </summary>
    public class ReadingListWithArticlesDto : ReadingListDto
    {
        /// <summary>
        /// Articles in this reading list
        /// </summary>
        public List<SavedArticleDto> Articles { get; set; } = new List<SavedArticleDto>();
    }
}