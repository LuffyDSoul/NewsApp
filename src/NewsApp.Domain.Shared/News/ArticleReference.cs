using System;
using System.Collections.Generic;

namespace NewsApp.News
{
    /// <summary>
    /// Value Object representing a reference to a news article
    /// </summary>
    public class ArticleReference
    {
        public string Url { get; private set; } = string.Empty;
        public string Title { get; private set; } = string.Empty;
        public string Source { get; private set; } = string.Empty;
        public DateTime PublishedAt { get; private set; }

        public ArticleReference(string url, string title, string source, DateTime publishedAt)
        {
            if (string.IsNullOrWhiteSpace(url))
                throw new ArgumentException("Article URL cannot be empty", nameof(url));
            
            if (string.IsNullOrWhiteSpace(title))
                throw new ArgumentException("Article title cannot be empty", nameof(title));

            if (string.IsNullOrWhiteSpace(source))
                throw new ArgumentException("Article source cannot be empty", nameof(source));

            Url = url;
            Title = title;
            Source = source;
            PublishedAt = publishedAt;
        }

        // For EF Core
        private ArticleReference() { }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            
            var other = (ArticleReference)obj;
            return Url == other.Url && 
                   Title == other.Title && 
                   Source == other.Source && 
                   PublishedAt == other.PublishedAt;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Url, Title, Source, PublishedAt);
        }

        public override string ToString() => $"{Title} - {Source}";
    }
}
