using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NewsApp.Articulos;

namespace NewsApp.Busqueda
{
    public class Search
    {
        public string Keyword { get; set; }
        public ICollection<ArticleDto> Articulos { get; set; }

    }
}
