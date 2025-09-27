using System;
using System.ComponentModel.DataAnnotations;
using Volo.Abp.Application.Dtos;

namespace NewsApp.Lists
{
    /// <summary>
    /// DTO for reading lists
    /// </summary>
    public class ReadingListDto : EntityDto<Guid>
    {
        /// <summary>
        /// Name of the reading list
        /// </summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Optional parent list ID for hierarchical organization
        /// </summary>
        public Guid? ParentId { get; set; }

        /// <summary>
        /// ID of the user who owns this list
        /// </summary>
        public Guid OwnerUserId { get; set; }

        /// <summary>
        /// Description of the reading list
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// Whether this list is publicly visible
        /// </summary>
        public bool IsPublic { get; set; }

        /// <summary>
        /// Number of items in this list
        /// </summary>
        public int ItemCount { get; set; }

        /// <summary>
        /// Number of unread items in this list
        /// </summary>
        public int UnreadCount { get; set; }

        /// <summary>
        /// When this list was created
        /// </summary>
        public DateTime CreationTime { get; set; }

        /// <summary>
        /// When this list was last modified
        /// </summary>
        public DateTime? LastModificationTime { get; set; }

        /// <summary>
        /// Name of the parent list (if applicable)
        /// </summary>
        public string? ParentName { get; set; }

        /// <summary>
        /// Owner's user name
        /// </summary>
        public string? OwnerUserName { get; set; }
    }

    /// <summary>
    /// DTO for creating reading lists
    /// </summary>
    public class CreateReadingListDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsPublic { get; set; } = false;
    }

    /// <summary>
    /// DTO for updating reading lists
    /// </summary>
    public class UpdateReadingListDto
    {
        [Required]
        [MaxLength(200)]
        public string Name { get; set; } = string.Empty;

        public Guid? ParentId { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        public bool IsPublic { get; set; }
    }

    /// <summary>
    /// DTO for reading list items
    /// </summary>
    public class ReadingListItemDto : EntityDto<Guid>
    {
        /// <summary>
        /// ID of the reading list this item belongs to
        /// </summary>
        public Guid ReadingListId { get; set; }

        /// <summary>
        /// ID of the referenced article
        /// </summary>
        public Guid ArticleId { get; set; }

        /// <summary>
        /// Snapshot of article title
        /// </summary>
        public string ArticleTitle { get; set; } = string.Empty;

        /// <summary>
        /// Snapshot of article URL
        /// </summary>
        public string ArticleUrl { get; set; } = string.Empty;

        /// <summary>
        /// Snapshot of article source
        /// </summary>
        public string ArticleSource { get; set; } = string.Empty;

        /// <summary>
        /// When the article was published
        /// </summary>
        public DateTime ArticlePublishedAt { get; set; }

        /// <summary>
        /// Whether this article has been read by the user
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// When this item was added to the reading list
        /// </summary>
        public DateTime AddedAt { get; set; }

        /// <summary>
        /// When this article was marked as read
        /// </summary>
        public DateTime? ReadAt { get; set; }

        /// <summary>
        /// User's personal notes about this article
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// User's rating for this article (1-5 stars)
        /// </summary>
        public int? Rating { get; set; }
    }

    /// <summary>
    /// DTO for adding articles to reading lists
    /// </summary>
    public class AddArticleToListDto
    {
        [Required]
        public Guid ArticleId { get; set; }

        public string? Notes { get; set; }
    }

    /// <summary>
    /// DTO for updating reading list items
    /// </summary>
    public class UpdateReadingListItemDto
    {
        public bool IsRead { get; set; }
        
        [MaxLength(2000)]
        public string? Notes { get; set; }

        [Range(1, 5)]
        public int? Rating { get; set; }
    }

    /// <summary>
    /// DTO for reading list with items
    /// </summary>
    public class ReadingListWithItemsDto : ReadingListDto
    {
        /// <summary>
        /// Items in this reading list
        /// </summary>
        public ReadingListItemDto[] Items { get; set; } = Array.Empty<ReadingListItemDto>();

        /// <summary>
        /// Child lists (for hierarchical display)
        /// </summary>
        public ReadingListDto[] Children { get; set; } = Array.Empty<ReadingListDto>();
    }

    /// <summary>
    /// DTO for hierarchical reading list structure
    /// </summary>
    public class ReadingListHierarchyDto : ReadingListDto
    {
        /// <summary>
        /// Depth level in the hierarchy (0 = root level)
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// Full path from root to this list
        /// </summary>
        public string[] Path { get; set; } = Array.Empty<string>();

        /// <summary>
        /// Child lists
        /// </summary>
        public ReadingListHierarchyDto[] Children { get; set; } = Array.Empty<ReadingListHierarchyDto>();
    }
}
