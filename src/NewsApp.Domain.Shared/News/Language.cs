using System;
using System.Collections.Generic;

namespace NewsApp.News
{
    /// <summary>
    /// Value Object representing a language for news articles
    /// </summary>
    public class Language
    {
        public string Code { get; private set; } = string.Empty;
        public string Name { get; private set; } = string.Empty;

        public Language(string code, string name)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new ArgumentException("Language code cannot be empty", nameof(code));
            
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Language name cannot be empty", nameof(name));

            Code = code.ToLowerInvariant();
            Name = name;
        }

        // For EF Core
        private Language() { }

        public override bool Equals(object? obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;
            
            var other = (Language)obj;
            return Code == other.Code;
        }

        public override int GetHashCode()
        {
            return Code.GetHashCode();
        }

        public static Language English => new("en", "English");
        public static Language Spanish => new("es", "Spanish");
        public static Language French => new("fr", "French");
        public static Language German => new("de", "German");
        public static Language Portuguese => new("pt", "Portuguese");

        public override string ToString() => $"{Name} ({Code})";
    }
}
