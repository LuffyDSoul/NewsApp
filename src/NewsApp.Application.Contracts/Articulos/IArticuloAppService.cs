using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NewsApp.Articulos
{
        public interface IArticuloAppService : IApplicationService
        {
            Task<ICollection<ArticuloDto>> GetArticulosAsync();
            Task<ArticuloDto> GetArticuloAsync(int id);
            Task InsertArticuloAsync(ArticuloDto articulo);
            Task UpdateArticuloAsync(ArticuloDto articulo);
            Task RemoveArticuloAsync(ArticuloDto articulo);



        }
}
