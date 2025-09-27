using System;
using System.Collections.Generic;
using System.Linq;

namespace NewsApp.Application
{
    /// <summary>
    /// Helper methods for mapping collections
    /// </summary>
    public static class MappingHelper
    {
        /// <summary>
        /// Converts a collection to DTOs using a mapping function
        /// </summary>
        public static List<TDto> ToDto<TEntity, TDto>(this IEnumerable<TEntity> entities, Func<TEntity, TDto> mapper)
        {
            return entities?.Select(mapper).ToList() ?? new List<TDto>();
        }

        /// <summary>
        /// Converts a collection to DTOs array using a mapping function
        /// </summary>
        public static TDto[] ToDto<TEntity, TDto>(this IEnumerable<TEntity> entities, Func<TEntity, TDto> mapper, bool asArray)
        {
            return entities?.Select(mapper).ToArray() ?? Array.Empty<TDto>();
        }

        /// <summary>
        /// Converts a list to DTOs using a mapping function
        /// </summary>
        public static List<TDto> ToDto<TEntity, TDto>(this List<TEntity> entities, Func<TEntity, TDto> mapper)
        {
            return entities?.Select(mapper).ToList() ?? new List<TDto>();
        }
    }
}