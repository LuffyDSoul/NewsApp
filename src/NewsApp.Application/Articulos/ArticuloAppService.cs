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
    private readonly IRepository<Article, int> _repository;
    public NewAppService(IRepository<Article, int> repository)
    {
        _repository = repository;
    }

    public async Task<ICollection<ArticleDto>> GetArticulosAsync()
    {
        var articulo = await _repository.GetListAsync();

        return ObjectMapper.Map<ICollection<Article>, ICollection<ArticleDto>>(articulo);
    }

    public async Task<ArticleDto> GetArticuloAsync(int id)
    {
        var articulo = await _repository.GetAsync(id); // newE porque new es una palabra reservada

        return ObjectMapper.Map<Article, ArticleDto>(articulo);
    }

    public async Task InsertArticuloAsync(ArticleDto articulo)
    {
        var articuloMapped = ObjectMapper.Map<ArticleDto, Article>(articulo);

        await _repository.InsertAsync(articuloMapped);
    }

    public async Task UpdateArticuloAsync(ArticleDto articulo)
    {
        var articuloMapped = ObjectMapper.Map<ArticleDto, Article>(articulo);

        await _repository.UpdateAsync(articuloMapped);
    }

    public async Task RemoveArticuloAsync(ArticleDto articulo)
        {
        var articuloMapped = ObjectMapper.Map<ArticleDto, Article>(articulo);
        await _repository.DeleteAsync(articuloMapped);
    }
}
}
