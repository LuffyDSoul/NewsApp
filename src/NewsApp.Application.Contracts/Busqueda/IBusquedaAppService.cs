using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace NewsApp.Busqueda
{
    public interface ISearchAppService
    {
        Task<Search> SearchArticulos(string keyword);
        /*Task SaveSearch(NewsListDto newsList, Search search);
        Task AddAlert(FolderDto folder, string keyword);
        Task AddNewInFolder(FolderDto folder, NewDto newE);
        Task AddNewListInFolder(FolderDto folder, NewsListDto newList);
        Task SearchWithDate(string keyword, AlertDto alert);*/
    }
}
