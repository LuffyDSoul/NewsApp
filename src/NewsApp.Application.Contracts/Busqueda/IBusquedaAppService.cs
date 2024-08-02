using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsApp.Articulos;
{

}

namespace NewsApp.Busqueda
{
    public interface ISearchAppService
    {
        Task<Search> SearchArticulo(string keyword);
        //Task SaveSearch(NewsListDto newsList, Search search);
        //Task AddAlert(FolderDto folder, string keyword);
        //Task AddNewInFolder(FolderDto folder, NewDto newE);
        //Task AddNewListInFolder(FolderDto folder, NewsListDto newList);
        //Task SearchWithDate(string keyword, AlertDto alert);
    }
}
