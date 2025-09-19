using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using Volo.Abp.Domain.Entities.Auditing;

namespace NewsApp.Domain.Lists
{
    /// <summary>
    /// Represents a reading list that can contain news articles.
    /// Supports hierarchical structure with parent-child relationships.
    /// </summary>
    public class ReadingList : AuditedAggregateRoot<Guid>
    {
        /// <summary>
        /// Name of the reading list
        /// </summary>
        [Required]
        [MaxLength(200)]
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
        [MaxLength(1000)]
        public string? Description { get; set; }

        /// <summary>
        /// Whether this list is publicly visible
        /// </summary>
        public bool IsPublic { get; set; } = false;

        /// <summary>
        /// Items in this reading list
        /// </summary>
        public virtual ICollection<ReadingListItem> Items { get; set; } = new List<ReadingListItem>();

        /// <summary>
        /// Child lists for hierarchical organization
        /// </summary>
        public virtual ICollection<ReadingList> Children { get; set; } = new List<ReadingList>();

        /// <summary>
        /// Parent list for hierarchical organization
        /// </summary>
        public virtual ReadingList? Parent { get; set; }

        protected ReadingList()
        {
            // For EF Core
        }

        public ReadingList(
            Guid id,
            string name,
            Guid ownerUserId,
            Guid? parentId = null,
            string? description = null,
            bool isPublic = false) : base(id)
        {
            SetBasicInfo(name, ownerUserId, parentId, description, isPublic);
        }

        public void SetBasicInfo(string name, Guid ownerUserId, Guid? parentId = null, string? description = null, bool isPublic = false)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("List name cannot be empty", nameof(name));

            if (ownerUserId == Guid.Empty)
                throw new ArgumentException("Owner user ID cannot be empty", nameof(ownerUserId));

            Name = name;
            OwnerUserId = ownerUserId;
            ParentId = parentId;
            Description = description;
            IsPublic = isPublic;
        }

        public ReadingListItem AddArticle(Guid articleId, string articleTitle, string articleUrl, string articleSource, DateTime publishedAt)
        {
            if (Items.Any(i => i.ArticleUrl == articleUrl))
                throw new InvalidOperationException("Article is already in this reading list");

            var item = new ReadingListItem(
                Guid.NewGuid(),
                Id,
                articleId,
                articleTitle,
                articleUrl,
                articleSource,
                publishedAt);

            Items.Add(item);
            return item;
        }

        public void RemoveArticle(Guid articleId)
        {
            var item = Items.FirstOrDefault(i => i.ArticleId == articleId);
            if (item != null)
            {
                Items.Remove(item);
            }
        }

        public void MarkArticleAsRead(Guid articleId, DateTime? readAt = null)
        {
            var item = Items.FirstOrDefault(i => i.ArticleId == articleId);
            if (item != null)
            {
                item.MarkAsRead(readAt ?? DateTime.UtcNow);
            }
        }

        public void MarkArticleAsUnread(Guid articleId)
        {
            var item = Items.FirstOrDefault(i => i.ArticleId == articleId);
            if (item != null)
            {
                item.MarkAsUnread();
            }
        }

        public int GetUnreadCount()
        {
            return Items.Count(i => !i.IsRead);
        }

        public bool CanBeParentOf(ReadingList potentialChild)
        {
            // Prevent circular references
            var current = this;
            while (current.Parent != null)
            {
                if (current.Parent.Id == potentialChild.Id)
                    return false;
                current = current.Parent;
            }
            return true;
        }
    }
}
