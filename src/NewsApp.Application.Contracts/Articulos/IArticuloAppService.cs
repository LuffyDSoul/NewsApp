using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;

namespace NewsApp.Articulos;

public interface IArticuloAppService :
    ICrudAppService< //Defines CRUD methods
        ArticuloDto, //Used to show articles
        Guid, //Primary key of the article entity
        PagedAndSortedResultRequestDto //Used for paging/sorting
{

}