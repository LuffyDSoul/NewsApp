using AutoMapper.Internal.Mappers;
using NewsAPI.Models;
using NewsApp.Articulos;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Domain.Entities;

namespace NewsApp.Articulos
{
    public class NewAppService : NewsAppAppService, IArticuloAppService
    {
    private readonly IRepository<Articulo, int> _repository;
    public NewAppService(IRepository<Articulo, int> repository)
    {
        _repository = repository;
    }

    public async Task<ICollection<ArticuloDto>> GetArticulosAsync()
    {
        var articulo = await _repository.GetListAsync();

        return ObjectMapper.Map<ICollection<Articulo>, ICollection<ArticuloDto>>(articulo);
    }

    public async Task<ArticuloDto> GetArticuloAsync(int id)
    {
        var articulo = await _repository.GetAsync(id); // newE porque new es una palabra reservada

        return ObjectMapper.Map<Articulo, ArticuloDto>(articulo);
    }

    public async Task InsertArticuloAsync(ArticuloDto articulo)
    {
        var articuloMapped = ObjectMapper.Map<ArticuloDto, Articulo>(articulo);

        await _repository.InsertAsync(articuloMapped);
    }

    public async Task UpdateArticuloAsync(ArticuloDto articulo)
    {
        var articuloMapped = ObjectMapper.Map<ArticuloDto, Articulo>(articulo);

        await _repository.UpdateAsync(articuloMapped);
    }

    public async Task RemoveArticuloAsync(ArticuloDto articulo)
        {
        var articuloMapped = ObjectMapper.Map<ArticuloDto, Articulo>(articulo);
        await _repository.DeleteAsync(articuloMapped);
    }
}
}
