using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NewsApp.Articulos
{
        public interface IArticuloAppService : IApplicationService
        {
            Task<ICollection<ArticleDto>> GetArticulosAsync();
            Task<ArticleDto> GetArticuloAsync(int id);
            Task InsertArticuloAsync(ArticleDto articulo);
            Task UpdateArticuloAsync(ArticleDto articulo);
            Task RemoveArticuloAsync(ArticleDto articulo);



        }
}
