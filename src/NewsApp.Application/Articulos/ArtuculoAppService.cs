using NewsApp.Articulos;
using System;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;

namespace Acme.BookStore.Books;

public class BookAppService :
    CrudAppService<
        Articulo, //The Book entity
        ArticuloDto, //Used to show books
        Guid, //Primary key of the book entity
        PagedAndSortedResultRequestDto, //Used for paging/sorting
        IArticuloAppService //implement the IBookAppService
{
    public BookAppService(IRepository<Articulo, Guid> repository)
        : base(repository)
    {

    }
}
