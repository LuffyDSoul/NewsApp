using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Volo.Abp.Domain.Entities;
using Volo.Abp.Identity;

namespace NewsApp.ReadingLists
{
    public class ReadingList : Entity<Guid>
    {
        /// <summary>
        /// The user who owns this reading list
        /// </summary>
        public Guid UserId { get; set; }
        
        /// <summary>
        /// Navigation property to User
        /// </summary>
        public virtual IdentityUser? User { get; set; }

        /// <summary>
        /// Name of the reading list/topic
        /// </summary>
        [Required]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// Description of what this list is about
        /// </summary>
        [MaxLength(1024)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this list is public or private
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Color or icon identifier for UI purposes
        /// </summary>
        [MaxLength(50)]
        public string? Color { get; set; }

        /// <summary>
        /// Order for displaying lists
        /// </summary>
        public int SortOrder { get; set; } = 0;

        /// <summary>
        /// When this list was created
        /// </summary>
        public DateTime CreatedAt { get; set; }

        /// <summary>
        /// When this list was last updated
        /// </summary>
        public DateTime? UpdatedAt { get; set; }

        /// <summary>
        /// Articles saved in this list
        /// </summary>
        public virtual ICollection<SavedArticle> SavedArticles { get; set; }

        public ReadingList()
        {
            SavedArticles = new HashSet<SavedArticle>();
            CreatedAt = DateTime.UtcNow;
        }

        public ReadingList(
            Guid userId,
            string name,
            string? description = null,
            bool isPublic = false,
            string? color = null)
        {
            UserId = userId;
            Name = name;
            Description = description;
            IsPublic = isPublic;
            Color = color;
            CreatedAt = DateTime.UtcNow;
            SavedArticles = new HashSet<SavedArticle>();
        }

        /// <summary>
        /// Gets the number of articles in this list
        /// </summary>
        public int ArticleCount => SavedArticles?.Count ?? 0;

        /// <summary>
        /// Gets the number of unread articles in this list
        /// </summary>
        public int UnreadCount => SavedArticles?.Count(a => !a.IsRead) ?? 0;
    }
}